using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;
using TsukiTag.Models;

namespace TsukiTag.ViewModels
{
    public class PictureMetadataEditorViewModel : ViewModelBase
    {
        private readonly IPictureControl pictureControl;
        private Picture picture;
        private string filterString;

        public string FilterString
        {
            get { return filterString; }
            set
            {
                filterString = value;
                this.RaisePropertyChanged(nameof(FilterString));
                this.RaisePropertyChanged(nameof(FilteredTags));
                this.RaisePropertyChanged(nameof(TagCount));
            }
        }

        public Picture Picture
        {
            get { return picture; }
            set
            {
                picture = value;
                this.RaisePropertyChanged(nameof(Picture));
            }
        }

        public int TagCount => FilteredTags.Count;

        public List<string> FilteredTags
        {
            get
            {
                if (!string.IsNullOrEmpty(FilterString))
                {
                    var filterParts = FilterString.Split(' ').Where(s => !string.IsNullOrEmpty(s));
                    return Picture?.TagList.Where(s => filterParts.Any(fs => s.IndexOf(fs) > -1)).ToList() ?? new List<string>();
                }

                return Picture?.TagList ?? new List<string>();
            }
        }

        public PictureMetadataEditorViewModel(
            Picture picture,
            IPictureControl pictureControl
        )
        {
            this.picture = picture;
            this.pictureControl = pictureControl;
        }

        ~PictureMetadataEditorViewModel()
        {

        }
    }
}
