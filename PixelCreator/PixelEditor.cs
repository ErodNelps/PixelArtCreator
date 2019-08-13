using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixelCreator
{
    public class PixelEditor : FrameworkElement, INotifyPropertyChanged
    {
        private readonly Surface _surface;
        private readonly Visual _gridTransparent;

        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
        public int Magnification { get; } = 10;
        public Color BrushColor { get; set; } = Colors.Black;

        public PixelEditor()
        {
            _gridTransparent = CreateGridLines();
            _surface = new Surface(this);

            Cursor = Cursors.Pen;
            AddVisualChild(_gridTransparent);
            AddVisualChild(_surface);
        }
        public PixelEditor(int pixelWidth, int pixelHeight)
        {
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            
            _gridTransparent = CreateGridLines();
            _surface = new Surface(this);

            Cursor = Cursors.Pen;
            AddVisualChild(_gridTransparent);
            AddVisualChild(_surface);
        }

        protected override int VisualChildrenCount => 2;
        
        //Setting z-order of visual children
        protected override Visual GetVisualChild(int index)
        {
            return index == 0 ? _gridTransparent : _surface;
        }
        
        //Draw pixel
        private void Draw()
        {
            var p = Mouse.GetPosition(_surface);
            var magnification = Magnification;
            var surfaceWidth = PixelWidth * magnification;
            var surfaceHeight = PixelHeight * magnification;

            if (p.X < 0 || p.X >= surfaceWidth || p.Y < 0 || p.Y >= surfaceHeight)
                return;

            _surface.SetColor(
                (int)(p.X / magnification),
                (int)(p.Y / magnification),
                BrushColor);

            //_surface.InvalidateVisual();
        }

        public Point GetMousePosition(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var p = e.GetPosition(this);
            Debug.WriteLine($"{(int)(p.X + 1) / 10} - {(int)(p.Y + 1) / 10}");
            Debug.WriteLine($"{_surface.GetColor((int)p.X, (int)p.Y)}");
            return p;
        }

        public Color GetPixelColor(int x, int y)
        {
            return _surface.GetColor(x, y);
        }

        //FILL BUCKET
        public void floodfill(int x, int y, Color targetColor, Color replacementColor)
        {
            var magnification = Magnification;
            var surfaceWidth = PixelWidth * magnification;
            var surfaceHeight = PixelHeight * magnification;

            //DFS 
            //if (x < 0 || x >= surfaceWidth || y < 0 || y >= surfaceHeight)
            //    return;

            //if (!Equals(_surface.GetColor(x * Magnification, y * Magnification), targetColor))
            //    return;

            //if (Equals(_surface.GetColor(x * Magnification, y * Magnification), targetColor))
            //{
            //    _surface.SetColor(
            //   x, y, replacementColor);

            //    floodfill(x + 1, y, targetColor, replacementColor);
            //    floodfill(x, y + 1, targetColor, replacementColor);
            //    floodfill(x - 1, y, targetColor, replacementColor);
            //    floodfill(x, y - 1, targetColor, replacementColor);
            //}

            //BFS
            Stack<Point> pixels = new Stack<Point>();
            pixels.Push(new Point(x, y));

            if (Equals(replacementColor, targetColor))
                return;

            while (pixels.Count() > 0)
            {
                Point pt = pixels.Pop();
                if (pt.X > 0 && pt.X < surfaceWidth && pt.Y > 0 && pt.Y < surfaceHeight)
                {
                    if (Equals(_surface.GetColor((int)pt.X * Magnification, (int)pt.Y * Magnification), targetColor))
                    {
                        _surface.SetColor(
                         (int)pt.X, (int)pt.Y, replacementColor);

                        pixels.Push(new Point(pt.X - 1, pt.Y));
                        pixels.Push(new Point(pt.X + 1, pt.Y));
                        pixels.Push(new Point(pt.X, pt.Y - 1));
                        pixels.Push(new Point(pt.X, pt.Y + 1));
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
                Draw();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (MainWindow.selectedTool == Tools.Tool.Pencil)
            {
                base.OnMouseLeftButtonDown(e);
                CaptureMouse();
                Draw();
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var magnification = Magnification;
            var size = new Size(PixelWidth * magnification, PixelHeight * magnification);

            _surface.Measure(size);

            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _surface.Arrange(new Rect(finalSize));
            return finalSize;
        }

        public Visual CreateGridLines()
        {
            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();

            var w = PixelWidth;
            var h = PixelHeight;
            var m = Magnification;
            var pen = new Pen();
            pen.Freeze();
            int flip = 0;
            for (var x = 0; x < w * m; x += m)
            {
                flip = 1 - flip;
                for (var y = 0; y < h * m; y += m)
                {
                    var brush = new SolidColorBrush(flip % 2 == 0 ? Colors.Transparent : Colors.LightGray);
                    dc.DrawRectangle(brush, pen, new Rect(x, y, m, m));
                    flip = 1 - flip;
                }
            }

            dc.Close();
            return dv;
        }

        public System.Drawing.Bitmap ToBitmap()
        {
            System.Drawing.Bitmap bitmap = _surface.BitmapFromSource();
            return bitmap;
        }
        public WriteableBitmap GetWriteableBitmap()
        {
            return _surface.GetMap();
        }
        private sealed class Surface : FrameworkElement
        {
            private readonly PixelEditor _owner;
            private WriteableBitmap _bitmap;

            public Surface(PixelEditor owner)
            {
                _owner = owner;
                _bitmap = BitmapFactory.New(owner.PixelWidth, owner.PixelHeight);
                _bitmap.Clear(Colors.Transparent);
                RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            }
            
            protected override void OnRender(DrawingContext dc)
            {
                base.OnRender(dc);
                var magnification = _owner.Magnification;
                var width = _bitmap.PixelWidth*magnification;
                var height = _bitmap.PixelHeight*magnification;

                dc.DrawImage(_bitmap, new Rect(0, 0, width, height));
            }

            internal void SetColor(int x, int y, Color color)
            {
                _bitmap.SetPixel(x, y, color);
            }

            internal Color GetColor(int x, int y)
            {
                if (x < 0 || y < 0)
                    return Colors.Transparent;

                return _bitmap.GetPixel((int)(x / _owner.Magnification) , (int)(y / _owner.Magnification));
            }

            public WriteableBitmap GetMap()
            {
                return _bitmap.Clone();
            }
            public System.Drawing.Bitmap BitmapFromSource()
            {
                if (_bitmap == null)
                {
                    throw new ArgumentNullException(nameof(_bitmap));
                }

                System.Drawing.Bitmap bitmap;
                using (System.IO.MemoryStream outStream = new System.IO.MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(_bitmap));
                    enc.Save(outStream);
                    bitmap = new System.Drawing.Bitmap(outStream);
                }
                return bitmap;
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
