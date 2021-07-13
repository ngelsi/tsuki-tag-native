using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;

namespace TsukiTag.ViewModels
{
    public class OnlineNavigationBarViewModel : ViewModelBase
    {
        private bool canAdvanceToPreviousPage;
        private bool canAdvanceToNextPage;

        private readonly IProviderFilterControl providerFilterControl;

        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }

        public bool CanAdvanceToPreviousPage
        {
            get { return canAdvanceToPreviousPage; }
            set { 
                    this.canAdvanceToPreviousPage = value;
                    this.RaisePropertyChanged(nameof(CanAdvanceToPreviousPage));
                }
        }

        public bool CanAdvanceToNextPage
        {
            get { return canAdvanceToNextPage; }
            set { 
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
        }

        ~OnlineNavigationBarViewModel()
        {
            this.providerFilterControl.PageChanged -= OnPageChanged;
            this.providerFilterControl.FilterChanged -= OnFilterChanged;
        }

        private void OnPageChanged(object? sender, int e)
        {
            this.CheckPageAdvancements();
        }

        private void OnFilterChanged(object? sender, EventArgs e)
        {
            this.CheckPageAdvancements();
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
