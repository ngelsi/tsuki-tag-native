using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Extensions;

namespace TsukiTag.Models.Repository
{
    public class MetadataGroup : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Guid id;
        private string name;
        private string title;
        private string description;
        private string author;
        private string copyright;
        private string notes;
        private bool isDefault;

        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
        }

        public string Title
        {
            get { return title; }
            set { title = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title))); }
        }

        public string Description
        {
            get { return description; }
            set { description = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description))); }
        }

        public string Author
        {
            get { return author; }
            set { author = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Author))); }
        }

        public string Copyright
        {
            get { return copyright; }
            set { copyright = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Copyright))); }
        }

        public string Notes
        {
            get { return notes; }
            set { notes = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes))); }
        }

        public bool IsDefault
        {
            get { return isDefault; }
            set { isDefault = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDefault))); }
        }

        public Picture ProcessPicture(Picture picture)
        {
            picture.Author = Author != null ? Author?.ReplaceProperties(picture) : picture.Author;
            picture.Title = Title?.ReplaceProperties(picture);
            picture.Description = Description?.ReplaceProperties(picture);
            picture.Copyright = Copyright?.ReplaceProperties(picture);
            picture.Notes = Notes?.ReplaceProperties(picture);

            return picture;
        }
    }
}
