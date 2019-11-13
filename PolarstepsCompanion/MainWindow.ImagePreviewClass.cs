using ExifLibrary;
using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PolarstepsCompanion
{
    public partial class MainWindow
    {
        public class ImageClass
        {
            private string imagePreviewFilename;
            private string imagePreviewPath; // A full path, with dir and filename
            private DateTime? imagePreviewDateTaken;
            private MainWindow mainWindow;

            public ImageClass(string rootDir, string path, MainWindow mainWindow)
            {
                this.ImagePreviewFilename = path.Substring(rootDir.Length + 1);
                this.ImagePreviewPath = path;
                this.mainWindow = mainWindow;
                //this.metadataDirectories = ImageMetadataReader.ReadMetadata(path);
            }

            public string ImagePreviewFilename { get => imagePreviewFilename; set => imagePreviewFilename = value; }
            public string ImagePreviewPath { get => imagePreviewPath; set => imagePreviewPath = value; }
            public String ImagePreviewDateTaken
            {
                get
                {
                    if (imagePreviewDateTaken != null)
                        return imagePreviewDateTaken.ToString();
                    else
                        return "Date/Time not available";
                }

                private set
                {
                    //imagePreviewDateTaken = value;

                }
            }

            public DateTime? ImagePreviewDateFixed
            {
                get
                {
                    if (mainWindow == null || mainWindow.FixTimeTimeSpan == null)
                        return imagePreviewDateTaken;

                    return imagePreviewDateTaken + mainWindow.FixTimeTimeSpan;
                }
            }

            private static readonly DateTime EpochStart = new DateTime(1970, 1, 1);
            public DataPoint? ImageLocation
            {
                get
                {
                    if (mainWindow?.polarstepsProcessor?.IsTripProcessed == true && ImagePreviewDateFixed != null)
                    {
                        TimeSpan t = ImagePreviewDateFixed.Value - EpochStart;
                        return mainWindow.polarstepsProcessor.GetLocation(Convert.ToInt32(t.TotalSeconds));
                    }

                    return null;
                }
            }

            public String LocationMapsLink
            {
                get
                {
                    if (mainWindow?.polarstepsProcessor?.IsTripProcessed == true && ImagePreviewDateFixed.HasValue)
                    {
                        DataPoint dp = ImageLocation;
                        if (dp == null)
                            return "";
                        else
                            return $"https://www.openstreetmap.org/?mlat={dp.Lat}&mlon={dp.Lon}#map=12";
                    }
                    else
                        return "";
                }
            }

            public String ClickText
            {
                get
                {
                    if (String.IsNullOrWhiteSpace(LocationMapsLink))
                        return "Not available";
                    else
                        return "Click";
                }
            }



            internal void Preprocess()
            {
                imagePreviewDateTaken = GetPhotoDateTaken(ImagePreviewPath);
                mainWindow.RaisePropertyChanged("ImagePreviewDateTaken");
                mainWindow.RaisePropertyChanged("ImagePreviewDateFixed");
            }

            internal void FinalProcess(bool renamePhotos, bool overwritePhotos, bool overwriteLocation, TimeSpan? timeSpanToShift, string outputDirectoryPath)
            {
                //try
                //{
                string dirToSave = "";
                if (overwritePhotos)
                    dirToSave = ImagePreviewPath.Substring(0, ImagePreviewPath.Length - ImagePreviewFilename.Length - 1);
                else
                {
                    dirToSave = outputDirectoryPath;
                    System.IO.Directory.CreateDirectory(dirToSave);
                }

                ImageFile imageFile = ImageFile.FromFile(ImagePreviewPath);

                string filenameToSave = "";
                if (renamePhotos)
                    filenameToSave = GetFilenamePrefix(imageFile) + ImagePreviewFilename;
                else
                    filenameToSave = ImagePreviewFilename;


                DataPoint location = ImageLocation;
                if (location != null && (overwriteLocation || !imageFile.Properties.Contains(ExifTag.GPSLatitude)))
                {
                    //imageFile.Properties.Set(ExifTag.GPSLatitude, new GPSLatitudeLongitude(ExifTag.GPSLatitude,
                    //new[] { new MathEx.UFraction32(Math.Abs(location.Lat)), new MathEx.UFraction32(0), new MathEx.UFraction32(0) }));
                    imageFile.Properties.Set(ExifTag.GPSLatitude, Math.Abs(location.Lat));
                    imageFile.Properties.Set(ExifTag.GPSLatitudeRef,
                        location.Lat > 0 ? GPSLatitudeRef.North : GPSLatitudeRef.South);

                    //imageFile.Properties.Set(ExifTag.GPSLongitude, new GPSLatitudeLongitude(ExifTag.GPSLongitude,
                    //new[] { new MathEx.UFraction32(Math.Abs(location.Lon)), new MathEx.UFraction32(0), new MathEx.UFraction32(0) }));
                    imageFile.Properties.Set(ExifTag.GPSLongitude, Math.Abs(location.Lon));
                    imageFile.Properties.Set(ExifTag.GPSLongitudeRef,
                        location.Lon > 0 ? GPSLongitudeRef.East : GPSLongitudeRef.West);

                    imageFile.Properties.Set(ExifTag.GPSDateStamp, ImagePreviewDateFixed);
                    imageFile.Properties.Set(ExifTag.DateTime, ImagePreviewDateFixed);

                    imageFile.Save(Path.Combine(dirToSave, filenameToSave));
                }
                else
                {
                    if (overwritePhotos)
                    {
                        File.Move(ImagePreviewPath, Path.Combine(dirToSave, filenameToSave));
                    }
                    else
                    {
                        File.Copy(ImagePreviewPath, Path.Combine(dirToSave, filenameToSave));
                    }
                }
                //}
                //catch (Exception e)
                //{
                //    System.Windows.MessageBox.Show($"Error saving file \"{ImagePreviewFilename}\"\n" + e.ToString());
                //}
            }



            private string GetFilenamePrefix(ImageFile imageFile)
            {
                ExifDateTime exifDateTime = (ExifDateTime)imageFile.Properties.Get(ExifTag.DateTime);
                if (exifDateTime == null)
                    return "";

                DateTime dateTime = exifDateTime;
                if (dateTime == null || dateTime == DateTime.MinValue)
                    return "";

                return dateTime.ToString("yyyyMMdd.HHmmss.");
            }

            public static DateTime? GetPhotoDateTaken(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentException("Bad photo path.", nameof(path));
                }

                ImageFile file = ImageFile.FromFile(path);
                ExifDateTime dateTime = file.Properties.Get<ExifDateTime>(ExifTag.DateTime);

                if (dateTime == null)
                    return null;
                else
                    return dateTime.Value;

            }
        }
    }
}
