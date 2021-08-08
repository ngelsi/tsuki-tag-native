using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models
{
    public class PictureImageTuple
    {
        public Picture Picture { get; set; }

        public Bitmap Image { get; set; }

        public PictureImageTuple()
        {

        }

        public PictureImageTuple(Picture picture, Bitmap image)
        {
            Picture = picture;
            Image = image;
        }

        ~PictureImageTuple()
        {
            Image?.Dispose();
        }
    }
}
