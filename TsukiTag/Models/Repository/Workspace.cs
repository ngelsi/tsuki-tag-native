using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Extensions;


namespace TsukiTag.Models.Repository
{
    public class Workspace : PictureResourceList
    {
        private bool downloadSourcePictures;
        private bool convertToJpg;
        private bool injectTags;
        private bool injectMetadata;
        private string folderPath;
        private string fileNameTemplate;
        private bool deleteFileOnRemove;

        public string FolderPath
        {
            get { return folderPath; }
            set
            {
                folderPath = value;
                NotifyPropertyChanged(nameof(FolderPath));
            }
        }

        public string FileNameTemplate
        {
            get { return fileNameTemplate; }
            set
            {
                fileNameTemplate = value;
                NotifyPropertyChanged(nameof(FileNameTemplate));
            }
        }

        public bool DownloadSourcePictures
        {
            get { return downloadSourcePictures; }
            set
            {
                downloadSourcePictures = value;
                NotifyPropertyChanged(nameof(DownloadSourcePictures));
            }
        }

        public bool ConvertToJpg
        {
            get { return convertToJpg; }
            set
            {
                convertToJpg = value;
                NotifyPropertyChanged(nameof(ConvertToJpg));
            }
        }

        public bool InjectTags
        {
            get { return injectTags; }
            set
            {
                injectTags = value;

                NotifyPropertyChanged(nameof(InjectTags));

                if (value == true)
                {
                    ConvertToJpg = true;
                }
            }
        }

        public bool InjectMetadata
        {
            get { return injectMetadata; }
            set
            {
                injectMetadata = value;

                NotifyPropertyChanged(nameof(InjectMetadata));

                if (value == true)
                {
                    ConvertToJpg = true;
                }
            }
        }

        public bool DeleteFileOnRemove
        {
            get { return deleteFileOnRemove; }
            set
            {
                deleteFileOnRemove = value;
                NotifyPropertyChanged(nameof(DeleteFileOnRemove));
            }
        }
    }
}
