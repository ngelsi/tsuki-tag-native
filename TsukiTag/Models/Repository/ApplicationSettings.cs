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

        public string Id { get; set; }

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


    }
}
