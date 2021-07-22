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

        private OnlineProvider onlineProvider;
        private ContentControl currentContent;

        public ReactiveCommand<Unit, Unit> SwitchToSettingsCommand { get; }

        public ReactiveCommand<Unit, Unit> SwitchToOnlineBrowsingCommand { get; }

        public ContentControl CurrentContent
        {
            get { return currentContent; }
            set { this.RaiseAndSetIfChanged(ref currentContent, value); }
        }

        //Avalonia exception happens if we try to re-instate the entire online
        //content. So for now to circumvent this, we just store the content in memory.
        //At least the benefit of having the online session not interrupted by visiting
        //the settings screen is there.
        public OnlineProvider OnlineProvider
        {
            get
            {
                if(onlineProvider == null)
                {
                    onlineProvider = new OnlineProvider();
                }

                return onlineProvider;
            }
        }

        public MainWindowViewModel(
            INavigationControl navigationControl
        )
        {
            this.navigationControl = navigationControl;
            this.navigationControl.SwitchedToOnlineBrowsing += OnSwitchedToOnlineBrowsing;
            this.navigationControl.SwitchedToSettings += OnSwitchedToSettings;

            this.SwitchToSettingsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.navigationControl.SwitchToSettings();
            });

            this.SwitchToOnlineBrowsingCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await this.navigationControl.SwitchToOnlineBrowsing();
            });

            CurrentContent = OnlineProvider;
        }

        ~MainWindowViewModel()
        {
            this.navigationControl.SwitchedToOnlineBrowsing -= OnSwitchedToOnlineBrowsing;
            this.navigationControl.SwitchedToSettings -= OnSwitchedToSettings;
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
                CurrentContent = OnlineProvider;
            });
        }
    }
}
