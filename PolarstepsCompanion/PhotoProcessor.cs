using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace PolarstepsCompanion
{
    public static class PhotoProcessor
    {
        private const string EXIF_IFD0_DIR = "Exif IFD0";

        public static DateTime? GetPhotoDateTaken(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Bad photo path.", nameof(path));
            }

            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(path);
            foreach (Directory dir in directories)
            {
                if (String.Compare(dir.Name, EXIF_IFD0_DIR, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    foreach (Tag tag in dir.Tags)
                    {
                        if (String.Compare(tag.Name, "Date/Time", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            if (DateTime.TryParseExact(tag.Description, "yyyy:MM:dd HH:mm:ss",
                            CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dateTaken))
                            {
                                return dateTaken;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
