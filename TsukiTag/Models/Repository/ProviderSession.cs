using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models.Repository
{
    public class ProviderSession
    {
        public const string OnlineProviderSession = "f73d0d69-a674-4da0-9ea5-009c318a1587";
        public const string AllOnlineListsSession = "f73d0d69-a674-4da0-9ea5-009c318a1588";
        public const string AllWorkspacesSession = "f73d0d69-a674-4da0-9ea5-009c318a1589";

        public Guid Id { get; set; }

        public string Context { get; set; }

        public string[] Ratings { get; set; }

        public string[] Providers { get; set; }

        public int Limit { get;set;}
    }
}
