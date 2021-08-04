using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;

namespace TsukiTag.ViewModels
{
    public class TagBarViewModel : ViewModelBase
    {
        private readonly IProviderFilterControl providerFilterControl;
        private readonly IPictureControl pictureControl;

        private string tagString;
        private ConcurrentDictionary<string, byte> seenTags;       
        private ObservableCollection<string> currentTags;
        private ObservableCollection<string> tagSuggestions;
        private ObservableCollection<string> currentExcludedTags;
        private string excludedTagString;

        public ObservableCollection<string> CurrentTags
        {
            get { return currentTags; }
            set { currentTags = value; this.RaisePropertyChanged(nameof(CurrentTags)); }
        }

        public ObservableCollection<string> CurrentExcludedTags
        {
            get { return currentExcludedTags; }
            set { currentExcludedTags = value; this.RaisePropertyChanged(nameof(CurrentExcludedTags)); }
        }

        public ObservableCollection<string> TagSuggestions
        {
            get { return tagSuggestions; }
            set { tagSuggestions = value; this.RaisePropertyChanged(nameof(TagSuggestions)); }
        }

        public string TagString
        {
            get { return tagString; }
            set
            {
                tagString = value; this.RaisePropertyChanged((nameof(TagString)));
            }
        }

        public string ExcludedTagString
        {
            get { return excludedTagString; }
            set { excludedTagString = value; this.RaisePropertyChanged(nameof(ExcludedTagString)); }
        }

        public TagBarViewModel(
            IProviderFilterControl providerFilterControl,
            IPictureControl pictureControl
        )
        {
            this.seenTags = new ConcurrentDictionary<string, byte>();
            this.tagSuggestions = new ObservableCollection<string>();

            this.providerFilterControl = providerFilterControl;
            this.providerFilterControl.FilterChanged += OnFilterChanged;

            this.pictureControl = pictureControl;
            this.pictureControl.PictureAdded += OnPictureAdded;
        }

        ~TagBarViewModel()
        {
            this.providerFilterControl.FilterChanged -= OnFilterChanged;
            this.pictureControl.PictureAdded -= OnPictureAdded;
        }

        public async void OnTagClicked(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.RemoveTag(tag);
            });
        }

        public async void OnExcludeTagClicked(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.RemoveExcludeTag(tag);
            });
        }

        public async void OnTagAdded(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.AddTag(tag);
            });

            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                TagString = string.Empty;
            });
        }

        public async void OnExcludeTagAdded(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.AddExcludeTag(tag);
            });

            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                ExcludedTagString = string.Empty;
            });
        }

        public async void OnAutoCompleteInitiated(string filter)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                TagString = tagSuggestions.Select(t => new { index = t.IndexOf(filter, StringComparison.OrdinalIgnoreCase), value = t }).Where(t => t.index > -1).OrderBy(t => t.index).FirstOrDefault()?.value ?? filter;
            });
        }

        public async void OnExcludeAutoCompleteInitiated(string filter)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                ExcludedTagString = tagSuggestions.Select(t => new { index = t.IndexOf(filter, StringComparison.OrdinalIgnoreCase), value = t }).Where(t => t.index > -1).OrderBy(t => t.index).FirstOrDefault()?.value ?? filter;
            });
        }

        private async void OnPictureAdded(object? sender, Picture e)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var tags = await this.pictureControl.GetTags();
                    foreach (var tag in tags.Tags)
                    {
                        if(!seenTags.ContainsKey(tag.Tag))
                        {
                            seenTags.TryAdd(tag.Tag, 1);
                        }
                    }

                    TagSuggestions = new ObservableCollection<string>(seenTags.Keys);
                }
                catch { }
            });
        }

        private async void OnFilterChanged(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var filter = await this.providerFilterControl.GetCurrentFilter();

                CurrentTags = new ObservableCollection<string>(filter.Tags);
                CurrentExcludedTags = new ObservableCollection<string>(filter.ExcludedTags);
            });
        }
    }
}
