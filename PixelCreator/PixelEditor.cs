using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixelCreator
{
    public class PixelEditor : FrameworkElement
    {
        private readonly Surface _surface;
        private readonly Visual _gridTransparent;

        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
        public int Magnification { get; } = 10;
        public Color primaryBrush { get; set; }
        public Color secondaryBrush { get; set; }
        public int BrushSize { get; set; } = 20;
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
        private void Draw(Color brushColor)
        {
            var p = Mouse.GetPosition(_surface);
            var magnification = Magnification;
            var surfaceWidth = PixelWidth * magnification;
            var surfaceHeight = PixelHeight * magnification;

            if (p.X < 0 || p.X >= surfaceWidth || p.Y < 0 || p.Y >= surfaceHeight)
                return;

            _surface.SetColor(
                (int)((p.X)/ magnification),
                (int)((p.Y)/ magnification),
                brushColor);
            //_surface.InvalidateVisual();
        }
        
        public void ClearMap()
        {
            _surface.Clear();
        }
        public bool HasUnsavedChanges()
        {
            return _surface.IsDirty();
        }
        public void ChangesIsSaved() {
            _surface.MapIsSaved();
        }
        private void Erase()
        {
            Draw(Colors.Transparent);
        }

        public Point GetMousePosition(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var p = e.GetPosition(this);
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
            //var surfaceWidth = PixelWidth * magnification;
            //var surfaceHeight = PixelHeight * magnification;

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
                if (pt.X >= 0 && pt.X < PixelWidth && pt.Y >= 0 && pt.Y < PixelHeight)
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

        //ROTATE
        public void Rotate(int angle)
        {
            _surface.Rotate(angle);
        }



        Point startingPointDrawing;
        Point oldPointDrawing;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            switch (MainWindow.selectedTool)
            {
                case Tools.Tool.Pencil:
                    {
                        base.OnMouseMove(e);
                        if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
                            Draw(primaryBrush);
                        else if (e.RightButton == MouseButtonState.Pressed && IsMouseCaptured)
                            Draw(secondaryBrush);
                    }
                    break;
                case Tools.Tool.Eraser:
                    {
                        base.OnMouseMove(e);
                        if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
                            Erase();
                    }
                    break;
                case Tools.Tool.DrawLine:
                    {
                        base.OnMouseMove(e);
                        var p = GetMousePosition(e);
                        if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
                        {
                            if (oldPointDrawing != p)
                            {
                                _surface.DrawLine((int)startingPointDrawing.X, (int)startingPointDrawing.Y, (int)oldPointDrawing.X, (int)oldPointDrawing.Y, Colors.Transparent);
                            }
                            _surface.DrawLine((int)startingPointDrawing.X, (int)startingPointDrawing.Y, (int)p.X, (int)p.Y, primaryBrush);
                            oldPointDrawing = p;
                        }
                    }
                    break;
                case Tools.Tool.DrawEllipse:
                    {

                    }
                    break;
                case Tools.Tool.DrawRectangle:
                    {
                        base.OnMouseMove(e);
                        var p = GetMousePosition(e);
                        if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
                        {
                            if (oldPointDrawing != p)
                            {
                                _surface.DrawRectangle((int)startingPointDrawing.X, (int)startingPointDrawing.Y, (int)oldPointDrawing.X, (int)oldPointDrawing.Y, Colors.Transparent);
                                //_surface.DrawRectangle((int)startingPointDrawing.X, (int)startingPointDrawing.Y, Math.Abs((int)oldPointDrawing.X - (int)startingPointDrawing.X), Math.Abs((int)oldPointDrawing.Y - (int)startingPointDrawing.Y), Colors.Transparent);
                            }
                            _surface.DrawRectangle((int)startingPointDrawing.X, (int)startingPointDrawing.Y, (int)p.X, (int)p.Y, primaryBrush);
                            //_surface.DrawRectangle((int)startingPointDrawing.X, (int)startingPointDrawing.Y, Math.Abs((int)p.X - (int)startingPointDrawing.X), Math.Abs((int)p.Y - (int)startingPointDrawing.Y), BrushColor);
                            oldPointDrawing = p;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (MainWindow.selectedTool == Tools.Tool.Pencil)
            {
                base.OnMouseLeftButtonDown(e);
                CaptureMouse();
                Draw(secondaryBrush);
            }
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (MainWindow.selectedTool == Tools.Tool.Pencil)
            {
                base.OnMouseLeftButtonDown(e);
                CaptureMouse();
                Draw(primaryBrush);
            }
            else if (MainWindow.selectedTool == Tools.Tool.Eraser)

                switch (MainWindow.selectedTool)
                {
                    case Tools.Tool.Pencil:
                        {
                            base.OnMouseLeftButtonDown(e);
                            CaptureMouse();
                            Draw(primaryBrush);
                        }
                        break;
                    case Tools.Tool.Eraser:
                        {
                            base.OnMouseLeftButtonDown(e);
                            CaptureMouse();
                            Erase();
                        }
                        break;
                    case Tools.Tool.DrawLine:
                        {
                            base.OnMouseLeftButtonDown(e);
                            //Drawline
                            startingPointDrawing = GetMousePosition(e);

                            CaptureMouse();
                        }
                        break;
                    case Tools.Tool.DrawEllipse:
                        {

                        }
                        break;
                    case Tools.Tool.DrawRectangle:
                        {
                            base.OnMouseLeftButtonDown(e);

                            startingPointDrawing = GetMousePosition(e);

                            CaptureMouse();
                        }
                        break;
                    default:
                        break;
                }
        }
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            ReleaseMouseCapture();
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
        public byte[] ToByteArray()
        {
            return _surface.WriteableBitmapToByteArray();
        }
        public WriteableBitmap GetWriteableBitmap()
        {
            return _surface.GetMap();
        }
        public void SetWriteableBitmap(WriteableBitmap bitmap)
        {
            _surface.SetMap(bitmap);
        }

        private sealed class Surface : FrameworkElement
        {
            private readonly PixelEditor _owner;
            private WriteableBitmap _bitmap;
            private bool isDirty { get; set; }
            public Surface(PixelEditor owner)
            {
                _owner = owner;
                _bitmap = BitmapFactory.New(owner.PixelWidth, owner.PixelHeight);
                _bitmap.Clear(Colors.Transparent);
                RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
                _bitmap.Changed += OnBitmapChanged;
            }           
            public void SetMap(WriteableBitmap bitmap)
            {
                _bitmap = bitmap;
            }
            /// <summary>
            ///     Recognise unsave changes on bitmap
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            void OnBitmapChanged(object sender, EventArgs e)
            {
                isDirty = true;
            }
            /// <summary>
            ///     Notify that unsave changes are saved
            /// </summary>
            public void MapIsSaved() {
                isDirty = false;
            }
            /// <summary>
            ///     Get boolean value check if there is any unsave changes
            /// </summary>
            /// <returns></returns>
            public bool IsDirty(){
                return isDirty;
            }
            /// <summary>
            ///     Render magnified version of Writeablebitmap
            /// </summary>
            /// <param name="dc"></param>
            protected override void OnRender(DrawingContext dc)
            {
                base.OnRender(dc);
                var magnification = _owner.Magnification;
                var width = _bitmap.PixelWidth*magnification;
                var height = _bitmap.PixelHeight*magnification;
                dc.DrawImage(_bitmap, new Rect(0, 0, width, height));
            }
            /// <summary>
            /// Clear Writablebitmap with Colors.Transparent
            /// </summary>
            public void Clear()
            {
                _bitmap.Clear(Colors.Transparent);
            }
            /// <summary>
            /// Set color at pixel (x, y)
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="color"></param>
            internal void SetColor(int x, int y, Color color)
            {
                _bitmap.SetPixel(x, y, color);
            }
            /// <summary>
            /// Gets color at pixel (x, y)
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            internal Color GetColor(int x, int y)
            {
                if (x < 0 || y < 0)
                    return Colors.Transparent;

                return _bitmap.GetPixel((int)(x / _owner.Magnification) , (int)(y / _owner.Magnification));
            }
            /// <summary>
            /// DrawLine from P(startX, startY) to P(endX, endY)
            /// </summary>
            /// <param name="startX"></param>
            /// <param name="startY"></param>
            /// <param name="endX"></param>
            /// <param name="endY"></param>
            /// <param name="color"></param>
            internal void DrawLine(int startX, int startY, int endX, int endY, Color color)
            {
                _bitmap.DrawLine(startX / _owner.Magnification, startY / _owner.Magnification, endX / _owner.Magnification, endY / _owner.Magnification, color);
            }

            internal void DrawRectangle(int startX, int startY, int width, int height, Color color)
            {
                _bitmap.DrawRectangle(startX / _owner.Magnification, startY / _owner.Magnification, width / _owner.Magnification, height / _owner.Magnification, color);
            }
            
            //ROTATE
            internal void Rotate(int angle)
            {
                 _bitmap = _bitmap.Rotate(angle);
            }

            /// <summary>
            /// Creates a modifiable clone of WritableBitmap
            /// </summary>
            /// <returns></returns>
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
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(_bitmap));
                    enc.Save(outStream);
                    bitmap = new System.Drawing.Bitmap(outStream);
                }
                return bitmap;
            }
            public byte[] WriteableBitmapToByteArray()
            {
                var width = _bitmap.PixelWidth;
                var height = _bitmap.PixelHeight;
                var stride = width * ((_bitmap.Format.BitsPerPixel + 7) / 8);
                var bitmapData = new byte[height * stride];
                _bitmap.CopyPixels(bitmapData, stride, 0);
                return bitmapData;
            }
        }
       
    }
}
