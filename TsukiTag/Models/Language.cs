using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies;

namespace TsukiTag.Models
{
    public class Language
    {
        private ILocalizer localizer;

        public ILocalizer Localizer
        {
            get
            {
                if (localizer == null)
                {
                    localizer = Ioc.SimpleIoc.Localizer;
                }

                return localizer;
            }
        }

        public Language()
        { }

        public string Providers => Localizer.Get(nameof(Providers));
        public string AllTags => Localizer.Get(nameof(AllTags));
        public string Metadata => Localizer.Get(nameof(Metadata));
        public string Safe => Localizer.Get(nameof(Safe));
        public string Questionable => Localizer.Get(nameof(Questionable));
        public string Explicit => Localizer.Get(nameof(Explicit));
        public string NextPage => Localizer.Get(nameof(NextPage));
        public string PreviousPage => Localizer.Get(nameof(PreviousPage));
        public string Browse => Localizer.Get(nameof(Browse));
        public string PreviousPicture => Localizer.Get(nameof(PreviousPicture));
        public string NextPicture => Localizer.Get(nameof(NextPicture));
        public string OpenPicture => Localizer.Get(nameof(OpenPicture));
        public string DeselectPicture => Localizer.Get(nameof(DeselectPicture));
        public string SwitchToTag => Localizer.Get(nameof(SwitchToTag));
        public string SwitchToMetadata => Localizer.Get(nameof(SwitchToMetadata));
        public string OriginalView => Localizer.Get(nameof(OriginalView));
        public string ClosePicture => Localizer.Get(nameof(ClosePicture));
        public string AddThisTagSearch => Localizer.Get(nameof(AddThisTagSearch));
        public string RemoveThisTagSearch => Localizer.Get(nameof(RemoveThisTagSearch));
        public string SetThisTagSearch => Localizer.Get(nameof(SetThisTagSearch));
    }
}
