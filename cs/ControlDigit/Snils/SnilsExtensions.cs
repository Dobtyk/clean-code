using System;

namespace ControlDigit
{
    public static class SnilsExtensions
    {
        public static int CalculateSnils(this long number)
        {
            var resultSum = UpcExtensions.CalculateSum(number, CalculateDigitSnils);
            var result = 0;

            while (resultSum > 101)
            {
                resultSum %= 101;
            }
            
            if (resultSum < 100)
            {
                result = resultSum;
            }
            
            return result;
        }
        
        private static int CalculateDigitSnils(int digit, int position)
        {
            return digit * position;
        }
    }
}
