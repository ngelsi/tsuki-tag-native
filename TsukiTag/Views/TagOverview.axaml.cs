using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System.Reactive.Concurrency;
using TsukiTag.Models;

namespace TsukiTag.Views
{
    public partial class TagOverview : UserControl
    {
        public TagOverview()
        {            
            InitializeComponent();                        
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void FilterBoxGotFocus(object sender, GotFocusEventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                (sender as TextBox)?.SelectAll();
            });
        }

        private void FilterBoxGotPress(object sender, PointerPressedEventArgs e)
        {
            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                (sender as TextBox)?.SelectAll();
            });
        }

        private void TagLabelGotPress(object sender, PointerPressedEventArgs e)
        {
            var tag = (sender as Label)?.Content?.ToString();
            if(!string.IsNullOrEmpty(tag))
            {
                (this.DataContext as TsukiTag.ViewModels.TagOverviewViewModel)?.OnTagClicked(tag);
            }
        }

        private void TagPlusGotPress(object sender, PointerPressedEventArgs e)
        {
            var tag = ((sender as Label)?.DataContext as TagCollectionElement)?.Tag;
            if (!string.IsNullOrEmpty(tag))
            {
                (this.DataContext as TsukiTag.ViewModels.TagOverviewViewModel)?.OnTagAdded(tag);
            }
        }

        private void TagMinusGotPress(object sender, PointerPressedEventArgs e)
        {
            var tag = ((sender as Label)?.DataContext as TagCollectionElement)?.Tag;
            if (!string.IsNullOrEmpty(tag))
            {
                (this.DataContext as TsukiTag.ViewModels.TagOverviewViewModel)?.OnTagRemoved(tag);
            }
        }
    }
}
