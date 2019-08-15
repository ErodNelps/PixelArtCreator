using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
    [Serializable]
    class FrameGIF : INotifyPropertyChanged
    {
        public Bitmap bitmap { get; set; }
        //[field: NonSerialized]
        public WriteableBitmap wbitmap { get; set; }
        //public WriteableBitmap wbitmap
        //{
        //    get { return _wbitmap; }
        //    set { _wbitmap = value; RaisePropertyChanged("Wbitmap"); }
        //}
        public byte[] wbitmapByteArray { get; set; }
        string _speed;
        public string Speed
        {
            get { return _speed;}
            set
            {
                _speed = value;
                RaisePropertyChanged("Speed");
            }
        }
        public FrameGIF Clone()
        {
            FrameGIF clone = new FrameGIF() { bitmap = bitmap, wbitmap = wbitmap.Clone(), Speed = Speed };
            return clone;
        }
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
