using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models;

namespace TsukiTag.Models
{
    public class PictureOpenedEventArgs : EventArgs
    {
        public Picture Picture { get; set; }

        public Bitmap Image { get; set; }

        public PictureOpenedEventArgs(Picture picture, Bitmap image)
        {
            Picture = picture;
            Image = image;
        }
    }
}
