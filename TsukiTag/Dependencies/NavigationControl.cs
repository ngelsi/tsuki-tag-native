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

        Task SwitchToTagOverview();

        Task SwitchToMetadataOverview();

        Task EnforceMetadataOverview();

        Task StopEnforceMetadataOverview();
    }

    public class NavigationControl : INavigationControl
    {
        public event EventHandler SwitchedToTagOverview;
        public event EventHandler SwitchedToMetadataOverview;
        public event EventHandler TemporaryMetadataOverviewStart;
        public event EventHandler TemporaryMetadataOverviewEnd;

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
    }
}
