using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models.Repository
{
    public class PreviousSession : INotifyPropertyChanged
    {
        private string[] tags;
        private string[] excludedTags;
        private int page;
        private Guid id;
        private DateTime added;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }
        public DateTime Added
        {
            get { return added; }
            set { added = value; }
        }

        public string[] Tags
        {
            get { return tags; }
            set
            {
                tags = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tags)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public string[] ExcludedTags
        {
            get { return excludedTags; }
            set
            {
                excludedTags = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExcludedTags)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public int Page
        {
            get { return page; }
            set
            {
                page = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Page)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public string Name
        {
            get
            {
                var str = Tags?.Any() == true || ExcludedTags?.Any() == true ?
                    $"({Page + 1}) {string.Join(", ", Tags)} {string.Join(", ", ExcludedTags?.Select(s => $"-{s}"))}" :
                    $"({Page + 1}) {Language.PreviousSessionsDefault}";

                return str.Length > 50 ? str.Substring(0, 46) + "..." : str;
            }
        }
    }
}
