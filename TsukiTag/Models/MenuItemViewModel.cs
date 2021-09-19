using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TsukiTag.Models
{
    public class MenuItemViewModel
    {
        public string Header { get; set; }

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        public IList<MenuItemViewModel> Items { get; set; }

        public bool IsEnabled { get; set; }

        public string Icon { get; set; }

        public bool HasIcon { get; set; }

        public MenuItemViewModel()
        {
            IsEnabled = true;
        }
    }
}
