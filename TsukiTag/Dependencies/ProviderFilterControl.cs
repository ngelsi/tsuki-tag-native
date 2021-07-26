using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Dependencies.ProviderSpecific;
using TsukiTag.Models;
using TsukiTag.Models.ProviderSpecific;
using TsukiTag.Models.Repository;

namespace TsukiTag.Dependencies
{
    public interface IProviderFilterControl
    {
        event EventHandler<int> PageChanged;

        event EventHandler FilterChanged;

        Task<ProviderFilter> GetCurrentFilter();

        bool CanAdvanceNextPage();

        bool CanAdvancePreviousPage();

        Task NextPage();

        Task PreviousPage();

        Task ResetPage();

        Task AddTag(string tag);

        Task RemoveTag(string tag);

        Task SetTag(string tag);

        Task RemoveRating(string rating);

        Task AddRating(string rating);

        Task AddProvider(string provider);

        Task RemoveProvider(string provider);
    }

    public class ProviderFilterControl : IProviderFilterControl
    {
        private ProviderFilter currentFilter;

        private readonly IDbRepository dbRepository;

        public ProviderFilter CurrentFilter { get => currentFilter; }

        public event EventHandler<int> PageChanged;

        public event EventHandler FilterChanged;

        public ProviderFilterControl(
            IDbRepository dbRepository
        )
        {
            this.dbRepository = dbRepository;
            this.InitializeFilter();
        }

        public bool CanAdvanceNextPage()
        {
            return true;
        }

        public bool CanAdvancePreviousPage()
        {
            return currentFilter.Page > 0;
        }

        public async Task NextPage()
        {
            await Task.Run(() =>
            {
                currentFilter.Page += 1;
                PageChanged?.Invoke(this, CurrentFilter.Page);
            });
        }

        public async Task PreviousPage()
        {
            await Task.Run(() =>
            {
                currentFilter.Page -= 1;
                PageChanged?.Invoke(this, CurrentFilter.Page);
            });
        }

        public async Task ResetPage()
        {
            await Task.Run(() =>
            {
                currentFilter.Page = 0;
                PageChanged?.Invoke(this, CurrentFilter.Page);
            });
        }

        public async Task AddTag(string tag)
        {
            await Task.Run(() =>
            {
                if (!currentFilter.Tags.Contains(tag))
                {
                    currentFilter.Page = 0;
                    currentFilter.Tags.Add(tag);

                    FilterChanged?.Invoke(this, EventArgs.Empty);
                }
            });
        }

        public async Task RemoveTag(string tag)
        {
            await Task.Run(() =>
            {
                currentFilter.Page = 0;
                currentFilter.Tags.Remove(tag);

                FilterChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task SetTag(string tag)
        {
            await Task.Run(() =>
            {
                currentFilter.Page = 0;
                currentFilter.Tags.Clear();
                currentFilter.Tags.Add(tag);

                FilterChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task AddRating(string rating)
        {
            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(rating) && !currentFilter.Ratings.Contains(rating))
                {
                    currentFilter.Ratings.Add(rating);
                    ApplyBroadcastProviderChanges();
                }
            });
        }

        public async Task RemoveRating(string rating)
        {
            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(rating) && currentFilter.Ratings.Contains(rating))
                {
                    currentFilter.Ratings.Remove(rating);
                    ApplyBroadcastProviderChanges();
                }
            });
        }

        public async Task AddProvider(string provider)
        {
            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(provider) && !currentFilter.Providers.Contains(provider))
                {
                    currentFilter.Providers.Add(provider);
                    ApplyBroadcastProviderChanges();
                }
            });
        }

        public async Task RemoveProvider(string provider)
        {
            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(provider) && currentFilter.Providers.Contains(provider))
                {
                    currentFilter.Providers.Remove(provider);
                    ApplyBroadcastProviderChanges();
                }
            });
        }

        public async Task<ProviderFilter> GetCurrentFilter()
        {
            return await Task.FromResult(CurrentFilter);
        }

        private async void ApplyBroadcastProviderChanges()
        {
            await Task.Run(() =>
            {
                var session = dbRepository.ProviderSession.Get(ProviderSession.OnlineProviderSession);
                if (session != null)
                {
                    session.Providers = currentFilter.Providers.ToArray();
                    session.Ratings = currentFilter.Ratings.ToArray();

                    dbRepository.ProviderSession.AddOrUpdate(session);
                }

                FilterChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        private void InitializeFilter()
        {
            var session = dbRepository.ProviderSession.Get(ProviderSession.OnlineProviderSession);
            currentFilter = new ProviderFilter();
            currentFilter.Providers.AddRange(session.Providers);
            currentFilter.Ratings.AddRange(session.Ratings);
        }
    }
}
