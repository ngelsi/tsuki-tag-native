using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using ReactiveUI;
using System.Reactive.Concurrency;
using TsukiTag.Models.Repository;
using TsukiTag.ViewModels;

namespace TsukiTag.Views
{
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();

            this.Initialized += OnInitialized;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnInitialized(object? sender, System.EventArgs e)
        {
            if (this.DataContext is SettingsViewModel vm)
            {
                vm.Initialize();
            }
        }

        private void OnCurrentTagToAddKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                var textbox = (sender as TextBox);
                var id = (textbox?.DataContext as OnlineList)?.Id;

                if(id != null)
                {
                    (this.DataContext as SettingsViewModel)?.OnAddTagstoAdd(id.Value);
                }
            }
        }

        private void OnCurrentTagToRemoveKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textbox = (sender as TextBox);
                var id = (textbox?.DataContext as OnlineList)?.Id;

                if (id != null)
                {
                    (this.DataContext as SettingsViewModel)?.OnAddTagstoRemove(id.Value);
                }
            }
        }

        private void OnOptionalConditionTagKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textbox = (sender as TextBox);
                var id = (textbox?.DataContext as OnlineList)?.Id;

                if (id != null)
                {
                    (this.DataContext as SettingsViewModel)?.OnAddOptionalConditionTag(id.Value);
                }
            }
        }

        private void OnMandatoryConditionTagKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textbox = (sender as TextBox);
                var id = (textbox?.DataContext as OnlineList)?.Id;

                if (id != null)
                {
                    (this.DataContext as SettingsViewModel)?.OnAddMandatoryConditionTag(id.Value);
                }
            }
        }

        private void OnRemoveTagstoAddPressed(object sender, PointerPressedEventArgs e)
        {
            var icon = (sender as Projektanker.Icons.Avalonia.Icon);
            var button = (icon?.Parent as Button);
            var tag = (button?.DataContext as string);

            var itemscontrol = button.FindAncestorOfType<ItemsControl>();
            var id = (itemscontrol?.DataContext as OnlineList)?.Id;

            if (tag != null && id != null)
            {
                RxApp.MainThreadScheduler.Schedule(async () =>
                {
                    (this.DataContext as SettingsViewModel)?.OnRemoveTagsToAdd(id.Value, tag);
                });
            }
        }

        private void OnRemoveTagstoRemovePressed(object sender, PointerPressedEventArgs e)
        {
            var icon = (sender as Projektanker.Icons.Avalonia.Icon);
            var button = (icon?.Parent as Button);
            var tag = (button?.DataContext as string);

            var itemscontrol = button.FindAncestorOfType<ItemsControl>();
            var id = (itemscontrol?.DataContext as OnlineList)?.Id;

            if (tag != null && id != null)
            {
                RxApp.MainThreadScheduler.Schedule(async () =>
                {
                    (this.DataContext as SettingsViewModel)?.OnRemoveTagsToRemove(id.Value, tag);
                });
            }
        }

        private void OnRemoveOptionalConditionTagPressed(object sender, PointerPressedEventArgs e)
        {
            var icon = (sender as Projektanker.Icons.Avalonia.Icon);
            var button = (icon?.Parent as Button);
            var tag = (button?.DataContext as string);

            var itemscontrol = button.FindAncestorOfType<ItemsControl>();
            var id = (itemscontrol?.DataContext as OnlineList)?.Id;

            if (tag != null && id != null)
            {
                RxApp.MainThreadScheduler.Schedule(async () =>
                {
                    (this.DataContext as SettingsViewModel)?.OnRemoveOptionalConditionTag(id.Value, tag);
                });
            }
        }

        private void OnRemoveMandatoryConditionTagPressed(object sender, PointerPressedEventArgs e)
        {
            var icon = (sender as Projektanker.Icons.Avalonia.Icon);
            var button = (icon?.Parent as Button);
            var tag = (button?.DataContext as string);

            var itemscontrol = button.FindAncestorOfType<ItemsControl>();
            var id = (itemscontrol?.DataContext as OnlineList)?.Id;

            if (tag != null && id != null)
            {
                RxApp.MainThreadScheduler.Schedule(async () =>
                {
                    (this.DataContext as SettingsViewModel)?.OnRemoveMandatoryConditionTag(id.Value, tag);
                });
            }
        }

    }
}
