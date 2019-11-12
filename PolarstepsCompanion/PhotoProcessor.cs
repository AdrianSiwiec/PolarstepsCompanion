using MetadataExtractor;
using ExifLibrary;
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
        private BackgroundWorker preprocessingBackgroundWorker;
        private BackgroundWorker finalBackgroundWorker;
        private MainWindow mainWindow;

        public PhotoProcessor(string directory, MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            Images = new ObservableCollection<MainWindow.ImagePreviewClass>();

            IEnumerable<string> imagePaths = System.IO.Directory.EnumerateFiles(directory, "*", System.IO.SearchOption.AllDirectories).
                    Where(f => PhotoExtensions.Contains(System.IO.Path.GetExtension(f), StringComparer.InvariantCultureIgnoreCase));

            foreach (string image in imagePaths)
            {
                Images.Add(new MainWindow.ImagePreviewClass(directory, image, mainWindow));
            }

            preprocessingBackgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            preprocessingBackgroundWorker.DoWork += PreprocessingBackgroundWorker_DoWork;
            preprocessingBackgroundWorker.ProgressChanged += PreprocessingBackgroundWorker_ProgressChanged;
            preprocessingBackgroundWorker.RunWorkerCompleted += PreprocessingBackgroundWorker_RunWorkerCompleted;
            preprocessingBackgroundWorker.RunWorkerAsync();

            finalBackgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            finalBackgroundWorker.DoWork += FinalBackgroundWorker_DoWork;
            finalBackgroundWorker.ProgressChanged += FinalBackgroundWorker_ProgressChanged;
            finalBackgroundWorker.RunWorkerCompleted += FinalBackgroundWorker_RunWorkerCompleted;
        }

        private void FinalBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void FinalBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void FinalBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void PreprocessingBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.PhotosLoadedPreprocessingBar.Value = 100;
            mainWindow.PhotosLoadedPreprocessingText.Text = "Done!";

            mainWindow.RaisePropertyChanged("ImagePreviewDateTaken");
            mainWindow.DateTakenColumn.Visibility = System.Windows.Visibility.Visible;
            mainWindow.ImagePreviewDataGrid.Items.Refresh();
            mainWindow.SelectPhotosDir.IsEnabled = true;

            IsPreprocessingDone = true;
        }

        private void PreprocessingBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Trace.WriteLine("Progress: " + e.ProgressPercentage);
            mainWindow.PhotosLoadedPreprocessingBar.Value = e.ProgressPercentage;
            mainWindow.PhotosLoadedPreprocessingText.Text = e.ProgressPercentage + "%";
        }

        private void PreprocessingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (images != null)
            {
                for (int i = 0; i < images.Count; i++)
                {
                    images[i].Preprocess();
                    preprocessingBackgroundWorker.ReportProgress(((i * 100 + 1)) / images.Count);
                }

            }
        }

        public ObservableCollection<MainWindow.ImagePreviewClass> Images { get => images; set => images = value; }
        public bool IsPreprocessingDone { get => isPreprocessingDone; private set => isPreprocessingDone = value; }

        private bool isPreprocessingDone = false;

        public static DateTime? GetPhotoDateTaken(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Bad photo path.", nameof(path));
            }

            ImageFile file = ImageFile.FromFile(path);
            ExifDateTime dateTime = file.Properties.Get<ExifDateTime>(ExifTag.DateTime);

            return dateTime;
        }

        public static void SaveFile(string inputDir, string inputFilename, bool changeName, bool overwrite, string outputDir, DataPoint dataPoint)
        {
            throw new NotImplementedException("Save file not implemented");
        }

        internal void DoFinalProcessing(bool? isChecked1, bool? isChecked2, string outputDirectoryPath)
        {
            throw new NotImplementedException();
        }
    }
}
