using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Dependencies
{
    public interface INavigationControl
    {
        event EventHandler SwitchedToTagOverview;

        event EventHandler SwitchedToMetadataOverview;

        event EventHandler TemporaryMetadataOverviewStart;

        event EventHandler TemporaryMetadataOverviewEnd;

        event EventHandler SwitchedToSettings;

        event EventHandler SwitchedToOnlineBrowsing;

        event EventHandler SwitchedToAllOnlineListBrowsing;

        event EventHandler<Guid> SwitchedToSpecificOnlineListBrowsing;

        Task SwitchToTagOverview();

        Task SwitchToMetadataOverview();

        Task EnforceMetadataOverview();

        Task StopEnforceMetadataOverview();

        Task SwitchToSettings();

        Task SwitchToOnlineBrowsing();

        Task SwitchToAllOnlineListBrowsing();

        Task SwitchToSpecificOnlineListBrowsing(Guid id);
    }

    public class NavigationControl : INavigationControl
    {
        public event EventHandler SwitchedToTagOverview;
        public event EventHandler SwitchedToMetadataOverview;
        public event EventHandler TemporaryMetadataOverviewStart;
        public event EventHandler TemporaryMetadataOverviewEnd;
        public event EventHandler SwitchedToSettings;
        public event EventHandler SwitchedToOnlineBrowsing;
        public event EventHandler SwitchedToAllOnlineListBrowsing;
        public event EventHandler<Guid> SwitchedToSpecificOnlineListBrowsing;

        public NavigationControl()
        {

        }

        public async Task SwitchToTagOverview()
        {
            await Task.Run(() =>
            {
                SwitchedToTagOverview?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task SwitchToMetadataOverview()
        {
            await Task.Run(() =>
            {
                SwitchedToMetadataOverview?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task EnforceMetadataOverview()
        {
            await Task.Run(() =>
            {
                TemporaryMetadataOverviewStart?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task StopEnforceMetadataOverview()
        {
            await Task.Run(() =>
            {
                TemporaryMetadataOverviewEnd?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task SwitchToSettings()
        {
            await Task.Run(() =>
            {
                SwitchedToSettings?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task SwitchToOnlineBrowsing()
        {
            await Task.Run(() =>
            {
                SwitchedToOnlineBrowsing?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task SwitchToAllOnlineListBrowsing()
        {
            await Task.Run(() =>
            {
                SwitchedToAllOnlineListBrowsing?.Invoke(this, EventArgs.Empty);
            });
        }

        public async Task SwitchToSpecificOnlineListBrowsing(Guid id)
        {
            await Task.Run(() =>
            {
                SwitchedToSpecificOnlineListBrowsing?.Invoke(this, id);
            });
        }
    }
}
