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
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(errorMessage ?? "Value cannot be empty.");
            }
        }

        public static void ThrowIfAnyNullOrWhitespace(params string[] values)
        {
            foreach (var value in values)
            {
                ThrowIfNullOrWhitespace(value);
            }
        }
    }
}
