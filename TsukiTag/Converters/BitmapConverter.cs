﻿using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models;

namespace TsukiTag.Converters
{
    public class BitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Picture picture)
            {
                return picture.PreviewImage as IBitmap;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SampleBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Picture picture)
            {
                Bitmap pic = null;
                try
                {
                    if (!string.IsNullOrEmpty(picture.FileUrl))
                    {
                        pic = Ioc.SimpleIoc.PictureDownloader.DownloadLocalBitmap(picture.FileUrl).GetAwaiter().GetResult();
                    }

                    if (pic == null && !string.IsNullOrEmpty(picture.Url))
                    {
                        pic = Ioc.SimpleIoc.PictureDownloader.DownloadBitmap(picture.Url).GetAwaiter().GetResult();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error<Picture>(ex, $"Error occurred while downloading bitmap for image", picture);
                }


                return pic;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
