using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;
using TsukiTag.Views;

namespace TsukiTag.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ContentControl currentContent;

        public ContentControl CurrentContent
        {
            get { return currentContent; }
            set { this.RaiseAndSetIfChanged(ref currentContent, value); }
        }

        public MainWindowViewModel(
        )
        {
            CurrentContent = new OnlineProvider();
        }
    }
}
