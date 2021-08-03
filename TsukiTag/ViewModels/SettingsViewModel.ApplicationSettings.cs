using LiteDB;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;
using TsukiTag.Models.Repository;


namespace TsukiTag.ViewModels
{
    public partial class SettingsViewModel
    {
        private ApplicationSettings applicationSettings;

        public ReactiveCommand<Unit, Unit> AddBlacklistTagCommand { get; set; }


        public ApplicationSettings ApplicationSettings
        {
            get { return applicationSettings; }
            set
            {
                applicationSettings = value;
                this.RaisePropertyChanged(nameof(ApplicationSettings));
            }
        }

        public async void OnAddBlacklistTag()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if(!string.IsNullOrEmpty(applicationSettings.CurrentBlacklistTag))
                {
                    applicationSettings.BlacklistTags = applicationSettings.BlacklistTags == null ? new string[] { applicationSettings.CurrentBlacklistTag } : applicationSettings.BlacklistTags.Append(applicationSettings.CurrentBlacklistTag).Distinct().ToArray();
                    applicationSettings.CurrentBlacklistTag = string.Empty;
                }                
            });
        }

        public async void OnRemoveBlacklistTag(string tag)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                applicationSettings.BlacklistTags = applicationSettings.BlacklistTags.Except(new string[] { tag }).ToArray();
            });
        }
    }
}
