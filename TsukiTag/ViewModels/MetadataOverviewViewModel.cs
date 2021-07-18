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
    public class MetadataOverviewViewModel : ViewModelBase
    {
        private readonly IPictureControl pictureControl;
        private readonly IProviderFilterControl providerFilterControl;
        private readonly INavigationControl navigationControl;

        private string filterString;
        private int currentPictureIndex;

        public ObservableCollection<Picture> SelectedPictures { get; set; }

        public bool HasSelectedPicture => SelectedPictures.Count > 0;

        public bool HasMultipleSelectedPicture => SelectedPictures.Count > 1;

        public int SelectedPictureCount => SelectedPictures.Count;

        public Picture CurrentPicture => SelectedPictures.Count != 0 && SelectedPictures.Count >= CurrentPictureIndex ? SelectedPictures[CurrentPictureIndex] : null;

        public int CurrentPictureIndex
        {
            get { return currentPictureIndex; }
            set
            {
                currentPictureIndex = value;

                this.RaisePropertyChanged(nameof(CurrentPictureIndex));
                this.RaisePropertyChanged(nameof(CurrentPictureIndexDisplay));
                this.RaisePropertyChanged(nameof(CurrentPicture));
                this.RaisePropertyChanged(nameof(FilterString));
                this.RaisePropertyChanged(nameof(FilteredTags));
                this.RaisePropertyChanged(nameof(TagCount));
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

        public int TagCount => FilteredTags.Count;

        public List<string> FilteredTags
        {
            get
            {
                if (!string.IsNullOrEmpty(FilterString))
                {
                    var filterParts = FilterString.Split(' ').Where(s => !string.IsNullOrEmpty(s));
                    return CurrentPicture?.TagList.Where(s => filterParts.Any(fs => s.IndexOf(fs) > -1)).ToList() ?? new List<string>();
                }

                return CurrentPicture?.TagList ?? new List<string>();
            }
        }

        public string CurrentPictureIndexDisplay => (currentPictureIndex + 1).ToString();

        public ReactiveCommand<Unit, Unit> PreviousPictureCommand { get; set; }
        public ReactiveCommand<Unit, Unit> NextPictureCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeselectPictureCommand { get; set; }
        public ReactiveCommand<Unit, Unit> OpenPictureCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SwitchToTabOverviewCommand { get; set; }

        public MetadataOverviewViewModel(
            IPictureControl pictureControl,
            IProviderFilterControl providerFilterControl,
            INavigationControl navigationControl
        )
        {
            this.SelectedPictures = new ObservableCollection<Picture>();

            this.pictureControl = pictureControl;
            this.providerFilterControl = providerFilterControl;
            this.navigationControl = navigationControl;

            this.pictureControl.PictureSelected += OnPictureSelected;
            this.pictureControl.PictureDeselected += OnPictureDeselected;
            //this.pictureControl.PicturesReset += OnPicturesReset;

            this.PreviousPictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnPreviousPicture();
            });

            this.NextPictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnNextPicture();
            });

            this.DeselectPictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnDeselectPicture();
            });

            this.OpenPictureCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnOpenPicture();
            });

            this.SwitchToTabOverviewCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                this.OnSwitchToTagOverview();
            });
        }

        ~MetadataOverviewViewModel()
        {
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

        public async void OnNextPicture()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if ((CurrentPictureIndex + 1) < SelectedPictures.Count)
                {
                    CurrentPictureIndex += 1;
                }
            });
        }

        public async void OnOpenPicture()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var picture = SelectedPictures.ElementAt(CurrentPictureIndex);
                if (picture != null)
                {
                    this.pictureControl.OpenPicture(picture);
                }
            });
        }

        private async void OnSwitchToTagOverview()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.navigationControl.SwitchToTagOverview();
            });
        }

        public async void OnOpenPictureInBackground()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var picture = SelectedPictures.ElementAt(CurrentPictureIndex);
                if (picture != null)
                {
                    this.pictureControl.OpenPictureInBackground(picture);
                }
            });
        }

        public async void OnDeselectPicture()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var picture = SelectedPictures.ElementAt(CurrentPictureIndex);
                if (picture != null)
                {
                    this.pictureControl.DeselectPicture(picture);
                }
            });
        }

        public async void OnPreviousPicture()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                if ((CurrentPictureIndex - 1) >= 0)
                {
                    CurrentPictureIndex -= 1;
                }
            });
        }

        private void OnPicturesReset(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                SelectedPictures = new ObservableCollection<Picture>();

                this.RaisePropertyChanged(nameof(CurrentPicture));
                this.RaisePropertyChanged(nameof(SelectedPictures));
                this.RaisePropertyChanged(nameof(SelectedPictureCount));
                this.RaisePropertyChanged(nameof(CurrentPictureIndex));
                this.RaisePropertyChanged(nameof(HasSelectedPicture));
                this.RaisePropertyChanged(nameof(HasMultipleSelectedPicture));
                this.RaisePropertyChanged(nameof(CurrentPictureIndexDisplay));
            });
        }

        private void OnPictureDeselected(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                SelectedPictures.Remove(e);

                if (CurrentPictureIndex >= SelectedPictures.Count)
                {
                    CurrentPictureIndex = SelectedPictures.Count - 1;
                }

                this.RaisePropertyChanged(nameof(CurrentPicture));
                this.RaisePropertyChanged(nameof(SelectedPictures));
                this.RaisePropertyChanged(nameof(SelectedPictureCount));
                this.RaisePropertyChanged(nameof(CurrentPictureIndex));
                this.RaisePropertyChanged(nameof(HasSelectedPicture));
                this.RaisePropertyChanged(nameof(HasMultipleSelectedPicture));
            });
        }

        private void OnPictureSelected(object? sender, Picture e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var picture = SelectedPictures.FirstOrDefault(p => p.Md5 == e.Md5);
                if (picture != null)
                {
                    CurrentPictureIndex = SelectedPictures.IndexOf(picture);
                }
                else
                {
                    SelectedPictures.Add(e);
                    CurrentPictureIndex = SelectedPictures.Count - 1;
                }

                this.RaisePropertyChanged(nameof(CurrentPicture));
                this.RaisePropertyChanged(nameof(SelectedPictures));
                this.RaisePropertyChanged(nameof(SelectedPictureCount));
                this.RaisePropertyChanged(nameof(CurrentPictureIndex));
                this.RaisePropertyChanged(nameof(HasSelectedPicture));
                this.RaisePropertyChanged(nameof(HasMultipleSelectedPicture));
                this.RaisePropertyChanged(nameof(CurrentPictureIndexDisplay));
            });
        }
    }
}
