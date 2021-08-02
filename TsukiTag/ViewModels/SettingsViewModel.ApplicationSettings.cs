using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models.Repository;

namespace TsukiTag.ViewModels
{
    public partial class SettingsViewModel
    {
        private ApplicationSettings applicationSettings;

        public ApplicationSettings ApplicationSettings
        {
            get { return applicationSettings; }
            set
            {
                applicationSettings = value;
                this.RaisePropertyChanged(nameof(ApplicationSettings));
            }
        }


    }
}
