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
    public class OnlineNavigationBarViewModel : ViewModelBase
    {
        private bool canAdvanceToPreviousPage;
        private bool canAdvanceToNextPage;

        private readonly IProviderFilterControl providerFilterControl;
        private bool safeRatingEnabled;
        private bool questionableRatingEnabled;
        private bool explicitRatingEnabled;
        private bool safebooruEnabled;
        private bool gelbooruEnabled;
        private bool konachanEnabled;

        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }
        public ReactiveCommand<string, Unit> SwitchRatingCommand { get; }
        public ReactiveCommand<string, Unit> SwitchProviderCommand { get; }


        public bool SafebooruEnabled
        {
            get { return safebooruEnabled; }
            set
            {
                safebooruEnabled = value;
                this.RaisePropertyChanged(nameof(SafebooruEnabled));
            }
        }

        public bool GelbooruEnabled
        {
            get { return gelbooruEnabled; }
            set
            {
                gelbooruEnabled = value;
                this.RaisePropertyChanged(nameof(GelbooruEnabled));
            }
        }

        public bool KonachanEnabled
        {
            get { return konachanEnabled; }
            set
            {
                konachanEnabled = value;
                this.RaisePropertyChanged(nameof(KonachanEnabled));
            }
        }

        public bool SafeRatingEnabled
        {
            get { return safeRatingEnabled; }
            set
            {
                safeRatingEnabled = value;
                this.RaisePropertyChanged(nameof(SafeRatingEnabled));
            }
        }

        public bool QuestionableRatingEnabled
        {
            get { return questionableRatingEnabled; }
            set
            {
                questionableRatingEnabled = value;
                this.RaisePropertyChanged(nameof(QuestionableRatingEnabled));
            }
        }

        public bool ExplicitRatingEnabled
        {
            get { return explicitRatingEnabled; }
            set
            {
                explicitRatingEnabled = value;
                this.RaisePropertyChanged(nameof(ExplicitRatingEnabled));
            }
        }

        public bool CanAdvanceToPreviousPage
        {
            get { return canAdvanceToPreviousPage; }
            set
            {
                this.canAdvanceToPreviousPage = value;
                this.RaisePropertyChanged(nameof(CanAdvanceToPreviousPage));
            }
        }

        public bool CanAdvanceToNextPage
        {
            get { return canAdvanceToNextPage; }
            set
            {
                this.canAdvanceToNextPage = value;
                this.RaisePropertyChanged(nameof(CanAdvanceToNextPage));
            }
        }

        public OnlineNavigationBarViewModel(
            IProviderFilterControl providerFilterControl
        )
        {
            this.providerFilterControl = providerFilterControl;
            this.providerFilterControl.PageChanged += OnPageChanged;
            this.providerFilterControl.FilterChanged += OnFilterChanged;

            this.canAdvanceToNextPage = true;

            this.NextPageCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.providerFilterControl.NextPage();
            });

            this.PreviousPageCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.providerFilterControl.PreviousPage();
            });

            this.SwitchRatingCommand = ReactiveCommand.CreateFromTask<string>(async (rating) =>
            {
                await this.SwitchRating(rating);
            });

            this.SwitchProviderCommand = ReactiveCommand.CreateFromTask<string>(async (provider) =>
            {
                await this.SwitchProvider(provider);
            });


            this.ConfigureFilters();
        }

        ~OnlineNavigationBarViewModel()
        {
            this.providerFilterControl.PageChanged -= OnPageChanged;
            this.providerFilterControl.FilterChanged -= OnFilterChanged;
        }

        private async Task SwitchRating(string rating)
        {
            var filter = await this.providerFilterControl.GetCurrentFilter();
            if (filter.Ratings.Contains(rating))
            {
                await this.providerFilterControl.RemoveRating(rating);
            }
            else
            {
                await this.providerFilterControl.AddRating(rating);
            }
        }

        private async Task SwitchProvider(string provider)
        {
            var filter = await this.providerFilterControl.GetCurrentFilter();
            if (filter.Providers.Contains(provider))
            {
                await this.providerFilterControl.RemoveProvider(provider);
            }
            else
            {
                await this.providerFilterControl.AddProvider(provider);
            }
        }

        private async void ConfigureFilters()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var currentFilter = await this.providerFilterControl.GetCurrentFilter();

                SafeRatingEnabled = currentFilter.Ratings.Contains(Rating.Safe.Name);
                QuestionableRatingEnabled = currentFilter.Ratings.Contains(Rating.Questionable.Name);
                ExplicitRatingEnabled = currentFilter.Ratings.Contains(Rating.Explicit.Name);

                SafebooruEnabled = currentFilter.Providers.Contains(Provider.Safebooru.Name);
                GelbooruEnabled = currentFilter.Providers.Contains(Provider.Gelbooru.Name);
                KonachanEnabled = currentFilter.Providers.Contains(Provider.Konachan.Name);
            });
        }

        private void OnPageChanged(object? sender, int e)
        {
            this.CheckPageAdvancements();
        }

        private void OnFilterChanged(object? sender, EventArgs e)
        {
            this.CheckPageAdvancements();
            this.ConfigureFilters();
        }

        private void CheckPageAdvancements()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                this.CanAdvanceToNextPage = this.providerFilterControl.CanAdvanceNextPage();
                this.CanAdvanceToPreviousPage = this.providerFilterControl.CanAdvancePreviousPage();

                this.RaisePropertyChanged(nameof(CanAdvanceToNextPage));
                this.RaisePropertyChanged(nameof(CanAdvanceToPreviousPage));
            });
        }
    }
}
