using System;

namespace EasyDynamo.Tools.Validators
{
    public static class InputValidator
    {
        public static void ThrowIfNotPositive(long value, string errorMessage = null)
        {
            if (value <= 0)
            {
                throw new ArgumentException(
                    errorMessage ?? "Value should be a positive integer.");
            }
        }

        public static void ThrowIfNullOrWhitespace(string value, string errorMessage = null)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            throw new ArgumentNullException(errorMessage ?? "Value cannot be empty.");
        }

        public static void ThrowIfAnyNullOrWhitespace(params string[] values)
        {
            foreach (var value in values)
            {
                ThrowIfNullOrWhitespace(value);
            }
        }

        public static void ThrowIfNull<T>(T item, string errorMessage = null) where T : class
        {
            if (item != null)
            {
                return;
            }

            var fallbackErrorMessage = "Value cannot be null.";

            throw new ArgumentNullException(errorMessage ?? fallbackErrorMessage);
        }

        public static void ThrowIfAnyNull(params object[] items)
        {
            foreach (var item in items)
            {
                ThrowIfNull(item);
            }
        }
    }
}
