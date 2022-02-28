using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace IdentityModel.AspNetCore
{
    internal static class StringExtensions
    {
        [DebuggerStepThrough]
        public static bool IsMissing([NotNullWhen(false)]this string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
        
        [DebuggerStepThrough]
        public static bool IsPresent([NotNullWhen(true)]this string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

    }
}