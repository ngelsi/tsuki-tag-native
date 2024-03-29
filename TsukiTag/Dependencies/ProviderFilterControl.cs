﻿using System;
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

        Task Refresh();

        Task NextPage();

        Task PreviousPage();

        Task ResetPage();

        Task AddTag(string tag);

        Task RemoveTag(string tag);

        Task AddExcludeTag(string tag);

        Task RemoveExcludeTag(string tag);

        Task SetTag(string tag);

        Task SetFilter(string[]? tags, string[]? excludedTags, int? page);

        Task RemoveRating(string rating);

        Task AddRating(string rating);

        Task AddProvider(string provider);

        Task RemoveProvider(string provider);

        Task ReinitializeFilter(string providerSessionId);
    }

    public class ProviderFilterControl : IProviderFilterControl
    {
        private ProviderFilter currentFilter;
        private string currentSession;

        private readonly IDbRepository dbRepository;

        public ProviderFilter CurrentFilter { get => currentFilter; }

        public event EventHandler<int> PageChanged;

        public event EventHandler FilterChanged;

        public ProviderFilterControl(
            IDbRepository dbRepository
        )
        {
            this.dbRepository = dbRepository;
        }

        public bool CanAdvanceNextPage()
        {
            return true;
        }

        public bool CanAdvancePreviousPage()
        {
            return currentFilter.Page > 0;
        }

        public async Task Refresh()
        {
            await Task.Run(() =>
            {
                FilterChanged?.Invoke(this, EventArgs.Empty);
            });
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
                if (tag.Contains(":"))
                {
                    if (tag.StartsWith("page", StringComparison.OrdinalIgnoreCase) && int.TryParse(tag.Split(':')[1], out int pageNumber))
                    {
                        currentFilter.Page = Math.Max(0, pageNumber - 1);
                        FilterChanged?.Invoke(this, EventArgs.Empty);

                        return;
                    }
                }

                if (currentFilter.ExcludedTags.Contains(tag))
                {
                    currentFilter.Page = 0;
                    currentFilter.ExcludedTags.Remove(tag);

                    FilterChanged?.Invoke(this, EventArgs.Empty);
                }
                else if (!currentFilter.Tags.Contains(tag))
                {
                    currentFilter.Page = 0;
                    currentFilter.Tags.Add(tag);

                    FilterChanged?.Invoke(this, EventArgs.Empty);
                }
            });
        }

        public async Task AddExcludeTag(string tag)
        {
            await Task.Run(() =>
            {
                if (currentFilter.Tags.Contains(tag))
                {
                    currentFilter.Page = 0;
                    currentFilter.Tags.Remove(tag);

                    FilterChanged?.Invoke(this, EventArgs.Empty);
                }
                else if (!currentFilter.ExcludedTags.Contains(tag))
                {
                    currentFilter.Page = 0;
                    currentFilter.ExcludedTags.Add(tag);

                    FilterChanged?.Invoke(this, EventArgs.Empty);
                }
            });
        }

        public async Task RemoveTag(string tag)
        {
            await Task.Run(() =>
            {
                if (currentFilter.Tags.Contains(tag))
                {
                    currentFilter.Page = 0;
                    currentFilter.Tags.Remove(tag);

                    FilterChanged?.Invoke(this, EventArgs.Empty);
                }
                else if (!currentFilter.ExcludedTags.Contains(tag))
                {
                    currentFilter.Page = 0;
                    currentFilter.ExcludedTags.Add(tag);

                    FilterChanged?.Invoke(this, EventArgs.Empty);
                }
            });
        }

        public async Task RemoveExcludeTag(string tag)
        {
            await Task.Run(() =>
            {
                if (currentFilter.ExcludedTags.Contains(tag))
                {
                    currentFilter.Page = 0;
                    currentFilter.ExcludedTags.Remove(tag);

                    FilterChanged?.Invoke(this, EventArgs.Empty);                    
                }
            });
        }

        public async Task SetTag(string tag)
        {
            await Task.Run(() =>
            {
                currentFilter.Page = 0;
                currentFilter.Tags.Clear();
                currentFilter.Tags.Add(tag);
                currentFilter.ExcludedTags.Clear();

                FilterChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task SetFilter(string[]? tags, string[]? excludedTags, int? page)
        {
            await Task.Run(() => {

                currentFilter.Page = page ?? 0;
                
                currentFilter.Tags.Clear();                
                currentFilter.ExcludedTags.Clear();

                if(tags != null && tags.Length > 0)
                {
                    currentFilter.Tags = new List<string>(tags);
                }

                if(excludedTags != null && excludedTags.Length > 0)
                {
                    currentFilter.ExcludedTags = new List<string>(excludedTags);
                }

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

        public async Task ReinitializeFilter(string providerSessionId)
        {
            this.currentSession = providerSessionId;
            var appSettings = dbRepository.ApplicationSettings.Get();

            var session = dbRepository.ProviderSession.Get(providerSessionId);
            if (session != null)
            {
                var filter = new ProviderFilter();

                filter.Session = providerSessionId;
                filter.Providers.AddRange(session.Providers);
                filter.Ratings.AddRange(session.Ratings);

                if (session.Limit > 0)
                {
                    filter.Limit = session.Limit;
                }

                if (!appSettings.RemoveTagsOnContextSwitch && currentFilter != null)
                {
                    filter.Tags = new List<string>(currentFilter.Tags);
                    filter.ExcludedTags = new List<string>(currentFilter.ExcludedTags);
                }

                currentFilter = filter;
                FilterChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private async void ApplyBroadcastProviderChanges()
        {
            await Task.Run(() =>
            {
                var session = dbRepository.ProviderSession.Get(currentSession);
                if (session != null)
                {
                    session.Providers = currentFilter.Providers.ToArray();
                    session.Ratings = currentFilter.Ratings.ToArray();

                    dbRepository.ProviderSession.AddOrUpdate(session);
                }

                FilterChanged?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}
