using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;

namespace TsukiTag.Views
{
    public partial class TagBar : UserControl
    {
        public TagBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void TagLabelGotPress(object sender, PointerPressedEventArgs e)
        {
            var tag = (sender as TextBlock)?.Text?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(tag))
            {
                (this.DataContext as TsukiTag.ViewModels.TagBarViewModel)?.OnTagClicked(tag);
            }
        }

        private void TagBoxGotKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                var tag = (sender as AutoCompleteBox)?.Text?.Trim();
                if(!string.IsNullOrEmpty(tag))
                {
                    (this.DataContext as TsukiTag.ViewModels.TagBarViewModel)?.OnTagAdded(tag);
                }
            }
            else if (e.Key == Key.Tab)
            {
                var filter = (sender as AutoCompleteBox)?.Text?.Trim();
                if(!string.IsNullOrEmpty(filter))
                {
                    (this.DataContext as TsukiTag.ViewModels.TagBarViewModel)?.OnAutoCompleteInitiated(filter);
                }                

                e.Handled = true;
            }
        }
    }
}
