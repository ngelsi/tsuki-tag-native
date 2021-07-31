using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models.Repository;
using TsukiTag.Views;

namespace TsukiTag.Models
{
    public class ProviderTabModel
    {
        private static ProviderTabModel onlineBrowserTab;
        private static ProviderTabModel allOnlineListsTab;
        private static ProviderTabModel allWorkspacesTab;

        public string Header { get; set; }

        public string Identifier { get; set; }

        public object Context { get; set; }

        public ContentControl Content { get; set; }

        public ProviderTabModel()
        {

        }

        public static ProviderTabModel OnlineBrowserTab
        {
            get
            {
                if (onlineBrowserTab == null)
                {
                    onlineBrowserTab = new ProviderTabModel() { Header = Ioc.SimpleIoc.Localizer.Get("Browse"), Content = new OnlineBrowser(), Identifier = "ONLINE", Context = ProviderSession.OnlineProviderSession };
                }

                return onlineBrowserTab;
            }
        }

        public static ProviderTabModel AllWorkspacesTab
        {
            get
            {
                if (allWorkspacesTab == null)
                {
                    allWorkspacesTab = new ProviderTabModel() { Header = Ioc.SimpleIoc.Localizer.Get("Browse"), Content = new WorkspaceBrowser(), Identifier = "ALLWORKSPACES", Context = ProviderSession.AllWorkspacesSession };
                }

                return allWorkspacesTab;
            }
        }

        public static ProviderTabModel AllOnlineListsTab
        {
            get
            {
                if (allOnlineListsTab == null)
                {
                    allOnlineListsTab = new ProviderTabModel() { Header = Ioc.SimpleIoc.Localizer.Get("Browse"), Content = new OnlineListBrowser(), Identifier = "ALLONLINELISTS", Context = ProviderSession.AllOnlineListsSession };
                }

                return allOnlineListsTab;
            }
        }

        public override bool Equals(object? obj)
        {
            return (obj as ProviderTabModel)?.Identifier == Identifier;
        }
    }
}
