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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.PolarstepsCBItems = new ObservableCollection<ComboBoxItem>();
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

        public class A
        {
            public string show;
            public string have;

            override public string ToString()
            {
                return show + " " + have;
            }
        }



        private void Button_Click_Polarsteps_Dir(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog();
            fileDialog.IsFolderPicker = true;
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
            if(PolarstepsIsValidDirectory && PolarstepsIsTripSelected)
            {
                polarstepsProcessor.Process();
            }
        }
    }
}
