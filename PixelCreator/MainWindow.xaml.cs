using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RadialMenuDemo.Utils;

namespace PixelCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private enum Tool
        {
            Pencil, FillBucket, ColorPicker, Hand, ZoomIn, ZoomOut
        }

        PixelEditor pixelEditor;
        BindingList<FrameGIF> frameCollection = new BindingList<FrameGIF>();
        Color _brushColor_Primary { get; set; }
        Color _brushColor_Secondary { get; set; }
        Brush _BrushColor_Primary { get; set; }
        Brush _BrushColor_Secondary { get; set; }
        List<Color> _recentColors = new List<Color>();
        Tool selectedTool;

        public int _gifPanelHeightFrom { get; set; }
        public int _gifPanelHeightTo { get; set; }
        public int mousePos_X { get; set; }
        public int mousePos_Y { get; set; }

        private void pixelGridMouseEnter(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            pixelEditor.GetMousePosition(e);
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            pixelEditor = new PixelEditor(128,128);
            //ParentGrid.Children.Add(pixelEditor);
            pixelGrid.Child = pixelEditor;
            //pixelGrid.Children.Add(pixelEditor);
            FrameContainer.ItemsSource = frameCollection;
            _BrushColor_Primary = new SolidColorBrush(_brushColor_Primary);
            
            _BrushColor_Secondary = new SolidColorBrush(_brushColor_Secondary);
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenRadialMenu1.Execute(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            if (colorGalleryStandard.SelectedColor.HasValue)
            {
                Color selectedColor = colorGalleryStandard.SelectedColor.Value;
                if (primaryColor.IsChecked == true)
                {
                    _brushColor_Primary = selectedColor;
                    _BrushColor_Primary = Brushes.Red;
                    RaisePropertyChanged("_BrushColor_Primary");
                }
                else if(secondColor.IsChecked == true)
                {
                    _brushColor_Secondary = selectedColor;
                    _BrushColor_Secondary = Brushes.Orange;
                    RaisePropertyChanged("_BrushColor_Secondary");
                }
                pixelEditor.BrushColor = _brushColor_Primary;
                int index = 0;
                if (!isRecentlyPicked(selectedColor,ref index) && _recentColors.Count <= 10)
                {
                    _recentColors.Add(selectedColor);
                }
                else if(!isRecentlyPicked(selectedColor, ref index) && _recentColors.Count == 10)
                {
                    _recentColors.RemoveAt(0);
                    _recentColors.Add(selectedColor);
                }
                else if(isRecentlyPicked(selectedColor, ref index))
                {
                    _recentColors.RemoveAt(index);
                    _recentColors.Add(selectedColor);
                }
            }
        }
        private bool isRecentlyPicked(Color color, ref int index)
        {
            index = 0;
            foreach (var item in _recentColors)
            {
                if (color == item)
                {
                    return true;
                }
                index++;
            }
            return false;
        }
        private void PencilTool_Selected(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Pen;
            selectedTool = Tool.Pencil;
            //pixelEditor.IsEnabled = !pixelEditor.IsEnabled;
        }
        private void ZoomInTool_Selected(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Cross;
            selectedTool = Tool.ZoomIn;
        }
        private void HandTool_Selected(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
            selectedTool = Tool.Hand;
        }
        private void ZoomIn()
        {
            scaleTransform.ScaleX++;
            scaleTransform.ScaleY++;
        }
        private void ZoomOut()
        {
            if (scaleTransform.ScaleX > 1)
            {
                scaleTransform.ScaleX--;
                scaleTransform.ScaleY--;
            }
        }

        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;

        private void scrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            scrollViewer.MouseMove += OnMouseMove;
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(scrollViewer);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { 
            switch (selectedTool)
            {
                case Tool.Hand:
                    {
                        var mousePos = e.GetPosition(scrollViewer);
                        if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y <
                            scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
                        {
                            scrollViewer.Cursor = Cursors.SizeAll;
                            lastDragPoint = mousePos;
                            Mouse.Capture(scrollViewer);
                        }
                    }
                    break;
                case Tool.ZoomIn:
                    {
                        ZoomIn();
                    }
                    break;
                case Tool.ZoomOut:
                    {
                        ZoomOut();
                    }
                    break;
                default:
                    break;
            }
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(pixelGrid);
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;
            if (e.Delta > 0)
            {
                ZoomIn();
            }
            else if (e.Delta < 0)
            {
                ZoomOut();
            }

            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, pixelGrid);

            e.Handled = true;
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2,
                                                         scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow =
                              scrollViewer.TranslatePoint(centerOfViewport, pixelGrid);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(pixelGrid);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / pixelGrid.Width;
                    double multiplicatorY = e.ExtentHeight / pixelGrid.Height;

                    double newOffsetX = scrollViewer.HorizontalOffset -
                                        dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset -
                                        dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }
        bool flag = true;
        public PowerEase easeMode = new PowerEase();
        private void FramePanelClicked(object sender, MouseButtonEventArgs e)
        {
            if (flag == true)
            {
                _gifPanelHeightFrom = 170;
                _gifPanelHeightTo = 35;
                RaisePropertyChanged("_framePanelHeightFrom");
                RaisePropertyChanged("_framePanelHeightTo");
                easeMode.EasingMode = EasingMode.EaseIn;
            }
            else if (flag == false)
            {
                _gifPanelHeightFrom = 35;
                _gifPanelHeightTo = 170;
                RaisePropertyChanged("_framePanelHeightFrom");
                RaisePropertyChanged("_framePanelHeightTo");
                easeMode.EasingMode = EasingMode.EaseOut;
            }
            flag = !flag;
        }
        private void AddFrame_Clicked(object sender, RoutedEventArgs e)
        {
            System.Drawing.Bitmap bitmap = pixelEditor.ToBitmap();

            string fileName = "file1.png";

            CreateThumbnail(fileName, pixelEditor.GetWriteableBitmap());
            frameCollection.Add(new FrameGIF() { bitmap = pixelEditor.GetWriteableBitmap(), speed = 100 });
        }

        void CreateThumbnail(string filename, BitmapSource image5)
        {
            if (filename != string.Empty)
            {
                using (FileStream stream5 = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                    encoder5.Frames.Add(BitmapFrame.Create(image5));
                    encoder5.Save(stream5);
                }
            }
        }

        public BitmapImage BitmapFromSource(BitmapSource bitmapSource)
        {

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);

            memoryStream.Position = 0;
            bImg.BeginInit();
            bImg.StreamSource = memoryStream;
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
        }
        /* #region Relay Command for radial Menu */
        private bool _isOpen1 = false;
        public bool IsOpen1
        {
            get
            {
                return _isOpen1;
            }
            set
            {
                _isOpen1 = value;
                RaisePropertyChanged();
            }
        }

        private bool _isOpen2 = false;
        public bool IsOpen2
        {
            get
            {
                return _isOpen2;
            }
            set
            {
                _isOpen2 = value;
                RaisePropertyChanged();
            }
        }
        public ICommand CloseRadialMenu1
        {
            get
            {
                return new RelayCommand(() => IsOpen1 = false);
            }
        }
        public ICommand OpenRadialMenu1
        {
            get
            {
                return new RelayCommand(() => { IsOpen1 = true; IsOpen2 = false; });
            }
        }
        public ICommand CloseRadialMenu2
        {
            get
            {
                return new RelayCommand(() => IsOpen2 = false);
            }
        }
        public ICommand OpenRadialMenu2
        {
            get
            {
                return new RelayCommand(() => { IsOpen2 = true; IsOpen1 = false; });
            }
        }
        public ICommand Test1
        {
            get
            {
                return new RelayCommand(() => System.Diagnostics.Debug.WriteLine("1"));
            }
        }
        public ICommand Test2
        {
            get
            {
                return new RelayCommand(() => System.Diagnostics.Debug.WriteLine("2"));
            }
        }
        public ICommand Test3
        {
            get
            {
                return new RelayCommand(() => System.Diagnostics.Debug.WriteLine("3"));
            }
        }
        public ICommand Test4
        {
            get
            {
                return new RelayCommand(() => System.Diagnostics.Debug.WriteLine("4"));
            }
        }
        public ICommand Test5
        {
            get
            {
                return new RelayCommand(() => System.Diagnostics.Debug.WriteLine("5"));
            }
        }
        public ICommand Test6
        {
            get
            {
                return new RelayCommand(
                    () =>
                    {
                        System.Diagnostics.Debug.WriteLine("6");
                    },
                    () =>
                    {
                        return false; // To disable the 6th item
                    }
                );
            }
        }
        /* #endregion */
    }
}
