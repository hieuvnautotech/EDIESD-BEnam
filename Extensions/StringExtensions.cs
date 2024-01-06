using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESD_EDI_BE.Extensions
{
    public static class StringExtensions
    {
        public static bool IsDecimal(this string value)
        {
            var result = !string.IsNullOrWhiteSpace(value) && decimal.TryParse(value, out _);
            return result;
        }

        public static bool IsDateTime(this string input)
        {
            return DateTime.TryParse(input, out _);
        }

        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }
    }
}