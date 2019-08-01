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
        public Visual _gridLines;
        //public bool isEnabled { get; set; } = true;
        public int PixelWidth { get; set; } = 128;
        public int PixelHeight { get; set; } = 128;
        public int Magnification { get; set; } = 10;
        public Color BrushColor { get; set; } = Colors.Black;
        public PixelEditor()
        {
            _surface = new Surface(this);
            _gridLines = CreateGridLines();

            Cursor = Cursors.Pen;
            AddVisualChild(_surface);
            AddVisualChild(_gridLines);
        }

        protected override int VisualChildrenCount => 2;

        protected override Visual GetVisualChild(int index)
        {
            return index == 0 ? _surface : _gridLines;
        }

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

            _surface.InvalidateVisual();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
                Draw();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            CaptureMouse();
            Draw();
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
            var d = -0.5d; // snap gridlines to device pixels

            var pen = new Pen(new SolidColorBrush(Color.FromArgb(63, 63, 63, 63)), 1d);

            pen.Freeze();

            for (var x = 1; x < w; x++)
                dc.DrawLine(pen, new Point(x * m + d, 0), new Point(x * m + d, h * m));

            for (var y = 1; y < h; y++)
                dc.DrawLine(pen, new Point(0, y * m + d), new Point(w * m, y * m + d));

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
                _bitmap = BitmapFactory.New(owner.PixelWidth*owner.Magnification, owner.PixelHeight*owner.Magnification);
                _bitmap.Clear(Colors.Transparent);
                RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            }
            
            protected override void OnRender(DrawingContext dc)
            {
                base.OnRender(dc);
                var magnification = _owner.Magnification;
                var width = _bitmap.PixelWidth * magnification;
                var height = _bitmap.PixelHeight * magnification;

                dc.DrawImage(_bitmap, new Rect(0, 0, width, height));
            }

            internal void SetColor(int x, int y, Color color)
            {
                _bitmap.SetPixel(x, y, color);
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
