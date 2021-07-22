using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsukiTag.Models
{
    public class ToastMessage
    {
        public string Id { get; set; }

        public string Text { get; set; }

        public bool IsCloseable { get; set; }

        public ToastMessage()
        {
            Id = Guid.NewGuid().ToString();
        }

        public static ToastMessage Closeable(string text, string? id = null)
        {
            return new ToastMessage() { Id = id ?? Guid.NewGuid().ToString(), IsCloseable = true, Text = text };
        }

        public static ToastMessage Uncloseable(string text, string? id = null)
        {
            return new ToastMessage() { Id = id ?? Guid.NewGuid().ToString(), IsCloseable = true, Text = text };
        }
    }
}
