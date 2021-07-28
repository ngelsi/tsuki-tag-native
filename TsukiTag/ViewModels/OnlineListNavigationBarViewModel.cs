using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;

namespace TsukiTag.ViewModels
{
    public class OnlineListNavigationBarViewModel : ViewModelBaseBrowserNavigationHandler
    {
        public OnlineListNavigationBarViewModel(
            IProviderFilterControl providerFilterControl
        ) : base(providerFilterControl)
        {
        }
    }
}
