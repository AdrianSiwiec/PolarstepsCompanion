using MetadataExtractor;
using ExifLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace PolarstepsCompanion
{
    public class PhotoProcessor
    {
        private const string EXIF_IFD0_DIR = "Exif IFD0";
        static private string[] PhotoExtensions = { ".JPG", ".JPEG" };

        private ObservableCollection<MainWindow.ImageClass> images;
        private BackgroundWorker preprocessingBackgroundWorker;
        private MainWindow mainWindow;

        private BackgroundWorker finalBackgroundWorker;
        private bool renamePhotos;
        private bool overwritePhotos;
        private bool overwriteLocation;
        private TimeSpan? timeSpanToShift;
        private string outputDirectoryPath;

        public PhotoProcessor(string directory, MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            Images = new ObservableCollection<MainWindow.ImageClass>();

            IEnumerable<string> imagePaths = System.IO.Directory.EnumerateFiles(directory, "*", System.IO.SearchOption.AllDirectories).
                    Where(f => PhotoExtensions.Contains(System.IO.Path.GetExtension(f), StringComparer.InvariantCultureIgnoreCase));

            foreach (string image in imagePaths)
            {
                Images.Add(new MainWindow.ImageClass(directory, image, mainWindow));
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



        private void PreprocessingBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.PhotosLoadedPreprocessingBar.Value = 100;
            mainWindow.PhotosLoadedPreprocessingText.Text = "Done!";

            mainWindow.RaisePropertyChanged("ImagePreviewDateTaken");
            mainWindow.DateTakenColumn.Visibility = System.Windows.Visibility.Visible;
            mainWindow.ImagePreviewDataGrid.Items.Refresh();
            mainWindow.SelectPhotosDir.IsEnabled = true;

            IsPreprocessingDone = true;
            mainWindow.UpdateFinalProcessingReadyStatus();
        }

        private void PreprocessingBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mainWindow.PhotosLoadedPreprocessingBar.Value = e.ProgressPercentage;
            mainWindow.PhotosLoadedPreprocessingText.Text = e.ProgressPercentage + "%";
        }

        private void PreprocessingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (images != null)
            {
                Queue<Task> tasks = new Queue<Task>();
                int counter = 0;
                for (int i = 0; i < images.Count; i++)
                {
                    while (tasks.Count >= 5)
                    {
                        tasks.Dequeue().Wait();
                        counter++;
                        preprocessingBackgroundWorker.ReportProgress(((counter * 100 + 1)) / images.Count);
                    }
                    tasks.Enqueue(images[i].Preprocess());
                }
            }
        }

        public ObservableCollection<MainWindow.ImageClass> Images { get => images; set => images = value; }
        public bool IsPreprocessingDone { get => isPreprocessingDone; private set => isPreprocessingDone = value; }

        private bool isPreprocessingDone = false;




        internal void DoFinalProcessing(bool? renamePhotos, bool? overwritePhotos, bool? overwriteLocation, TimeSpan? timeSpanToShift, string outputDirectoryPath)
        {
            this.renamePhotos = renamePhotos == true;
            this.overwritePhotos = overwritePhotos == true;
            this.overwriteLocation = overwriteLocation == true;
            this.timeSpanToShift = timeSpanToShift;
            this.outputDirectoryPath = outputDirectoryPath;

            finalBackgroundWorker.RunWorkerAsync();
        }

        private void FinalBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (images != null)
            {
                Queue<Task> tasks = new Queue<Task>();
                int counter = 0;
                for (int i = 0; i < images.Count; i++)
                {
                    while (tasks.Count >= 5)
                    {
                        tasks.Dequeue().Wait();
                        counter++;
                        finalBackgroundWorker.ReportProgress(((counter * 100 + 1)) / images.Count);
                    }
                    tasks.Enqueue(images[i].FinalProcess(renamePhotos, overwritePhotos, overwriteLocation, timeSpanToShift, outputDirectoryPath));
                }
            }
        }

        private void FinalBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mainWindow.FinalProcessingProgressBar.Value = 100;
            mainWindow.FinalProcessingProgressBarText.Text = "All Done!";
        }

        private void FinalBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mainWindow.FinalProcessingProgressBar.Value = e.ProgressPercentage;
            mainWindow.FinalProcessingProgressBarText.Text = e.ProgressPercentage + "%";
        }

    }
}
