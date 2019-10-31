using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace PolarstepsCompanion
{
    public class PhotoProcessor
    {
        private const string EXIF_IFD0_DIR = "Exif IFD0";
        static private string[] PhotoExtensions = { ".JPG", ".JPEG", ".NEF" };

        private ObservableCollection<MainWindow.ImagePreviewClass> images;
        private BackgroundWorker backgroundWorker;
        private MainWindow mainWindow;

        public PhotoProcessor(string directory, MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            Images = new ObservableCollection<MainWindow.ImagePreviewClass>();

            IEnumerable<string> imagePaths = System.IO.Directory.EnumerateFiles(directory, "*", System.IO.SearchOption.AllDirectories).
                    Where(f => PhotoExtensions.Contains(System.IO.Path.GetExtension(f), StringComparer.InvariantCultureIgnoreCase));

            foreach (string image in imagePaths)
            {
                Images.Add(new MainWindow.ImagePreviewClass(directory, image));
            }

            backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.PhotosLoadedPreprocessingBar.Value = 100;
            mainWindow.PhotosLoadedPreprocessingText.Text = "Done!";

            mainWindow.RaisePropertyChanged("ImagePreviewDateTaken");
            mainWindow.DateTakenColumn.Visibility = System.Windows.Visibility.Visible;

        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Trace.WriteLine("Progress: " + e.ProgressPercentage);
            mainWindow.PhotosLoadedPreprocessingBar.Value = e.ProgressPercentage;
            mainWindow.PhotosLoadedPreprocessingText.Text = e.ProgressPercentage + "%";
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (images != null)
            {
                for (int i = 0; i < images.Count; i++)
                {
                    images[i].Process();
                    backgroundWorker.ReportProgress(((i * 100 + 1)) / images.Count);
                }

                Trace.WriteLine("DONE WORKER " + images.Count);
            }
        }

        public ObservableCollection<MainWindow.ImagePreviewClass> Images { get => images; set => images = value; }

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
