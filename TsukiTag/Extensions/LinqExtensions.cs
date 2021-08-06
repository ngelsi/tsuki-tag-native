using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsukiTag.Models.Repository;

namespace TsukiTag.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static List<T> OrderByKeyword<T>(this List<T> list, string keyword) where T: PictureResourcePicture
        {
            if(keyword.Contains("score", StringComparison.OrdinalIgnoreCase))
            {
                return keyword.Contains("asc", StringComparison.OrdinalIgnoreCase) ? list.OrderBy(l => l.Picture?.Score).ToList() : list.OrderByDescending(l => l.Picture?.Score).ToList();
            }
            else if (keyword.Contains("added", StringComparison.OrdinalIgnoreCase))
            {
                return keyword.Contains("asc", StringComparison.OrdinalIgnoreCase) ? list.OrderBy(l => l.DateAdded).ToList() : list.OrderByDescending(l => l.DateAdded).ToList();
            }
            else if (keyword.Contains("modified", StringComparison.OrdinalIgnoreCase))
            {
                return keyword.Contains("asc", StringComparison.OrdinalIgnoreCase) ? list.OrderBy(l => l.DateModified).ToList() : list.OrderByDescending(l => l.DateModified).ToList();
            }
            else if (keyword.Contains("md5", StringComparison.OrdinalIgnoreCase))
            {
                return keyword.Contains("asc", StringComparison.OrdinalIgnoreCase) ? list.OrderBy(l => l.Picture?.Md5).ToList() : list.OrderByDescending(l => l.Picture?.Md5).ToList();
            }
            else if (keyword.Contains("id", StringComparison.OrdinalIgnoreCase))
            {
                return keyword.Contains("asc", StringComparison.OrdinalIgnoreCase) ? list.OrderBy(l => l.Picture?.Id).ToList() : list.OrderByDescending(l => l.Picture?.Id).ToList();
            }

            return list;
        }
    }
}
