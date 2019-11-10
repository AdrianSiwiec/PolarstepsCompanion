﻿using GalaSoft.MvvmLight.Messaging;
using MetadataExtractor;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PolarstepsCompanion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        static public readonly string PhotoExtensionsString = "Photos|*.jpg;*.jpeg;*.nef";

        private PolarstepsProcessor polarstepsProcessor;
        public MainWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<String>(this, (action) => ReceiveMessage(action));
            this.DataContext = this;
            this.PolarstepsCBItems = new ObservableCollection<ComboBoxItem>();
        }

        private void ReceiveMessage(string message)
        {

        }


        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(fileDialog.FileName);

                TextBlockContent = "";
                foreach (Directory directory in directories)
                {
                    foreach (Tag tag in directory.Tags)
                    {
                        TextBlockContent += tag.ToString();
                        TextBlockContent += "\n";
                    }
                    TextBlockContent += "\n";
                }

            }
            // For future use
            //CommonOpenFileDialog common = new CommonOpenFileDialog();
            //common.IsFolderPicker = true;
            //if (common.ShowDialog() == CommonFileDialogResult.Ok)
            //{
            //    Trace.WriteLine(common.FileName);
            //}
        }
        private string textBlockContent = "Empty so far...";
        public string TextBlockContent
        {
            get { return textBlockContent; }
            set
            {
                textBlockContent = value;
                RaisePropertyChanged("TextBlockContent");
            }
        }

        // Photos Tab

        private string photosPath;
        public string PhotosPath
        {
            get => photosPath;
            set
            {
                photosPath = value;
                RaisePropertyChanged("PhotosPath");
            }
        }

        private void SelectPhotosDir_Click(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                EnsurePathExists = true,
                Title = "Select The Folder To Process"
            };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SelectPhotosDir.IsEnabled = false;
                PhotosPath = fileDialog.FileName;

                PhotoProcessor processor = new PhotoProcessor(fileDialog.FileName, this);

                ImagePreviewDataGrid.ItemsSource = processor.Images;
                PreviewDataGrid.ItemsSource = processor.Images;

                PhotosLoadedInfo.Text = $"{processor.Images.Count} photos loaded succesfully.";
                RaisePropertyChanged("PhotosLoadedInfo");
            }

            ValidateOutputDirectory();
        }

        private void ImagePreviewDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ImagePreviewDataGrid.UnselectAllCells();
            PreviewDataGrid.UnselectAllCells();
        }

        //Fix Time Tab

        private string fixTimeCameraPhotoPath;
        public string FixTimeCameraPhotoPath
        {
            get => fixTimeCameraPhotoPath;
            set
            {
                fixTimeCameraPhotoPath = value;
                RaisePropertyChanged("FixTimeCameraPhotoPath");
            }
        }

        private string fixTimeCameraFilename;
        public string FixTimeCameraFilename
        {
            get => fixTimeCameraFilename;
            set
            {
                fixTimeCameraFilename = value;
                RaisePropertyChanged("FixTimeCameraFilename");
            }
        }

        private DateTime? fixTimeCameraDateTaken;
        public DateTime? FixTimeCameraDateTaken
        {
            get => fixTimeCameraDateTaken; set
            {
                fixTimeCameraDateTaken = value;
                FixTimeCameraDateTakenString = "Date taken: " + value;
            }
        }

        public string FixTimeCameraDateTakenString
        {
            get => fixTimeCameraDateTakenString; set
            {
                fixTimeCameraDateTakenString = value;
                RaisePropertyChanged("FixTimeCameraDateTakenString");
            }
        }

        private string fixTimeCameraDateTakenString;


        private string fixTimePhotoPhotoPath;
        public string FixTimePhotoPhotoPath
        {
            get => fixTimePhotoPhotoPath;
            set
            {
                fixTimePhotoPhotoPath = value;
                RaisePropertyChanged("FixTimePhotoPhotoPath");
            }
        }

        private string fixTimePhotoFilename;
        public string FixTimePhotoFilename
        {
            get => fixTimePhotoFilename;
            set
            {
                fixTimePhotoFilename = value;
                RaisePropertyChanged("FixTimePhotoFilename");
            }
        }

        private DateTime? fixTimePhotoDateTaken;
        public DateTime? FixTimePhotoDateTaken
        {
            get => fixTimePhotoDateTaken; set
            {
                fixTimePhotoDateTaken = value;
                FixTimePhotoDateTakenString = "Date taken: " + value;
            }
        }

        public string FixTimePhotoDateTakenString
        {
            get => fixTimePhotoDateTakenString; set
            {
                fixTimePhotoDateTakenString = value;
                RaisePropertyChanged("FixTimePhotoDateTakenString");
            }
        }


        private string fixTimePhotoDateTakenString;

        private void FixTimeCameraBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = PhotoExtensionsString
            };
            if (fileDialog.ShowDialog() == true)
            {
                FixTimeCameraPhotoPath = fileDialog.FileName;
                FixTimeCameraFilename = System.IO.Path.GetFileName(fileDialog.FileName);
                FixTimeCameraDateTaken = PhotoProcessor.GetPhotoDateTaken(fileDialog.FileName);
                UpdateTimeSpanCamera();
            }
        }

        private void FixTimePhotoBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = PhotoExtensionsString
            };
            if (fileDialog.ShowDialog() == true)
            {
                FixTimePhotoPhotoPath = fileDialog.FileName;
                FixTimePhotoFilename = System.IO.Path.GetFileName(fileDialog.FileName);
                FixTimePhotoDateTaken = PhotoProcessor.GetPhotoDateTaken(fileDialog.FileName);
                UpdateTimeSpanCamera();
            }
        }

        private void UpdateTimeSpanCamera()
        {
            if (FixTimeCameraDateTaken != null && FixTimePhotoDateTaken != null)
                FixTimeTimeSpan = FixTimePhotoDateTaken - FixTimeCameraDateTaken;
            else
                FixTimeTimeSpan = null;
        }

        public enum FixTime
        {
            None,
            Photos,
            Manual
        };

        private FixTime fixTimeOption = FixTime.None;
        public FixTime FixTimeOption
        {
            get => fixTimeOption; set
            {
                fixTimeOption = value;

                if (FixTimePhotosGrid != null)
                    if (value == FixTime.Photos) FixTimePhotosGrid.Visibility = Visibility.Visible;
                    else FixTimePhotosGrid.Visibility = Visibility.Collapsed;

                if (FixTimeManualGrid != null)
                    if (value == FixTime.Manual) FixTimeManualGrid.Visibility = Visibility.Visible;
                    else FixTimeManualGrid.Visibility = Visibility.Collapsed;

                RaisePropertyChanged("FixTimePhotosGrid");
                RaisePropertyChanged("FixTimeManualGrid");

            }
        }



        private void FixTimeCameraButton_Checked(object sender, RoutedEventArgs e)
        {
            FixTimeOption = FixTime.Photos;
            UpdateTimeSpanCamera();
        }

        private void FixTimeNoButton_Checked(object sender, RoutedEventArgs e)
        {
            FixTimeOption = FixTime.None;
            FixTimeTimeSpan = null;
        }

        private void FixTimeManualButton_Checked(object sender, RoutedEventArgs e)
        {
            FixTimeOption = FixTime.Manual;
            UpdateTimeSpanManual();
        }

        private string fixTimeManualPhotoPath;
        public string FixTimeManualPhotoPath
        {
            get => fixTimeManualPhotoPath;
            set
            {
                fixTimeManualPhotoPath = value;
                RaisePropertyChanged("FixTimeManualPhotoPath");
            }
        }

        private string fixTimeManualFilename;
        public string FixTimeManualFilename
        {
            get => fixTimeManualFilename;
            set
            {
                fixTimeManualFilename = value;
                RaisePropertyChanged("FixTimeManualFilename");
            }
        }

        private DateTime? fixTimeManualDateTaken = new DateTime(1451123);
        public DateTime? FixTimeManualDateTaken
        {
            get => fixTimeManualDateTaken; set
            {
                fixTimeManualDateTaken = value;
                FixTimeManualDateTakenString = "Date taken: " + value;
            }
        }

        public string FixTimeManualDateTakenString
        {
            get => fixTimeManualDateTakenString; set
            {
                fixTimeManualDateTakenString = value;
                RaisePropertyChanged("FixTimeManualDateTakenString");
            }
        }


        private string fixTimeManualDateTakenString;

        private void FixTimeManualBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = PhotoExtensionsString
            };
            if (fileDialog.ShowDialog() == true)
            {
                FixTimeManualPhotoPath = fileDialog.FileName;
                FixTimeManualFilename = System.IO.Path.GetFileName(fileDialog.FileName);
                FixTimeManualDateTaken = PhotoProcessor.GetPhotoDateTaken(fileDialog.FileName);

                FixTimeManualDateTime.Value = FixTimeManualDateTaken;

                UpdateTimeSpanManual();
            }
        }

        private void FixTimeManualDateTime_ValueChanged(object sender, RoutedEventArgs e)
        {
            UpdateTimeSpanManual();
        }

        private void UpdateTimeSpanManual()
        {
            if (FixTimeManualDateTime.Value != null && FixTimeManualDateTaken != null)
                FixTimeTimeSpan = FixTimeManualDateTime.Value - FixTimeManualDateTaken;
            else
                FixTimeTimeSpan = null;
        }

        private TimeSpan? fixTimeTimeSpan;
        public TimeSpan? FixTimeTimeSpan
        {
            get => fixTimeTimeSpan; set
            {
                fixTimeTimeSpan = value;
                if (value != null)
                    FixTimeTimeSpanMessage = "Your photos will be shifted by: " + Commons.TimeSpanPretty(value.Value);
                else
                    FixTimeTimeSpanMessage = "";

                PreviewDataGrid?.Items?.Refresh();
            }
        }

        private string fixTimeTimeSpanMessage;
        public string FixTimeTimeSpanMessage
        {
            get => fixTimeTimeSpanMessage; set
            {
                fixTimeTimeSpanMessage = value;
                RaisePropertyChanged("FixTimeTimeSpanMessage");
            }
        }


        // Polarsteps tab
        private string polarstepsDirectoryPath;
        public string PolarstepsDirectoryPath
        {
            get { return polarstepsDirectoryPath; }
            set
            {
                polarstepsDirectoryPath = value;
                RaisePropertyChanged("polarstepsDirectoryPath");
            }
        }

        private bool polarstepsIsValidDirectory = false;
        public bool PolarstepsIsValidDirectory
        {
            get { return polarstepsIsValidDirectory; }
            set
            {
                polarstepsIsValidDirectory = value;
                RaisePropertyChanged("PolarstepsIsValidDirectory");
            }

        }

        private bool polarstepsIsTripSelected = false;
        public bool PolarstepsIsTripSelected
        {
            get { return polarstepsIsTripSelected; }
            set
            {
                polarstepsIsTripSelected = value;
                RaisePropertyChanged("PolarstepsIsTripSelected");
            }
        }

        public ObservableCollection<ComboBoxItem> PolarstepsCBItems { get; set; }
        public ComboBoxItem PolarstepsCBSelectedItem { get; set; }
        private bool polarstepsCbIsEnabled = false;
        public bool PolarstepsCBIsEnabled
        {
            get { return polarstepsCbIsEnabled; }
            set
            {
                polarstepsCbIsEnabled = value;
                RaisePropertyChanged("PolarstepsCBIsEnabled");
            }
        }


        private void Button_Click_Polarsteps_Dir(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                polarstepsProcessor = new PolarstepsProcessor(fileDialog.FileName);

                if (!polarstepsProcessor.IsValidDirectory)
                {
                    PolarstepsIsValidDirectory = false;
                    return;
                }

                PolarstepsDirectoryPath = fileDialog.FileName;
                PolarstepsIsValidDirectory = true;

                PolarstepsCBItems.Clear();
                foreach (string trip in polarstepsProcessor.TripNames)
                {
                    PolarstepsCBItems.Add(new ComboBoxItem { Content = new PolarstepsTrip(trip) });
                }
                PolarstepsCBIsEnabled = true;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                polarstepsProcessor.TripSelected(e.AddedItems[0]);
                polarstepsProcessor.IsTripProcessed = false;

                PolarstepsIsTripSelected = polarstepsProcessor.SelectedTrip != null;

                polarstepsProcessor.Process();
            }
        }



        // Output tab

        private string outputDirectoryPath;
        public string OutputDirectoryPath
        {
            get => outputDirectoryPath; set
            {
                outputDirectoryPath = value;
                RaisePropertyChanged("OutputDirectoryPath");
            }
        }

        private bool outputIsDirectoryValid = false;
        public bool OutputIsDirectoryValid
        {
            get => outputIsDirectoryValid; set
            {
                outputIsDirectoryValid = value;
                RaisePropertyChanged("OutputIsDirectoryValid");
            }
        }

        private void OutputDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog fileDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                EnsurePathExists = false,
                Title = "Select output directory"
            };

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                OutputDirectoryPath = fileDialog.FileName;
                OutputIsDirectoryValid = true;

                ValidateOutputDirectory();
            }

        }

        private void ValidateOutputDirectory()
        {
            if (OutputDirectoryPath != null && OutputDirectoryPath.Contains(PhotosPath, StringComparison.InvariantCultureIgnoreCase))
            {
                OutputDirectoryPath = "Output directory should be different to selected photos directory.";
                OutputIsDirectoryValid = false;
            }
        }

        private bool OutputRename = false;

        private void OutputRename_Checked(object sender, RoutedEventArgs e)
        {
            if (OutputRenameYes?.IsChecked == true)
                OutputRename = true;
            else
                OutputRename = false;
        }

        private bool OutputOverwrite = false;

        private void OutputOverwrite_Checked(object sender, RoutedEventArgs e)
        {
            if (OutputOverwriteYes?.IsChecked == true)
                OutputOverwrite = true;
            else
                OutputOverwrite = false;

            if (OutputDirectoryGrid != null)
            {
                if (OutputOverwrite)
                    OutputDirectoryGrid.Visibility = Visibility.Hidden;
                else
                    OutputDirectoryGrid.Visibility = Visibility.Visible;
            }
        }

        private void StartProcessing_Click(object sender, RoutedEventArgs e)
        {
            if (PolarstepsIsValidDirectory && PolarstepsIsTripSelected)
            {
                polarstepsProcessor.Process();
            }
        }

        private void DG_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = (Hyperlink)e.OriginalSource;
            //System.Diagnostics.Process.Start(hyperlink.NavigateUri.AbsoluteUri);
            Trace.WriteLine(hyperlink.NavigateUri.AbsoluteUri);
            BrowserWindow browserWindow = new BrowserWindow(hyperlink.NavigateUri.AbsoluteUri);
            browserWindow.Show();
        }
    }
}
