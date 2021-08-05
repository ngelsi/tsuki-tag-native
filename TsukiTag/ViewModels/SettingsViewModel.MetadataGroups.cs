using Avalonia;
using Avalonia.Controls;
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
        private ObservableCollection<MetadataGroup> metadataGroups;

        public ObservableCollection<MetadataGroup> MetadataGroups
        {
            get { return metadataGroups; }
            set
            {
                metadataGroups = value;
                this.RaisePropertyChanged(nameof(MetadataGroups));
            }
        }

        public ReactiveCommand<Unit, Unit> AddNewMetadataGroupCommand { get; set; }
        public ReactiveCommand<Guid, Unit> SetMetadataGroupToDefaultCommand { get; set; }
        public ReactiveCommand<Guid, Unit> RemoveMetadataGroupCommand { get; set; }


        public async void OnNewMetadataGroupAdded()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                MetadataGroups.Add(new MetadataGroup()
                {
                    Id = Guid.NewGuid(),
                    IsDefault = metadataGroups == null || metadataGroups.Count == 0,
                    Name = Language.MetadataGroup + " " + (metadataGroups.Count + 1),
                    Notes = string.Join("\r\n", new string[]
                    {
                        $"PROVIDER: #provider#",
                        $"RATING: #rating#",
                        $"ID: #id#",
                        $"MD5: #md5#"
                    })
                });
            });
        }

        public async void OnSetMetadataGroupToDefault(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                foreach (var metadataGroup in MetadataGroups)
                {
                    metadataGroup.IsDefault = metadataGroup.Id == id;
                }
            });
        }

        public async void OnRemoveMetadataGroup(Guid id)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var metadataGroup = MetadataGroups.FirstOrDefault(l => l.Id == id);
                if (metadataGroup != null)
                {
                    metadataGroups.Remove(metadataGroup);
                }
            });
        }
    }
}
