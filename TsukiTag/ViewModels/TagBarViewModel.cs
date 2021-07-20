using ReactiveUI;
using System;
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
        private TagCollection tagCollection;
        private ObservableCollection<string> currentTags;
        private ObservableCollection<string> tagSuggestions;

        public ObservableCollection<string> CurrentTags
        {
            get { return currentTags; }
            set { currentTags = value; this.RaisePropertyChanged(nameof(CurrentTags)); }
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

        public TagBarViewModel(
            IProviderFilterControl providerFilterControl,
            IPictureControl pictureControl
        )
        {
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

        public async void OnAutoCompleteInitiated(string filter)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {               
                TagString = tagSuggestions.Select(t => new { index = t.IndexOf(filter, StringComparison.OrdinalIgnoreCase), value = t }).Where(t => t.index > -1).OrderBy(t => t.index).FirstOrDefault()?.value ?? filter;
            });
        }

        private async void OnPictureAdded(object? sender, Picture e)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var tags = await this.pictureControl.GetTags();
                    var tagStrings = tags.Tags.Select(s => s.Tag);
                    var newList = tagStrings.Concat(TagSuggestions).Distinct().ToList();

                    TagSuggestions = new ObservableCollection<string>(newList);
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
            });
        }
    }
}
