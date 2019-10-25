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

        static public string[] PhotoExtensions = { ".JPG", ".JPEG", ".NEF" };
        static public readonly string PhotoExtensionsString = "Photos|*.jpg;*.jpeg;*.nef";
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.PolarstepsCBItems = new ObservableCollection<ComboBoxItem>();
        }

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

        private string polarstepsButtonContent = "Browse...";
        public string PolarstepsButtonContent
        {
            get { return polarstepsButtonContent; }
            set
            {
                polarstepsButtonContent = value;
                RaisePropertyChanged("PolarstepsButtonContent");
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

        // Polarsteps Trips CB
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


        private PolarstepsProcessor polarstepsProcessor;

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
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

                PolarstepsButtonContent = fileDialog.FileName;
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
                PolarstepsIsTripSelected = polarstepsProcessor.SelectedTrip != null;
            }
        }

        private void PolarstepsStartProcessing_Click(object sender, RoutedEventArgs e)
        {
            if (PolarstepsIsValidDirectory && PolarstepsIsTripSelected)
            {
                polarstepsProcessor.Process();
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
                PhotosPath = fileDialog.FileName;

                ObservableCollection<ImagePreviewClass> images = new ObservableCollection<ImagePreviewClass>();


                var imagePaths = System.IO.Directory.EnumerateFiles(fileDialog.FileName, "*", System.IO.SearchOption.AllDirectories).
                    Where(f => PhotoExtensions.Contains(System.IO.Path.GetExtension(f), StringComparer.InvariantCultureIgnoreCase));

                foreach (string image in imagePaths)
                {
                    images.Add(new ImagePreviewClass(fileDialog.FileName, image));
                }

                ImagePreviewDataGrid.ItemsSource = images;

                PhotosLoadedInfo.Text = $"{images.Count} photos loaded succesfully.";
                RaisePropertyChanged("PhotosLoadedInfo");
            }
        }

        private void ImagePreviewDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ImagePreviewDataGrid.UnselectAllCells();
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

        private DateTime? fixTimeCameraDateTaken = new DateTime(1451123);
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

        private DateTime? fixTimePhotoDateTaken = new DateTime(1451123);
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
            }
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
        }

        private void FixTimeNoButton_Checked(object sender, RoutedEventArgs e)
        {
            FixTimeOption = FixTime.None;
        }

        private void FixTimeManualButton_Checked(object sender, RoutedEventArgs e)
        {
            FixTimeOption = FixTime.Manual;
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
            if(fileDialog.ShowDialog() == true)
            {
                FixTimeManualPhotoPath = fileDialog.FileName;
                FixTimeManualFilename = System.IO.Path.GetFileName(fileDialog.FileName);
                FixTimeManualDateTaken = PhotoProcessor.GetPhotoDateTaken(fileDialog.FileName);

                FixTimeManualDateTime.Value = FixTimeManualDateTaken;
            }
        }

    }
}
