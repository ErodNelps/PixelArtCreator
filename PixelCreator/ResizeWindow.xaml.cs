using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ResizeWindow.xaml
    /// </summary>
    public partial class ResizeWindow : Window
    {
        PixelEditor pxE { get; set; }
        public ResizeWindow(ref PixelEditor pixelEditor)
        {
            InitializeComponent();
            DataContext = pixelEditor;
            pxE = pixelEditor;
        }
        
        private void ApplyButton_Clicked(object sender, RoutedEventArgs e)
        {
            pxE = new PixelEditor(pxE.PixelHeight, pxE.PixelHeight);
            this.Close();
        }
    }
}
