using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using TsukiTag.Models;

namespace TsukiTag.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public Language Language { get; set; }

        public ViewModelBase()
        {
            Language = new Language();
        }
    }
}
