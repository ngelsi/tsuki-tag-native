using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;
using TsukiTag.Views;

namespace TsukiTag.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly INavigationControl navigationControl;

        private ProviderContext providerContext;
        private ContentControl currentContent;

        public ReactiveCommand<Unit, Unit> SwitchToSettingsCommand { get; }

        public ReactiveCommand<Unit, Unit> SwitchToOnlineBrowsingCommand { get; }

        public ReactiveCommand<Unit, Unit> SwitchToAllOnlineListBrowsingCommand { get; }

        public ContentControl CurrentContent
        {
            get { return currentContent; }
            set { this.RaiseAndSetIfChanged(ref currentContent, value); }
        }

        //Avalonia exception happens if we try to re-instate the entire provider
        //content. So for now to circumvent this, we just store the content in memory.
        //At least the benefit of having the browsing session not interrupted by visiting
        //the settings screen is there.
        public ProviderContext ProviderContext
        {
            get
            {
                if(providerContext == null)
                {
                    providerContext = new ProviderContext();
                }

                return providerContext;
            }
        }

        public MainWindowViewModel(
            INavigationControl navigationControl
        )
        {
            this.navigationControl = navigationControl;
            this.navigationControl.SwitchedToOnlineBrowsing += OnSwitchedToOnlineBrowsing;
            this.navigationControl.SwitchedToSettings += OnSwitchedToSettings;
            this.navigationControl.SwitchedToAllOnlineListBrowsing += OnSwitchedToAllOnlineListBrowsing;

            this.SwitchToSettingsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.navigationControl.SwitchToSettings();
            });

            this.SwitchToOnlineBrowsingCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.navigationControl.SwitchToOnlineBrowsing();
            });

            this.SwitchToAllOnlineListBrowsingCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.navigationControl.SwitchToAllOnlineListBrowsing();
            });

            CurrentContent = ProviderContext;
        }

        ~MainWindowViewModel()
        {
            this.navigationControl.SwitchedToOnlineBrowsing -= OnSwitchedToOnlineBrowsing;
            this.navigationControl.SwitchedToSettings -= OnSwitchedToSettings;
            this.navigationControl.SwitchedToAllOnlineListBrowsing -= OnSwitchedToAllOnlineListBrowsing;
        }

        private void OnSwitchedToSettings(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                CurrentContent = new Settings();
            });
        }

        private void OnSwitchedToOnlineBrowsing(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                CurrentContent = ProviderContext;
            });
        }

        private void OnSwitchedToAllOnlineListBrowsing(object? sender, EventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                CurrentContent = ProviderContext;
            });
        }
    }
}

