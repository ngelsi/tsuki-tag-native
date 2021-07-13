using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models;

namespace TsukiTag.Dependencies
{
    public interface IProviderFilterControl
    {
        event EventHandler<int> PageChanged;

        event EventHandler FilterChanged;

        ProviderFilter CurrentFilter { get; }

        bool CanAdvanceNextPage();

        bool CanAdvancePreviousPage();

        Task NextPage();

        Task PreviousPage();

        Task ResetPage();

        Task AddTag(string tag);

        Task RemoveTag(string tag);

        Task SetTag(string tag);
    }

    public class ProviderFilterControl : IProviderFilterControl
    {
        private ProviderFilter currentFilter;

        public ProviderFilter CurrentFilter { get => currentFilter; }

        public event EventHandler<int> PageChanged;

        public event EventHandler FilterChanged;

        public ProviderFilterControl()
        {
            currentFilter = new ProviderFilter();
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
                if(!currentFilter.Tags.Contains(tag))
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
    }
}
