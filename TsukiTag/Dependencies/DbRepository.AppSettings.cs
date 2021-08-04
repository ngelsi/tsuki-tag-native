using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models.Repository;

namespace TsukiTag.Dependencies
{
    public interface IApplicationSettingsDb
    {
        event EventHandler ApplicationSettingsChanged;

        ApplicationSettings Get();

        void Set(ApplicationSettings settings);
    }

    public partial class DbRepository
    {
        private const string AppSettingsKey = "8b34b745-d35f-47b3-9d84-565718de9413";

        public IApplicationSettingsDb ApplicationSettings { get; protected set; }


        private class ApplicationSettingsDb : IApplicationSettingsDb
        {
            public event EventHandler ApplicationSettingsChanged;

            private ApplicationSettings settingsCache;
            private DbRepository parent;

            public ApplicationSettingsDb(DbRepository parent)
            {
                this.parent = parent;
            }

            public ApplicationSettings Get()
            {
                EnsureApplicationSettingsCache();
                return settingsCache;
            }

            public void Set(ApplicationSettings settings)
            {
                using(var db = new LiteDatabase(MetadataRepositoryPath))
                {
                    var coll = db.GetCollection<ApplicationSettings>();
                    var dbSettings = coll.FindById(AppSettingsKey);

                    dbSettings = settings;
                    dbSettings.Id = AppSettingsKey;

                    coll.Upsert(dbSettings);
                }

                EnsureApplicationSettingsCache(true);
                ApplicationSettingsChanged?.Invoke(this, EventArgs.Empty);
            }

            private void EnsureApplicationSettingsCache(bool reset = false)
            {
                if(reset || settingsCache == null)
                {
                    using(var db = new LiteDatabase(MetadataRepositoryPath))
                    {
                        var coll = db.GetCollection<ApplicationSettings>();
                        var settings = coll.FindById(AppSettingsKey);

                        if(settings == null)
                        {
                            settings = new ApplicationSettings();
                            settings.Id = AppSettingsKey;
                            settings.AllowDuplicateImages = true;
                        }

                        settingsCache = settings;
                    }
                }
            }
        }
    }
}
