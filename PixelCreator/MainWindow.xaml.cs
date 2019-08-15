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
using AnimatedGif;
using Microsoft.Win32;
using System.Xml;
namespace PixelCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        PixelEditor pixelEditor;
        BindingList<FrameGIF> frameCollection = new BindingList<FrameGIF>();
        Color _brushColor_Primary { get; set; }
        Color _brushColor_Secondary { get; set; }
        Brush _BrushColor_Primary;
        public Brush BrushColor_Primary
        {
            get { return _BrushColor_Primary; }
            set
            {
                _BrushColor_Primary = value;
                RaisePropertyChanged("BrushColor_Primary");
            }
        }
        Brush _BrushColor_Secondary;
        public Brush BrushColor_Secondary
        {
            get { return _BrushColor_Secondary; }
            set
            {
                _BrushColor_Secondary = value;
                RaisePropertyChanged("BrushColor_Secondary");
            }
        }
        List<Color> _recentColors = new List<Color>();
        internal static Tools.Tool selectedTool;

        public int _gifPanelHeightFrom { get; set; }
        public int _gifPanelHeightTo { get; set; }
        public int mousePos_X { get; set; }
        public int mousePos_Y { get; set; }
        bool isEnterGrid = false;
        private void pixelGridMouseEnter(object sender, MouseEventArgs e)
        {
            isEnterGrid = true;
        }
        private void pixelGridMouseLeave(object sender, MouseEventArgs e)
        {
            isEnterGrid = false;
        }
        public int _currentHeight { get; set; } = 128;
        public int _currentWidth { get; set; } = 128;
        public MainWindow()
        {
            pixelEditor = new PixelEditor(_currentWidth, _currentHeight);
            InitializeComponent();
            DataContext = this;
            this.Closing += MainWindow_Closing;
            pixelGrid.Child = pixelEditor;

            FrameContainer.ItemsSource = frameCollection;
            frameCollection.Add(new FrameGIF() { bitmap = pixelEditor.ToBitmap(), wbitmap = pixelEditor.GetWriteableBitmap(), Speed = "100ms" });
            _brushColor_Primary = Colors.Black;
            _brushColor_Secondary = Colors.White;
            _BrushColor_Primary = new SolidColorBrush(_brushColor_Primary);
            _BrushColor_Secondary = new SolidColorBrush(_brushColor_Secondary);

            //PixelSizeLabel = $"Pixel Size: ({pixelSizeSlider.Value/10})";


            Directory.CreateDirectory("C:/PixelCreator");
        }
        
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!TriggerSaveMechanism())
                return;
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
                    BrushColor_Primary = new SolidColorBrush(_brushColor_Primary);
                }
                else if(secondColor.IsChecked == true)
                {
                    _brushColor_Secondary = selectedColor;
                    BrushColor_Secondary = new SolidColorBrush(_brushColor_Secondary);
                }
                pixelEditor.primaryBrush = _brushColor_Primary;
                pixelEditor.secondaryBrush = _brushColor_Secondary;
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
        //Tool select events
        private void PencilTool_Selected(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Pen;
            selectedTool = Tools.Tool.Pencil;
        }
        private void FillBucketTool_Selected(object sender, RoutedEventArgs e)
        {
            selectedTool = Tools.Tool.FillBucket;
        }
        private void ColorPickerTool_Selected(object sender, RoutedEventArgs e)
        {
            selectedTool = Tools.Tool.ColorPicker;
        }
        private void EraserTool_Selected(object sender, RoutedEventArgs e)
        {
            selectedTool = Tools.Tool.Eraser;
        }
        private void ZoomInTool_Selected(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Cross;
            selectedTool = Tools.Tool.ZoomIn;
        }
        private void ZoomOutTool_Selected(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Cross;
            selectedTool = Tools.Tool.ZoomOut;
        }
        private void HandTool_Selected(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
            selectedTool = Tools.Tool.Hand;
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

        private void DrawLine_Selected(object sender, RoutedEventArgs e)
        {
            selectedTool = Tools.Tool.DrawLine;
        }
        private void DrawEllipse_Selected(object sender, RoutedEventArgs e)
        {
            selectedTool = Tools.Tool.DrawEllipse;
        }
        private void DrawRectangle_Selected(object sender, RoutedEventArgs e)
        {
            selectedTool = Tools.Tool.DrawRectangle;
        }

        //ROTATE
        private void RotateMenuItemClicked(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string rotateMode = item.Header.ToString();

            switch(rotateMode)
            {
                case "Rotate Left 90°":
                    {
                        pixelEditor.Rotate(90);
                    }
                    break;
                case "Rotate Right 90°":
                    {
                        pixelEditor.Rotate(270);
                    }
                    break;
                case "Rotate 180°":
                    {
                        pixelEditor.Rotate(180);
                    }
                    break;
                case "Flip Vertical":
                    {
                        pixelEditor.FlipVertical();
                    }
                    break;
                case "Flip Horizontal":
                    {
                        pixelEditor.FlipHorizontal();
                    }
                    break;
                default:
                    break;
            }
        }

        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;

        void scrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            scrollViewer.MouseMove += OnMouseMove;
        }

        int _posX = 0;
        int _posY = 0;

        string _position;
        public string Position {
            get {return _position; }
            set { _position = value;
                RaisePropertyChanged("Position");
            }
        }
        void OnMouseMove(object sender, MouseEventArgs e)
        {
            var point = pixelEditor.GetMousePosition(e);
            if ((int)point.X / 10 + 1 > 0 && (int)point.X / 10 + 1 < pixelEditor.PixelWidth && (int)point.Y / 10 + 1 > 0 && (int)point.Y / 10 + 1 < pixelEditor.PixelHeight)
            {
                _posX = (int)point.X / 10 + 1;
                _posY = (int)point.Y / 10 + 1;

                Position = $"X: {_posX} Y:{_posY}";
            }
            if (isEnterGrid && selectedTool == Tools.Tool.Pencil)
            {
                base.OnMouseMove(e);
                var p = pixelEditor.GetMousePosition(e);
            }

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
                case Tools.Tool.Hand:
                    {
                        var mousePos = e.GetPosition(scrollViewer);
                        if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y < scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
                        {
                            scrollViewer.Cursor = Cursors.SizeAll;
                            lastDragPoint = mousePos;
                            Mouse.Capture(scrollViewer);
                        }
                    }
                    break;
                case Tools.Tool.FillBucket:
                    {
                        var p = (Point)pixelEditor.GetMousePosition(e);
                        Color targetColor = pixelEditor.GetPixelColor((int)p.X, (int)p.Y);
                        var magnification = pixelEditor.Magnification;
                        pixelEditor.floodfill((int)(p.X / magnification), (int)(p.Y / magnification), targetColor, _brushColor_Primary);
                    }
                    break;
                case Tools.Tool.ColorPicker:
                    {
                        var p = (Point)pixelEditor.GetMousePosition(e);
                        Color pickedColor = pixelEditor.GetPixelColor((int)p.X, (int)p.Y);
                        if (primaryColor.IsChecked == true)
                        {
                            _brushColor_Primary = pickedColor;
                            BrushColor_Primary = new SolidColorBrush(_brushColor_Primary); ;
                        }
                        else if (secondColor.IsChecked == true)
                        {
                            _brushColor_Primary = pickedColor;
                            BrushColor_Secondary = new SolidColorBrush(_brushColor_Secondary);
                        }
                    }
                    break;
                case Tools.Tool.ZoomIn:
                    {
                        ZoomIn();
                    }
                    break;
                case Tools.Tool.ZoomOut:
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
        //Gif Maker
        //Frame Panel Animation
        private void FramePanelClicked(object sender, MouseButtonEventArgs e)
        {
            if (flag == true)
            {
                _gifPanelHeightFrom = 180;
                _gifPanelHeightTo = 35;
                RaisePropertyChanged("_gifPanelHeightFrom");
                RaisePropertyChanged("_gifPanelHeightTo");
                easeMode.EasingMode = EasingMode.EaseIn;
            }
            else if (flag == false)
            {
                _gifPanelHeightFrom = 35;
                _gifPanelHeightTo = 180;
                RaisePropertyChanged("_gifPanelHeightFrom");
                RaisePropertyChanged("_gifPanelHeightTo");
                easeMode.EasingMode = EasingMode.EaseOut;
            }
            flag = !flag;
        }
        //Add frame button click event
        private void AddFrame_Clicked(object sender, RoutedEventArgs e)
        {
            frameCollection.Add(new FrameGIF() { bitmap = pixelEditor.ToBitmap(), wbitmap = pixelEditor.GetWriteableBitmap(), Speed = "100ms" });
        }

        private void AllFrameSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var frameSpeedcmb = sender as ComboBox;
            Debug.WriteLine($"{frameSpeedcmb.SelectedValue.ToString()}");

            foreach (var frame in frameCollection)
            {
                frame.Speed = frameSpeedcmb.SelectedValue.ToString();
            }
        }

        private void PreviewGIFButton_Clicked(object sender, RoutedEventArgs e)
        {
            int framesCount = frameCollection.Count();
            

            using (var gif = AnimatedGif.AnimatedGif.Create("C:/PixelCreator/pixelCreatorGif.gif", 100))
            {
                for (int i = 0; i < framesCount; i++)
                {
                    gif.AddFrame(frameCollection[i].bitmap, delay: int.Parse(frameCollection[i].Speed.Replace("ms", "")), quality: GifQuality.Bit8);
                }
            }
            PreviewGIFWindow preview = new PreviewGIFWindow("C:/gif.gif");
            if (preview.ShowDialog() == true)
            {
                SaveFileDialog exportGIFDialog = new SaveFileDialog();
                exportGIFDialog.Filter = "GIF | *.gif";
                exportGIFDialog.DefaultExt = "*.gif";
                exportGIFDialog.OverwritePrompt = true;
                if (exportGIFDialog.ShowDialog() == true)
                {
                    using (var gif = AnimatedGif.AnimatedGif.Create(exportGIFDialog.FileName, 100))
                    {
                        for (int i = 0; i < framesCount; i++)
                        {
                            gif.AddFrame(frameCollection[i].bitmap, delay: int.Parse(frameCollection[i].Speed.Replace("ms", "")), quality: GifQuality.Bit8);
                        }
                    }
                }
            }
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

        string currentFileName = "Untitled.xml";
        string currentSaveLoc = "";
        bool isSavedToFile = false;
        private void NewButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (!TriggerSaveMechanism())
            {
                return;
            }
            pixelEditor.ClearMap();
            frameCollection.Clear();
            isSavedToFile = false;
        }
        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
        bool TriggerSaveMechanism()
        {
            if (pixelEditor.HasUnsavedChanges())
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save changes to " + currentFileName, "Pixel Creator", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if(currentSaveLoc == null)
                    {
                        if (!isSavedToFile)
                        {
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Filter = "PixelCreator (*.xml)| *.xml";
                            saveFileDialog.DefaultExt = "*.xml";
                            saveFileDialog.OverwritePrompt = true;
                            if (saveFileDialog.ShowDialog() == true)
                            {
                                currentSaveLoc = saveFileDialog.FileName;
                                SaveToFile(currentFileName);
                                return true;
                            }
                        }
                        else
                        {
                            SaveToFile(currentFileName);
                            return true;
                        }
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }
        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!isSavedToFile)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PixelCreator (*.xml)| *.xml";
                saveFileDialog.DefaultExt = "*.xml";
                saveFileDialog.OverwritePrompt = true;
                if (saveFileDialog.ShowDialog() == true)
                {
                    currentSaveLoc = saveFileDialog.FileName;
                }
                else
                    return;
            }

            if(currentSaveLoc != String.Empty)
            {
                SaveToFile(currentSaveLoc);
                isSavedToFile = true;
                pixelEditor.ChangesIsSaved();
            }
        }

        void SaveToFile(string path)
        {
            var doc = new XmlDocument();
            
            var root = doc.CreateElement("FrameGIF");
            root.SetAttribute("FrameCount", frameCollection.Count.ToString());
            for (int i = 0; i < frameCollection.Count; i++)
            {
                var frame = doc.CreateElement("Frame");
                root.AppendChild(frame);
                frame.SetAttribute("Speed", frameCollection[i].Speed);
                frame.SetAttribute("WBitmap", Encoding.UTF8.GetString(frameCollection[i].wbitmap.ToByteArray()));
            }
            doc.AppendChild(root);
            doc.Save(currentSaveLoc);
            isSavedToFile = true;
        }    
        
        private void OpenButton_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == true)
            {
                FileInfo openfile = new FileInfo(openFileDialog.FileName);
                try
                {
                    if (openfile.Exists)
                    {
                        frameCollection.Clear();
                        var doc = new XmlDocument();
                        doc.Load(openfile.FullName);
                        var root = doc.DocumentElement;
                        var frameCount = int.Parse(root.Attributes["FrameCount"].Value);
                        for(int i =0; i < frameCount; i++)
                        {
                            byte[] byteArray = Encoding.ASCII.GetBytes(root.ChildNodes[i].Attributes["WBitmap"].Value);
                            var bitmapImage = ToImage(byteArray);
                            frameCollection.Add(new FrameGIF() { wbitmap = new WriteableBitmap(bitmapImage), Speed = root.ChildNodes[i].Attributes["Speed"].Value});
                        }
                        currentFileName = openfile.Name;
                        pixelEditor.SetWriteableBitmap(frameCollection[0].wbitmap);
                        pixelGrid.Child = null;
                        pixelGrid.Child = pixelEditor;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void Frame_Selected(object sender, RoutedEventArgs e)
        {
            if (!FrameContainer.HasItems)
                return;
            var frame = FrameContainer.SelectedItem as FrameGIF;
            if (frame == null)
                return;
            pixelEditor.SetWriteableBitmap(frame.wbitmap);
            pixelGrid.Child = null;
            pixelGrid.Child = pixelEditor;
        }

        private void CopyFrame_Clicked(object sender, RoutedEventArgs e)
        {
            if (!FrameContainer.HasItems)
                return;
            var frame = FrameContainer.SelectedItem as FrameGIF;
            if (frame == null)
                return;
            FrameGIF copiedFrame = frame.Clone();
            frameCollection.Add(copiedFrame);
        }

        private void DeleteFrame_Clicked(object sender, RoutedEventArgs e)
        {
            if (!FrameContainer.HasItems)
                return;
            var frame = FrameContainer.SelectedItem as FrameGIF;
            if (frame == null)
                return;
            frameCollection.Remove(frame);
        }
        private void MoveLeftButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (FrameContainer.SelectedItem == null)
            {
                MessageBox.Show("Please select item!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int selectedIndex = FrameContainer.SelectedIndex;

            if (selectedIndex == 0)
            {
                FrameContainer.SelectedItem = frameCollection[0];
                return;
            }
            else
            {
                SwapFile(selectedIndex, selectedIndex - 1);
                FrameContainer.SelectedItem = frameCollection[selectedIndex - 1];
            }

        }
        private void SwapFile(int indexA, int indexB)
        {
            frameCollection.Insert(indexB, frameCollection[indexA]);
            frameCollection.RemoveAt(indexA + 1);
        }
        private void MoveRightButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (FrameContainer.SelectedItem == null)
            {
                MessageBox.Show("Please select item!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int selectedIndex = FrameContainer.SelectedIndex;

            if (selectedIndex == frameCollection.Count - 1)
            {
                FrameContainer.SelectedItem = frameCollection[frameCollection.Count - 1];
                return;
            }
            else
            {
                SwapFile(selectedIndex + 1, selectedIndex);
                FrameContainer.SelectedItem = frameCollection[selectedIndex + 1];
            }
        }

        string _pixelSizeLabel;
        public string PixelSizeLabel
        {
            get { return _pixelSizeLabel; }
            set { _pixelSizeLabel = value;
                RaisePropertyChanged("PixelSizeLabel");
            }
        }
        private void PixelSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //PixelSizeLabel = $"Pixel Size: ({pixelSizeSlider.Value/10})";
        }

        private void ClearButton_Clicked(object sender, RoutedEventArgs e)
        {
            pixelEditor.ClearMap();
        }

        private void ExportButton_Clicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog exportImageDialog = new SaveFileDialog();
            exportImageDialog.Filter = "Png (Protable network graphics| *.png)";
            exportImageDialog.DefaultExt = "*.png";
            exportImageDialog.OverwritePrompt = true;
            if(exportImageDialog.ShowDialog() == true)
            {
                CreateThumbnail(exportImageDialog.FileName, pixelEditor.GetWriteableBitmap());
            }
            
        }

        private void SaveGIFButton_Clicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog exportGIFDialog = new SaveFileDialog();
            exportGIFDialog.Filter = "GIF | *.gif";
            exportGIFDialog.DefaultExt = "*.gif";
            exportGIFDialog.OverwritePrompt = true;
            if (exportGIFDialog.ShowDialog() == true)
            {
                int framesCount = frameCollection.Count();

                using (var gif = AnimatedGif.AnimatedGif.Create(exportGIFDialog.FileName, 100))
                {
                    for (int i = 0; i < framesCount; i++)
                    {
                        gif.AddFrame(frameCollection[i].bitmap, delay: int.Parse(frameCollection[i].Speed.Replace("ms", "")), quality: GifQuality.Bit8);
                    }
                }
            }
        }

        private void ResizeButton_Clicked(object sender, RoutedEventArgs e)
        {
            ResizeWindow resizeWindow = new ResizeWindow(pixelEditor);
            resizeWindow.Show();
        }
    }
}
