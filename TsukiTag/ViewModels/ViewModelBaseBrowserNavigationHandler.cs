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
    public class ViewModelBaseBrowserNavigationHandler : ViewModelBase
    {
        protected bool canAdvanceToPreviousPage;
        protected bool canAdvanceToNextPage;

        protected bool safeRatingEnabled;
        protected bool questionableRatingEnabled;
        protected bool explicitRatingEnabled;

        protected readonly IProviderFilterControl providerFilterControl;

        public ReactiveCommand<Unit, Unit> NextPageCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; protected set; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; protected set; }
        public ReactiveCommand<string, Unit> SwitchRatingCommand { get; protected set; }
        public ReactiveCommand<string, Unit> SwitchProviderCommand { get; protected set; }

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


        public ViewModelBaseBrowserNavigationHandler(
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

            this.RefreshCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.providerFilterControl.Refresh();
            });

            this.ConfigureFilters();
        }

        ~ViewModelBaseBrowserNavigationHandler()
        {
            this.providerFilterControl.PageChanged -= OnPageChanged;
            this.providerFilterControl.FilterChanged -= OnFilterChanged;
        }

        protected virtual async Task SwitchRating(string rating)
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

        protected virtual async Task SwitchProvider(string provider)
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

        protected virtual void OnPageChanged(object? sender, int e)
        {
            this.CheckPageAdvancements();
        }

        protected virtual void OnFilterChanged(object? sender, EventArgs e)
        {
            this.CheckPageAdvancements();
            this.ConfigureFilters();
        }

        protected virtual async void ConfigureFilters()
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var currentFilter = await this.providerFilterControl.GetCurrentFilter();

                SafeRatingEnabled = currentFilter.Ratings.Contains(Rating.Safe.Name);
                QuestionableRatingEnabled = currentFilter.Ratings.Contains(Rating.Questionable.Name);
                ExplicitRatingEnabled = currentFilter.Ratings.Contains(Rating.Explicit.Name);
            });
        }

        protected virtual void CheckPageAdvancements()
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
