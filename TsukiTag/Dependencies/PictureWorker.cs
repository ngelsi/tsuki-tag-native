using Avalonia.Media.Imaging;
using ExifLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        Task<string> SaveWorkspacePicture(Picture picture, Workspace workspace);

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
                                        exif.Properties.Set(ExifTag.Software, "TsukiTag");

                                        if (workspace.InjectMetadata)
                                        {
                                            exif.Properties.Set(ExifTag.WindowsKeywords, string.Join("; ", picture.TagList));
                                        }

                                        if (workspace.InjectTags)
                                        {
                                            exif.Properties.Set(ExifTag.WindowsComment, string.Join(Environment.NewLine, picture.Metadata));
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
    }
}
