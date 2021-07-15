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

namespace TsukiTag.ViewModels
{
    public class TagOverviewViewModel : ViewModelBase
    {
        private readonly IPictureControl pictureControl;
        private readonly IProviderFilterControl providerFilterControl;

        private TagCollection tags;
        private string filterString;

        

        public string FilterString
        {
            get { return filterString; }
            set { filterString = value; this.RaisePropertyChanged(nameof(FilterString)); this.RaisePropertyChanged(nameof(FilteredTags)); }
        }

        public TagCollection Tags
        {
            get { return tags; }
            set { tags = value; this.RaisePropertyChanged(nameof(Tags)); this.RaisePropertyChanged(nameof(FilteredTags)); }
        }

        public TagCollection FilteredTags
        {
            get
            {
                if (!string.IsNullOrEmpty(FilterString))
                {
                    var filterParts = FilterString.Split(' ').Where(s => !string.IsNullOrEmpty(s));
                    return new TagCollection() { Tags = Tags.Tags.Where(s => filterParts.Any(fs => s.Tag.IndexOf(fs) > -1)).ToList() };
                }

                return Tags;
            }
        }

        public TagOverviewViewModel(
            IPictureControl pictureControl,
            IProviderFilterControl providerFilterControl
        )
        {

            this.pictureControl = pictureControl;
            this.providerFilterControl = providerFilterControl;

            this.pictureControl.PictureAdded += OnPictureAdded;
            this.pictureControl.PictureRemoved += OnPictureRemoved;
            this.pictureControl.PicturesReset += OnPicturesReset;
        }



        ~TagOverviewViewModel()
        {
            this.pictureControl.PictureAdded -= OnPictureAdded;
            this.pictureControl.PictureRemoved -= OnPictureRemoved;
            this.pictureControl.PicturesReset -= OnPicturesReset;
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

        private async void OnPicturesReset(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                Tags = await pictureControl.GetTags();
            });
        }

        private async void OnPictureRemoved(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                Tags = await pictureControl.GetTags();
            });
        }

        private async void OnPictureAdded(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                Tags = await pictureControl.GetTags();
            });
        }
    }
}
