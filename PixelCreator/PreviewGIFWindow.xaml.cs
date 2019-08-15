using AnimatedGif;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PixelCreator
{
    /// <summary>
    /// Interaction logic for PreviewGIFWindow.xaml
    /// </summary>
    public partial class PreviewGIFWindow : Window, INotifyPropertyChanged
    {
        string _filePath;
        public string filePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                RaisePropertyChanged("filePath");
            }
        }
        public PreviewGIFWindow(string filePath)
        {
            InitializeComponent();
            DataContext = this;
            this.filePath = filePath;
        }

        private void SaveGifButton_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }
}
