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
        private bool autoApplyMetadataGroup;
        private Guid? metadataGroupId;
        private MetadataGroup metadataGroup;

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

                if (value == false)
                {
                    InjectTags = false;
                    InjectMetadata = false;
                }
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

        public bool AutoApplyMetadataGroup
        {
            get { return autoApplyMetadataGroup; }
            set
            {
                autoApplyMetadataGroup = value;
                NotifyPropertyChanged(nameof(AutoApplyMetadataGroup));
            }
        }

        public Guid? MetadataGroupId
        {
            get { return metadataGroupId; }
            set
            {
                metadataGroupId = value;
                NotifyPropertyChanged(nameof(MetadataGroupId));
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

        [BsonIgnore]
        public MetadataGroup MetadataGroup
        {
            get { return metadataGroup; }
            set
            {
                metadataGroup = value;

                if (value != null)
                {
                    metadataGroupId = value?.Id;
                }

                NotifyPropertyChanged(nameof(MetadataGroupId));
                NotifyPropertyChanged(nameof(AutoApplyMetadataGroup));
                NotifyPropertyChanged(nameof(MetadataGroup));
            }
        }

        public override Picture ProcessPicture(Picture picture)
        {
            if(MetadataGroup != null && AutoApplyMetadataGroup)
            {
                picture.Author = metadataGroup?.Author != null ? metadataGroup.Author?.ReplaceProperties(picture) : picture.Author;
                picture.Title = metadataGroup.Title?.ReplaceProperties(picture);
                picture.Description = metadataGroup.Description?.ReplaceProperties(picture);
                picture.Copyright = metadataGroup.Copyright?.ReplaceProperties(picture);
                picture.Notes = metadataGroup.Notes?.ReplaceProperties(picture);
            }

            return base.ProcessPicture(picture);
        }
    }
}
