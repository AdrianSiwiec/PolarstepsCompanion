using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PolarstepsCompanion
{
    public partial class MainWindow
    {
        public class ImagePreviewClass
        {
            private string imagePreviewFilename;
            private string imagePreviewPath;
            private DateTime? imagePreviewDateTaken;
            private MainWindow mainWindow;

            public ImagePreviewClass(string rootDir, string path, MainWindow mainWindow)
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
            public String LocationMapsLink
            {
                get
                {
                    if (mainWindow?.polarstepsProcessor?.IsTripProcessed == true && ImagePreviewDateFixed.HasValue)
                    {
                        TimeSpan t = ImagePreviewDateFixed.Value - EpochStart;
                        DataPoint dp = mainWindow.polarstepsProcessor.GetLocation(Convert.ToInt32(t.TotalSeconds));
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
                imagePreviewDateTaken = PhotoProcessor.GetPhotoDateTaken(ImagePreviewPath);
                mainWindow.RaisePropertyChanged("ImagePreviewDateTaken");
                mainWindow.RaisePropertyChanged("ImagePreviewDateFixed");
            }

            internal void FinalProcess(bool renamePhotos, bool overwritePhotos, bool overwriteLocation, TimeSpan? timeSpanToShift, string outputDirectoryPath)
            {
                //Remember the one hour rule
            }
        }
    }
}
