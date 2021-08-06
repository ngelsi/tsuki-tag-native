using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models.Repository
{
    public abstract class PictureResourcePicture
    {
        public Guid Id { get; set; }

        public Guid ResourceListId { get; set; }

        public DateTime? DateAdded { get; set; }

        public DateTime? DateModified { get; set; }

        public string Md5 { get; set; }

        public Picture Picture { get; set; }
    }
}
