using System;
using Blazorise;

namespace Marechai.Shared
{
    public static class Validators
    {
        public static void ValidateStringWithMaxLength(ValidatorEventArgs e, string message, int maxLength)
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

        public static void ValidateNullableDoubleBiggerThanZero(ValidatorEventArgs e)
        {
            if(!(e.Value is double item) ||
               item < 0)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        public static void ValidateNullableIntegerBiggerThanZero(ValidatorEventArgs e)
        {
            if(!(e.Value is int item) ||
               item < 0)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }
    }
}