﻿using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Views;

namespace TsukiTag.Models
{
    public class ProviderTabModel
    {
        private static ProviderTabModel pictureListTab;

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
                if (pictureListTab == null)
                {
                    pictureListTab = new ProviderTabModel() { Header = "Browse", Content = new OnlineBrowser(), Identifier = "ONLINE" };
                }

                return pictureListTab;
            }
        }

        public override bool Equals(object? obj)
        {
            return (obj as ProviderTabModel)?.Identifier == Identifier;
        }
    }
}
