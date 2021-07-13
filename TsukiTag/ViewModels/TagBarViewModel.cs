using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.ViewModels
{
    public class TagBarViewModel : ViewModelBase
    {
        private string fullTagString;

        public TagBarViewModel()
        {
            
        }

        public string FullTagString
        {
            get { return fullTagString; }
            set
            {
                this.RaiseAndSetIfChanged(ref fullTagString, value);
            }
        }
    }
}
