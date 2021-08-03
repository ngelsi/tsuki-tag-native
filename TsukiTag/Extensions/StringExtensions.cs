using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TsukiTag.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceProperties(this string str, object obj)
        {
            foreach (var property in obj.GetType().GetProperties().Where(t => t.PropertyType == typeof(string) || t.PropertyType == typeof(int) || t.PropertyType == typeof(bool)))
            {
                str = str.Replace($"#{property.Name?.ToLower()}#", property.GetValue(obj)?.ToString()?.ToLower(), StringComparison.OrdinalIgnoreCase);
            }

            str = str.Replace($"#guid#", Guid.NewGuid().ToString("N"));

            return str;
        }

        public static List<string> GetPropertyTemplateNameListType(this Type type)
        {
            var strs = new List<string>();
            var properties = type.GetProperties().Where(t => t.PropertyType == typeof(string) || t.PropertyType == typeof(int) || t.PropertyType == typeof(bool));

            foreach (var property in properties)
            {
                strs.Add(property.Name.ToLower());
            }

            strs.Add("guid");

            return strs;
        }

        public static List<string> GetPropertyTemplateNameList(this Type type)
        {
            return GetPropertyTemplateNameListType(type);
        }

        public static string GetPropertyTemplateNameIteration(this Type type)
        {
            return string.Join(", ", type.GetPropertyTemplateNameList());
        }

        public static bool WildcardMatches(this string str, string template)
        {
            var hasAsterisk = template.IndexOf('*') != -1;
            if (!hasAsterisk)
            {
                return str.Equals(template, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                if (template == "*")
                {
                    return true;
                }
                else if (template.StartsWith('*') && !template.EndsWith('*'))
                {
                    return str.EndsWith(template.Remove(0, 1), StringComparison.OrdinalIgnoreCase);
                }
                else if (template.EndsWith('*') && !template.StartsWith('*'))
                {
                    return str.StartsWith(template.Remove(template.Length - 1, 1), StringComparison.OrdinalIgnoreCase);
                }
                else if (template.Count(x => x == '*') > 1 && !template.StartsWith('/') && !template.EndsWith('/'))
                {
                    string regexPattern = string.Concat("^", Regex.Escape(template).Replace("\\*", ".*"), "$");
                    return Regex.IsMatch(str, regexPattern, RegexOptions.IgnoreCase);
                }
                else if (template.StartsWith('/') && template.EndsWith('/'))
                {
                    var regexPattern = template.Remove(0, 1).Remove(template.Length - 2, 1);
                    return Regex.IsMatch(str, regexPattern, RegexOptions.IgnoreCase);
                }

                //Invalid input at this point
                return false;
            }
        }
    }
}
