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
using TsukiTag.Extensions;
using TsukiTag.Models;

namespace TsukiTag.ViewModels
{
    public class TagOverviewViewModel : ViewModelBase
    {
        private readonly IPictureControl pictureControl;
        private readonly IProviderFilterControl providerFilterControl;
        private readonly INavigationControl navigationControl;

        private TagCollection tags;
        private string filterString;
        private bool hasSelectedPictures;

        public ReactiveCommand<Unit, Unit> SwitchToMetadataOverviewCommand { get; set; }

        public bool HasSelectedPictures
        {
            get { return hasSelectedPictures; }
            set
            {
                hasSelectedPictures = value;
                this.RaisePropertyChanged(nameof(HasSelectedPictures));
            }
        }

        public string FilterString
        {
            get { return filterString; }
            set { filterString = value; this.RaisePropertyChanged(nameof(FilterString)); this.RaisePropertyChanged(nameof(FilteredTags)); }
        }

        public TagCollection Tags
        {
            get { return tags; }
            set { tags = value; this.RaisePropertyChanged(nameof(Tags)); this.RaisePropertyChanged(nameof(FilteredTags)); this.RaisePropertyChanged(nameof(FilteredTags.Tags)); this.RaisePropertyChanged(nameof(FilteredTags.TagCount)); }
        }

        public TagCollection FilteredTags
        {
            get
            {
                if (!string.IsNullOrEmpty(FilterString))
                {
                    var filterParts = FilterString.Split(' ').Where(s => !string.IsNullOrEmpty(s));
                    return new TagCollection() { Tags = Tags.Tags.Where(s => filterParts.Any(fs => s.Tag.WildcardMatchesEx(fs))).ToList() };
                }

                return Tags;
            }
        }

        public TagOverviewViewModel(
            IPictureControl pictureControl,
            IProviderFilterControl providerFilterControl,
            INavigationControl navigationControl
        )
        {
            this.navigationControl = navigationControl;
            this.pictureControl = pictureControl;
            this.providerFilterControl = providerFilterControl;

            this.pictureControl.PictureAdded += OnPictureAdded;
            this.pictureControl.PictureRemoved += OnPictureRemoved;
            this.pictureControl.PicturesReset += OnPicturesReset;
            this.pictureControl.PictureSelected += OnPictureSelected;
            this.pictureControl.PictureDeselected += OnPictureDeselected;

            this.SwitchToMetadataOverviewCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnSwitchToMetadataOverview();
            });
        }

        ~TagOverviewViewModel()
        {
            this.pictureControl.PictureAdded -= OnPictureAdded;
            this.pictureControl.PictureRemoved -= OnPictureRemoved;
            this.pictureControl.PicturesReset -= OnPicturesReset;
            this.pictureControl.PictureSelected -= OnPictureSelected;
            this.pictureControl.PictureDeselected -= OnPictureDeselected;
        }

        public async void OnTagClicked(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.SetTag(tag);
            });
        }

        public async void OnTagAdded(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.AddTag(tag);
            });
        }

        public async void OnTagRemoved(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.RemoveTag(tag);
            });
        }

        private async void OnSwitchToMetadataOverview()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.navigationControl.SwitchToMetadataOverview();
            });
        }

        private async void OnPicturesReset(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                try
                {
                    Tags = await pictureControl.GetTags();
                }
                catch { }
            });
        }

        private async void OnPictureRemoved(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                try
                {
                    Tags = await pictureControl.GetTags();
                }
                catch { }
            });
        }

        private async void OnPictureAdded(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                try
                {
                    Tags = await pictureControl.GetTags();
                }
                catch { }
            });
        }


        private async void OnPictureDeselected(object? sender, Picture e)
        {
            this.HasSelectedPictures = await this.pictureControl.GetSelectedPictureCount() > 0;
        }

        private async void OnPictureSelected(object? sender, Picture e)
        {
            this.HasSelectedPictures = await this.pictureControl.GetSelectedPictureCount() > 0;
        }
    }
}
