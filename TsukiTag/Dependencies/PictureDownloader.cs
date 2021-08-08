using Avalonia.Controls;
using Avalonia.Media.Imaging;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models;

namespace TsukiTag.Dependencies
{
    public interface IPictureDownloader
    {
        Task<byte[]> DownloadImageBytes(string url);

        Task<Stream> DownloadImageStream(string url);

        Task<Bitmap> DownloadBitmap(string url);

        Task<Bitmap> DownloadBitmap(string url, string? md5);

        Task<Bitmap> DownloadBitmap(Picture picture);

        Task<Bitmap> DownloadLocalBitmap(string filePath);
    }

    public class PictureDownloader : IPictureDownloader
    {
        private readonly IDbRepository dbRepository;

        public PictureDownloader(
            IDbRepository dbRepository
        )
        {
            this.dbRepository = dbRepository;
        }

        public async Task<byte[]> DownloadImageBytes(string url)
        {
            using (var client = new WebClient())
            {
                using (var stream = await client.OpenReadTaskAsync(new Uri(url)))
                {
                    using (var ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);
                        return ms.ToArray();
                    }
                }
            }
        }

        public async Task<Stream> DownloadImageStream(string url)
        {
            using (var client = new WebClient())
            {
                using (var stream = await client.OpenReadTaskAsync(new Uri(url)))
                {
                    return stream;
                }
            }
        }

        public async Task<Bitmap> DownloadBitmap(Picture picture)
        {
            Bitmap image = null;

            if (!string.IsNullOrEmpty(picture.FileUrl))
            {
                image = await DownloadLocalBitmap(picture.FileUrl);
            }

            if (image == null && !string.IsNullOrEmpty(picture.Source) && picture.IsLocallyImported)
            {
                image = await DownloadLocalBitmap(picture.Source);
            }

            if (image == null && !string.IsNullOrEmpty(picture.Url))
            {
                image = await DownloadBitmap(picture.Url);
            }

            return image;
        }

        public async Task<Bitmap> DownloadLocalBitmap(string filePath)
        {
            if (File.Exists(filePath))
            {
                return await Task.FromResult(new Bitmap(filePath));
            }

            return await Task.FromResult<Bitmap>(null);
        }

        public async Task<Bitmap> DownloadBitmap(string url)
        {
            using (var client = new WebClient())
            {
                using (var stream = await client.OpenReadTaskAsync(new Uri(url)))
                {
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        ms.Position = 0;

                        return new Avalonia.Media.Imaging.Bitmap(ms);
                    }
                }
            }
        }

        public async Task<Bitmap> DownloadBitmap(string url, string? md5)
        {
            var bitmap = !string.IsNullOrEmpty(md5) ? this.dbRepository.ThumbnailStorage.FindThumbnail(md5) : null;
            if (bitmap != null)
            {
                return bitmap;
            }

            if (!string.IsNullOrEmpty(url))
            {
                bitmap = await DownloadBitmap(url);
            }

            return bitmap;
        }
    }
}
