using ExifLibrary;
using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

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
                this.ImagePreviewRelativePath = path.Substring(rootDir.Length + 1);
                this.ImagePreviewFullPath = path;
                this.mainWindow = mainWindow;
                //this.metadataDirectories = ImageMetadataReader.ReadMetadata(path);
            }

            public string ImagePreviewRelativePath { get => imagePreviewFilename; set => imagePreviewFilename = value; }
            public string ImagePreviewFullPath { get => imagePreviewPath; set => imagePreviewPath = value; }
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



            internal async Task Preprocess()
            {
                imagePreviewDateTaken = await GetPhotoDateTaken(ImagePreviewFullPath);
                mainWindow.RaisePropertyChanged("ImagePreviewDateTaken");
                mainWindow.RaisePropertyChanged("ImagePreviewDateFixed");
            }

            internal async Task FinalProcess(bool renamePhotos, bool overwritePhotos, bool overwriteLocation, TimeSpan? timeSpanToShift, string outputDirectoryPath)
            {
                try
                {
                    string dirToSave = "";
                    if (overwritePhotos)
                        dirToSave = Path.GetDirectoryName(ImagePreviewFullPath);
                    else
                    {
                        dirToSave = Path.GetDirectoryName(Path.Combine(outputDirectoryPath, ImagePreviewRelativePath));
                    }

                    ImageFile imageFile = await ImageFile.FromFileAsync(ImagePreviewFullPath);

                    string filenameToSave = Path.GetFileName(ImagePreviewRelativePath);

                    if (renamePhotos)
                        filenameToSave = GetFilenamePrefix(imageFile, ImagePreviewDateFixed) + filenameToSave;

                    bool saveImage = false;
                    if (timeSpanToShift != null)
                    {
                        imageFile.Properties.Set(ExifTag.DateTime, ImagePreviewDateFixed.Value);
                        imageFile.Properties.Set(ExifTag.DateTimeOriginal, ImagePreviewDateFixed.Value);

                        saveImage = true;
                    }

                    DataPoint location = ImageLocation;
                    if (location != null && (overwriteLocation || !imageFile.Properties.Contains(ExifTag.GPSLatitude)))
                    {
                        float deg = 0, min = 0, sec = 0;
                        GetDegMinSec(location.Lat, ref deg, ref min, ref sec);
                        imageFile.Properties.Set(ExifTag.GPSLatitude, deg, min, sec);
                        //imageFile.Properties.Set(ExifTag.GPSLatitude, Math.Abs(location.Lat));
                        imageFile.Properties.Set(ExifTag.GPSLatitudeRef,
                            location.Lat > 0 ? GPSLatitudeRef.North : GPSLatitudeRef.South);

                        GetDegMinSec(location.Lon, ref deg, ref min, ref sec);
                        imageFile.Properties.Set(ExifTag.GPSLongitude, deg, min, sec);
                        //imageFile.Properties.Set(ExifTag.GPSLongitude, Math.Abs(location.Lon));
                        imageFile.Properties.Set(ExifTag.GPSLongitudeRef,
                            location.Lon > 0 ? GPSLongitudeRef.East : GPSLongitudeRef.West);

                        imageFile.Properties.Set(ExifTag.GPSDateStamp, ImagePreviewDateFixed.Value);
                        //imageFile.Properties.Set(ExifTag.GPSMeasureMode, "2");

                        saveImage = true;
                    }


                    string pathToSave = Path.Combine(dirToSave, filenameToSave);
                    string fullDirToSave = Path.GetDirectoryName(pathToSave);
                    System.IO.Directory.CreateDirectory(fullDirToSave);

                    if (saveImage)
                    {
                        await imageFile.SaveAsync(pathToSave);
                    }
                    else
                    {
                        if (overwritePhotos)
                        {
                            File.Move(ImagePreviewFullPath, pathToSave);
                        }
                        else
                        {
                            File.Copy(ImagePreviewFullPath, pathToSave);
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show($"Error saving file \"{ImagePreviewRelativePath}\"\n" + e.ToString());
                }
            }

            private void GetDegMinSec(double lon_lat, ref float deg, ref float min, ref float sec)
            {
                lon_lat = Math.Abs(lon_lat);

                deg = (int)lon_lat;
                lon_lat -= deg;
                lon_lat *= 60;

                min = (int)lon_lat;
                lon_lat -= min;
                lon_lat *= 60;

                sec = Convert.ToSingle(lon_lat);
            }

            private string GetFilenamePrefix(ImageFile imageFile, DateTime? imagePreviewDateFixed)
            {
                DateTime? dateTime = imagePreviewDateFixed;
                if (dateTime == null || dateTime == DateTime.MinValue)
                    return "NA";

                return dateTime.Value.ToString("yyyy-MM-dd;HH.mm.ss.");
            }

            public static async Task<DateTime?> GetPhotoDateTaken(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentException("Bad photo path.", nameof(path));
                }

                ImageFile file = await ImageFile.FromFileAsync(path);
                ExifDateTime dateTime = file.Properties.Get<ExifDateTime>(ExifTag.DateTimeOriginal);

                if (dateTime == null || dateTime.Value <= DateTime.MinValue)
                    dateTime = file.Properties.Get<ExifDateTime>(ExifTag.DateTime);

                if (dateTime == null)
                    return null;
                else
                    return dateTime.Value;
            }
        }
    }
}
