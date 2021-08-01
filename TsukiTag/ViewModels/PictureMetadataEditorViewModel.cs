using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;

namespace TsukiTag.ViewModels
{
    public class PictureMetadataEditorViewModel : ViewModelBase
    {
        private readonly IPictureControl pictureControl;
        private readonly IProviderFilterControl providerFilterControl;
        private readonly INavigationControl navigationControl;

        private Picture picture;
        private string filterString;
        private string currentTag;

        public ReactiveCommand<Unit, Unit> AddTagCommand { get; }

        public string CurrentTag
        {
            get { return currentTag; }
            set
            {
                currentTag = value?.Replace(" ", "_") ?? string.Empty;
                this.RaisePropertyChanged(nameof(CurrentTag));
            }
        }

        public string FilterString
        {
            get { return filterString; }
            set
            {
                filterString = value;
                this.RaisePropertyChanged(nameof(FilterString));
                this.RaisePropertyChanged(nameof(FilteredTags));
                this.RaisePropertyChanged(nameof(TagCount));
            }
        }

        public Picture Picture
        {
            get { return picture; }
            set
            {
                picture = value;
                this.RaisePropertyChanged(nameof(Picture));
            }
        }

        public int TagCount => FilteredTags.Count;

        public List<string> FilteredTags
        {
            get
            {
                if (!string.IsNullOrEmpty(FilterString))
                {
                    var filterParts = FilterString.Split(' ').Where(s => !string.IsNullOrEmpty(s));
                    return Picture?.TagList.Where(s => filterParts.Any(fs => s.IndexOf(fs) > -1)).ToList() ?? new List<string>();
                }

                return Picture?.TagList?.ToList() ?? new List<string>();
            }
        }

        public PictureMetadataEditorViewModel(
            Picture picture,
            IPictureControl pictureControl,
            IProviderFilterControl providerFilterControl,
            INavigationControl navigationControl
        )
        {
            this.picture = picture;
            this.pictureControl = pictureControl;
            this.navigationControl = navigationControl;
            this.providerFilterControl = providerFilterControl;

            this.AddTagCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnTagAdded();
            });
        }

        ~PictureMetadataEditorViewModel()
        {

        }

        public async void OnTagRemoved(string tag)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                Picture.RemoveTag(tag);

                this.RaisePropertyChanged(nameof(Picture));
                this.RaisePropertyChanged(nameof(Picture.TagList));
                this.RaisePropertyChanged(nameof(Picture.Tags));
                this.RaisePropertyChanged(nameof(FilteredTags));
                this.RaisePropertyChanged(nameof(TagCount));
            });
        }

        public async void OnFilterTagAdded(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.AddTag(tag);
                await this.navigationControl.SwitchToBrowsingTab();
            });
        }

        public async void OnFilterTagRemoved(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.RemoveTag(tag);
                await this.navigationControl.SwitchToBrowsingTab();
            });
        }

        public async void OnFilterTagClicked(string tag)
        {
            await Task.Run(async () =>
            {
                await this.providerFilterControl.SetTag(tag);
                await this.navigationControl.SwitchToBrowsingTab();
            });
        }

        public async void OnTagAdded()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if (!string.IsNullOrEmpty(CurrentTag) && !Picture.TagList.Contains(CurrentTag))
                {
                    Picture.AddTag(CurrentTag);

                    this.RaisePropertyChanged(nameof(Picture));
                    this.RaisePropertyChanged(nameof(Picture.TagList));
                    this.RaisePropertyChanged(nameof(Picture.Tags));
                    this.RaisePropertyChanged(nameof(FilteredTags));
                    this.RaisePropertyChanged(nameof(TagCount));
                }

                CurrentTag = string.Empty;
            });
        }
    }
}
