using System;
using Blazorise;

namespace Marechai.Shared
{
    public static class Validators
    {
        public static void ValidateString(ValidatorEventArgs e, string message, int maxLength)
        {
            string item = e.Value as string;

            if(item?.Length > maxLength)
            {
                e.ErrorText = message;
                e.Status    = ValidationStatus.Error;
            }
            else
                e.Status = string.IsNullOrWhiteSpace(item) ? ValidationStatus.Error : ValidationStatus.Success;
        }

        public static void ValidateIntroducedDate(ValidatorEventArgs e)
        {
            if(!(e.Value is DateTime item) ||
               item.Year < 1900            ||
               item.Date >= DateTime.UtcNow.Date)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateDouble(ValidatorEventArgs e, double minValue = 0, double maxValue = double.MaxValue)
        {
            if(!(e.Value is double item) ||
               item < minValue || item > maxValue)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateInteger(ValidatorEventArgs e, int minValue = 0, int maxValue = int.MaxValue)
        {
            if(!(e.Value is int item) ||
               item < minValue        || item > maxValue)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateLong(ValidatorEventArgs e, long minValue = 0, long maxValue = long.MaxValue)
        {
            if(!(e.Value is long item) ||
               item < minValue        || item > maxValue)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateFloat(ValidatorEventArgs e, float minValue = 0, float maxValue = float.MaxValue)
        {
            if(!(e.Value is float item) ||
               item < minValue           || item > maxValue)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }
    }
}