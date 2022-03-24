using System;
using System.Collections.Generic;
using System.Diagnostics;
using Duende.IdentityServer.Extensions;

namespace Bff.InMemoryTests
{
    internal static class StringExtensions
    {
        [DebuggerStepThrough]
        public static bool IsMissing(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        [DebuggerStepThrough]
        public static bool IsPresent(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
        public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

        public static string RemovePreFix(this string str, params string[] preFixes) => str.RemovePreFix(StringComparison.Ordinal, preFixes);

        public static string RemovePreFix(
            this string aString,
            StringComparison cultureCaseSortComparison,
            params string[] preFixes)
        {
            if (aString.IsNullOrEmpty())
                return (string)null;
            if (((ICollection<string>)preFixes).IsNullOrEmpty<string>())
                return aString;
            foreach (string preFix in preFixes)
            {
                if (aString.StartsWith(preFix, cultureCaseSortComparison))
                    return aString.Right(aString.Length - preFix.Length);
            }
            return aString;
        }

        public static string Right(this string aString, int len)
        {
            Check.NotNull<string>(aString, nameof(aString));
            if (aString.Length < len)
                throw new ArgumentException("len argument mustn't exceed a given string's length.");
            return aString.Substring(aString.Length - len, len);
        }

        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        [DebuggerStepThrough]
        public static class Check
        {
            public static T NotNull<T>(T value, string parameterName) => (object)value != null ? value : throw new ArgumentNullException(parameterName);
        }
    }
}