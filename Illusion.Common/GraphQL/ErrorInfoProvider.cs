using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Illusion.Common.GraphQL
{
    /// <summary>
    /// Credit to GraphQL.NET
    /// </summary>
    public class ErrorInfoProvider
    {
        private static readonly ConcurrentDictionary<Type, string> ExceptionErrorCodes = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Generates an normalized error code for the specified exception by taking the type name, removing the "GraphQL" prefix, if any,
        /// removing the "Exception" suffix, if any, and then converting the result from PascalCase to UPPER_CASE.
        /// </summary>
        public static string GetErrorCode(Type exceptionType) => ExceptionErrorCodes.GetOrAdd(exceptionType, NormalizeErrorCode);

        /// <summary>
        /// Generates an normalized error code for the specified exception by taking the type name, removing the "GraphQL" prefix, if any,
        /// removing the "Exception" suffix, if any, and then converting the result from PascalCase to UPPER_CASE.
        /// </summary>
        public static string GetErrorCode<T>() where T : Exception => GetErrorCode(typeof(T));

        /// <summary>
        /// Generates an normalized error code for the specified exception by taking the type name, removing the "GraphQL" prefix, if any,
        /// removing the "Exception" suffix, if any, and then converting the result from PascalCase to UPPER_CASE.
        /// </summary>
        public static string GetErrorCode(Exception exception) => GetErrorCode(exception.GetType());

        private static string NormalizeErrorCode(Type exceptionType)
        {
            var code = exceptionType.Name;

            if (code.EndsWith(nameof(Exception), StringComparison.InvariantCulture))
            {
                code = code.Substring(0, code.Length - nameof(Exception).Length);
            }

            if (code.StartsWith("GraphQL", StringComparison.InvariantCulture))
            {
                code = code.Substring("GraphQL".Length);
            }

            return GetAllCapsRepresentation(code);
        }

        private static string GetAllCapsRepresentation(string str)
        {
            return Regex
                .Replace(NormalizeString(str), @"([A-Z])([A-Z][a-z])|([a-z0-9])([A-Z])", "$1$3_$2$4")
                .ToUpperInvariant();
        }

        private static string NormalizeString(string str)
        {
            str = str?.Trim();
            return string.IsNullOrWhiteSpace(str)
                ? string.Empty
                : NormalizeTypeName(str);
        }

        private static string NormalizeTypeName(string name)
        {
            var tickIndex = name.IndexOf('`');
            return tickIndex >= 0
                ? name.Substring(0, tickIndex)
                : name;
        }
    }
}
