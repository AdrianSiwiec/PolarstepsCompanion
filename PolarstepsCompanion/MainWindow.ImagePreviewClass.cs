using MetadataExtractor;
using System.Collections.Generic;

namespace PolarstepsCompanion
{
    public partial class MainWindow
    {
        class ImagePreviewClass
        {
            private string imagePreviewFilename;
            private string imagePreviewPath;
            private string imagePreviewDateTaken;

            public ImagePreviewClass(string rootDir, string path)
            {
                this.ImagePreviewFilename = path.Substring(rootDir.Length + 1);
                this.ImagePreviewPath = path;
                //this.metadataDirectories = ImageMetadataReader.ReadMetadata(path);
            }

            public string ImagePreviewFilename { get => imagePreviewFilename; set => imagePreviewFilename = value; }
            public string ImagePreviewPath { get => imagePreviewPath; set => imagePreviewPath = value; }
            public string ImagePreviewDateTaken { get => imagePreviewDateTaken; set => imagePreviewDateTaken = value; }
        }
    }
}
