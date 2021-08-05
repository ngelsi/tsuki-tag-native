﻿using Avalonia.Media.Imaging;
using ExifLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Extensions;
using TsukiTag.Models;
using TsukiTag.Models.Repository;

namespace TsukiTag.Dependencies
{
    public interface IPictureWorker
    {
        void OpenPictureInDefaultApplication(Picture picture, Workspace? workspace = null);

        void OpenPictureWebsite(Picture picture);

        Task<string> SaveWorkspacePicture(Picture picture, Workspace workspace);

        Task<Picture?> CreatePictureMetadataFromLocalImage(string imagePath);

        Task DeletePicture(WorkspacePicture picture, Workspace workspace);
    }

    public class PictureWorker : IPictureWorker
    {
        private readonly IPictureDownloader pictureDownloader;


        public PictureWorker(
            IPictureDownloader pictureDownloader
        )
        {
            this.pictureDownloader = pictureDownloader;
        }

        public async Task<Picture?> CreatePictureMetadataFromLocalImage(string imagePath)
        {
            try
            {
                return await Task.Run<Picture?>(async () =>
                {
                    try
                    {
                        var imageBytes = File.ReadAllBytes(imagePath);

                        var picture = new Picture();
                        picture.Provider = Provider.Local.Name;
                        picture.Id = Guid.NewGuid().ToString("N");
                        picture.Md5 = GetMD5HashFromFile(imageBytes);
                        picture.Tags = string.Empty;
                        picture.OverrideExtension(Path.GetExtension(imagePath).Remove(0, 1));
                        picture.IsLocallyImported = true;
                        picture.IsLocal = true;
                        picture.FileUrl = imagePath;
                        picture.Rating = Rating.Unknown.Name;
                        picture.Source = imagePath;

                        using (var ms = new MemoryStream(imageBytes.ToArray()))
                        {
                            var systemBitmap = new System.Drawing.Bitmap(ms);

                            picture.Width = systemBitmap.Width;
                            picture.Height = systemBitmap.Height;

                            var heightRatio = 150.0 / picture.Height;
                            var widthRatio = 150.0 / picture.Width;
                            var lowerRatio = heightRatio < widthRatio ? heightRatio : widthRatio;

                            picture.PreviewHeight = (int)(picture.Height * lowerRatio);
                            picture.PreviewWidth = (int)(picture.Width * lowerRatio);

                            if (picture.IsJpg)
                            {
                                try
                                {
                                    ms.Position = 0;
                                    var pictureMetadata = ImageFile.FromStream(ms);

                                    var tagProperty = pictureMetadata.Properties.Get(ExifTag.WindowsKeywords);
                                    if (tagProperty != null && tagProperty.Value != null)
                                    {
                                        picture.Tags = string.Join(" ", tagProperty.Value.ToString()?.Split(';').Select(s => s.Trim()));
                                    }
                                }
                                catch { }
                            }

                            using (var ms2 = new MemoryStream(ResizeImage(systemBitmap, picture.PreviewWidth, picture.PreviewHeight)))
                            {
                                ms2.Position = 0;
                                picture.PreviewImage = new Bitmap(ms2);
                            }

                            ms.Position = 0;
                            picture.SampleImage = new Bitmap(ms);
                            picture.SourceImage = picture.SampleImage;

                            systemBitmap.Dispose();
                            return picture;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async void OpenPictureInDefaultApplication(Picture picture, Workspace? workspace = null)
        {
            try
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        string filePath = null;
                        if (!string.IsNullOrEmpty(picture.FileUrl) && File.Exists(picture.FileUrl))
                        {
                            filePath = picture.FileUrl;
                        }
                        else
                        {
                            if (workspace?.DownloadSourcePictures == true && picture.SourceImage == null)
                            {
                                picture.SourceImage = await this.pictureDownloader.DownloadBitmap(picture.DownloadUrl);
                            }
                            else if (picture.SampleImage == null)
                            {
                                picture.SampleImage = await this.pictureDownloader.DownloadBitmap(picture.Url);
                            }

                            var selectedImage = (picture.SourceImage ?? picture.SampleImage);
                            if (selectedImage != null)
                            {
                                var temporaryFile = Path.Combine(Path.GetTempPath(), $"{picture.Md5}.{picture.Extension}");
                                selectedImage.Save(temporaryFile);

                                filePath = temporaryFile;
                            }
                        }

                        if (!string.IsNullOrEmpty(filePath))
                        {
                            System.Diagnostics.Process.Start(new ProcessStartInfo()
                            {
                                FileName = filePath,
                                UseShellExecute = true
                            });
                        }
                    }
                    catch (Exception)
                    {

                    }
                });
            }
            catch (Exception)
            {

            }
        }

        public async void OpenPictureWebsite(Picture picture)
        {
            try
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        string url = null;
                        var provider = Provider.Get(picture.Provider);
                        if (provider != null)
                        {
                            url = string.Format(provider.ImageUrl, picture.Id);
                        }

                        if (string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(picture.FileUrl))
                        {
                            url = $"file:///{picture.FileUrl.Replace("\\", "/")}";
                        }

                        if (!string.IsNullOrEmpty(url))
                        {
                            System.Diagnostics.Process.Start(new ProcessStartInfo()
                            {
                                FileName = url,
                                UseShellExecute = true
                            });
                        }
                    }
                    catch (Exception)
                    {

                    }
                });
            }
            catch (Exception)
            {

            }
        }

        public async Task DeletePicture(WorkspacePicture picture, Workspace workspace)
        {
            try
            {
                if (!string.IsNullOrEmpty(picture.FilePath) && File.Exists(picture.FilePath) && workspace.DeleteFileOnRemove)
                {
                    File.Delete(picture.FilePath);
                }
            }
            catch (Exception)
            {

            }
        }

        public async Task<string> SaveWorkspacePicture(Picture picture, Workspace workspace)
        {
            try
            {
                return await Task.Run<string>(async () =>
                {
                    try
                    {
                        if (workspace.DownloadSourcePictures && picture.SourceImage == null)
                        {
                            picture.SourceImage = await this.pictureDownloader.DownloadBitmap(picture.DownloadUrl);
                        }
                        else if (picture.SampleImage == null)
                        {
                            picture.SampleImage = await this.pictureDownloader.DownloadBitmap(picture.Url);
                        }

                        Bitmap imageBitmap = workspace.DownloadSourcePictures ? picture.SourceImage : picture.SampleImage;
                        if (imageBitmap == null)
                        {
                            return null;
                        }
                        else
                        {
                            using (var sourceStream = new MemoryStream())
                            {
                                using (var destStream = new MemoryStream())
                                {
                                    imageBitmap.Save(sourceStream);
                                    sourceStream.Position = 0;

                                    var drawingImage = new System.Drawing.Bitmap(sourceStream);
                                    var notJpg = !picture.IsJpg;

                                    var imageFormat = notJpg && workspace.ConvertToJpg ? ImageFormat.Jpeg : notJpg ? ImageFormat.Png : ImageFormat.Jpeg;
                                    picture.OverrideExtension(imageFormat == ImageFormat.Jpeg ? "jpg" : "png");

                                    var fileName = workspace.FileNameTemplate.ReplaceProperties(picture);
                                    var filePath = System.IO.Path.Combine(workspace.FolderPath, fileName);

                                    if (!System.IO.Directory.Exists(workspace.FolderPath))
                                    {
                                        Directory.CreateDirectory(workspace.FolderPath);
                                    }

                                    drawingImage.Save(destStream, imageFormat);
                                    destStream.Position = 0;

                                    if (workspace.InjectMetadata || workspace.InjectTags)
                                    {
                                        var exif = await ImageFile.FromStreamAsync(destStream);
                                        exif.Properties.Set(ExifTag.Software, "Tsuki-tag");

                                        if (workspace.InjectMetadata)
                                        {
                                            exif.Properties.Set(ExifTag.WindowsAuthor, picture.Author ?? string.Empty);
                                            exif.Properties.Set(ExifTag.PNGAuthor, picture.Author ?? string.Empty);
                                            exif.Properties.Set(ExifTag.WindowsTitle, picture.Title ?? string.Empty);
                                            exif.Properties.Set(ExifTag.ImageDescription, picture.Title ?? string.Empty);
                                            exif.Properties.Set(ExifTag.WindowsSubject, picture.Description ?? string.Empty);
                                            exif.Properties.Set(ExifTag.Copyright, picture.Copyright ?? string.Empty);
                                            exif.Properties.Set(ExifTag.WindowsComment, picture.Notes ?? string.Empty);
                                        }

                                        if (workspace.InjectTags)
                                        {
                                            exif.Properties.Set(ExifTag.WindowsKeywords, string.Join("; ", picture.TagList));
                                        }

                                        exif.Save(filePath);
                                    }
                                    else
                                    {
                                        File.WriteAllBytes(filePath, destStream.ToArray());
                                    }

                                    return filePath;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public byte[] ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new System.Drawing.Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = System.Drawing.Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
                }
            }

            using (var ms = new MemoryStream())
            {
                destImage.Save(ms, ImageFormat.Jpeg);
                ms.Position = 0;

                destImage.Dispose();
                return ms.ToArray();
            }
        }

        private string GetMD5HashFromFile(byte[] data)
        {
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var retVal = md5.ComputeHash(data);
            var sb = new StringBuilder();

            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
