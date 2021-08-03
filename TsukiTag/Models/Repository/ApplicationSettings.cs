using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models.Repository
{
    public class ApplicationSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool closeTabsOnContextSwitch;
        private bool jumpToBrowserTabOnClose;
        private bool deselectPicturesOnContextSwitch;
        private string[] blacklistTags;
        private string currentBlacklistTag;

        public string Id { get; set; }

        [BsonIgnore]
        public string CurrentBlacklistTag
        {
            get { return currentBlacklistTag; }
            set
            {
                currentBlacklistTag = value?.Trim()?.Replace(" ", "_");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBlacklistTag)));
            }
        }

        public bool CloseTabsOnContextSwitch
        {
            get { return closeTabsOnContextSwitch; }
            set
            {
                closeTabsOnContextSwitch = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CloseTabsOnContextSwitch)));
            }
        }

        public bool DeselectPicturesOnContextSwitch
        {
            get { return deselectPicturesOnContextSwitch; }
            set
            {
                deselectPicturesOnContextSwitch = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeselectPicturesOnContextSwitch)));
            }
        }

        public bool JumpToBrowserTabOnClose
        {
            get { return jumpToBrowserTabOnClose; }
            set
            {
                jumpToBrowserTabOnClose = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JumpToBrowserTabOnClose)));
            }
        }

        public string[] BlacklistTags
        {
            get { return blacklistTags; }
            set
            {
                blacklistTags = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BlacklistTags)));
            }
        }


    }
}
