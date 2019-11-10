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
            public DateTime? ImagePreviewDateTaken
            {
                get => imagePreviewDateTaken; set
                {
                    imagePreviewDateTaken = value;
                    mainWindow.RaisePropertyChanged("ImagePreviewDateTaken");
                    mainWindow.RaisePropertyChanged("ImagePreviewDateFixed");
                }
            }

            public DateTime? ImagePreviewDateFixed
            {
                get
                {
                    if (mainWindow == null || mainWindow.FixTimeTimeSpan == null)
                        return ImagePreviewDateTaken;

                    return ImagePreviewDateTaken + mainWindow.FixTimeTimeSpan;
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
                        return $"https://www.google.com/maps/search/?api=1&map_action=map&query={dp.Lat},{dp.Lon}";
                    }
                    return "https://www.google.com";
                }
            }

            public String ClickText
            {
                get
                {
                    return "Click";
                }
            }


            internal void Preprocess()
            {
                ImagePreviewDateTaken = PhotoProcessor.GetPhotoDateTaken(ImagePreviewPath);
            }
        }
    }
}
