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
    public abstract class PictureResourceList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected string name;
        protected string[] tagsToAdd;
        protected string[] tagsToRemove;
        protected bool isDefault;
        protected string currentTagToAdd;
        protected string currentTagToRemove;
        protected string[] optionalConditionTags;
        protected string[] mandatoryConditionTags;
        protected string currentOptionalConditionTag;
        protected string currentMandatoryConditionTag;

        public Guid Id { get; set; }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        [BsonIgnore]
        public string CurrentTagToAdd
        {
            get { return currentTagToAdd; }
            set
            {
                currentTagToAdd = value?.Trim()?.Replace(" ", "_");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTagToAdd)));
            }
        }

        [BsonIgnore]
        public string CurrentTagToRemove
        {
            get { return currentTagToRemove; }
            set
            {
                currentTagToRemove = value?.Trim()?.Replace(" ", "_");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTagToRemove)));
            }
        }

        [BsonIgnore]
        public string CurrentOptionalConditionTag
        {
            get { return currentOptionalConditionTag; }
            set
            {
                currentOptionalConditionTag = value?.Trim()?.Replace(" ", "_");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentOptionalConditionTag)));
            }
        }

        [BsonIgnore]
        public string CurrentMandatoryConditionTag
        {
            get { return currentMandatoryConditionTag; }
            set
            {
                currentMandatoryConditionTag = value?.Trim()?.Replace(" ", "_");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentMandatoryConditionTag)));
            }
        }

        public string[] TagsToAdd
        {
            get { return tagsToAdd; }
            set
            {
                tagsToAdd = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TagsToAdd)));
            }
        }

        public string[] TagsToRemove
        {
            get { return tagsToRemove; }
            set
            {
                tagsToRemove = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TagsToRemove)));
            }
        }

        public string[] OptionalConditionTags
        {
            get { return optionalConditionTags; }
            set
            {
                optionalConditionTags = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OptionalConditionTags)));
            }
        }

        public string[] MandatoryConditionTags
        {
            get { return mandatoryConditionTags; }
            set
            {
                mandatoryConditionTags = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MandatoryConditionTags)));
            }
        }

        public bool IsDefault
        {
            get { return isDefault; }
            set
            {
                isDefault = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDefault)));
            }
        }

        public bool IsEligible(Picture picture)
        {
            return optionalConditionTags?.Any(t => picture.TagList.Any(tt => tt.WildcardMatches(t))) == true ||
                   mandatoryConditionTags?.All(t => picture.TagList.Any(tt => tt.WildcardMatches(t))) == true;
        }

        public Picture ProcessPicture(Picture picture)
        {
            if (TagsToAdd != null && TagsToAdd?.Any() == true)
            {
                foreach (var tag in TagsToAdd)
                {
                    var check = tag.ReplaceProperties(picture);

                    if (!picture.TagList.Contains(check))
                    {
                        picture.AddTag(check);
                    }
                }
            }

            if (TagsToRemove != null && TagsToRemove?.Any() == true)
            {
                foreach (var tag in TagsToRemove)
                {
                    var check = tag.ReplaceProperties(picture);

                    if (picture.TagList.Contains(check))
                    {
                        picture.RemoveTag(check);
                    }
                }
            }

            return picture;
        }

        protected void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
