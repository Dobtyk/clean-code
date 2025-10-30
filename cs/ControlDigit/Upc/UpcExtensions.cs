using System;
using System.Linq;

namespace ControlDigit
{
    public static class UpcExtensions
    {
        private const int MULTIPLIER_ODD_POSITIONS = 3;
        
        public static int CalculateUpc(this long number)
        {
            var resultSum = CalculateSum(number, CalculateDigitUpc);
            var result = 0;
            
            if (resultSum % 10 != 0)
            {
                result = 10 - resultSum % 10;
            }
            
            return result;
        }
        
        public static int CalculateSum(long number, Func<int, int, int> calculateDigit)
        {
            //var revertNumberString = RevertNumber(number);

            //return revertNumberString.Select(t => int.Parse(t.ToString())).Select((digit, i) => calculateDigit(digit, i + 1)).Sum();

            var result = 0;
            var position = 1;
            while (number > 0)
            {
                var digit = (int)(number % 10);
                result += calculateDigit(digit, position);
                number /= 10;
                position += 1;
            }

            return result;
        }
        
        private static string RevertNumber(long number)
        {
            return new string(number.ToString().Reverse().ToArray());
        }
        
        private static int CalculateDigitUpc(int digit, int position)
        {
            if (position % 2 == 1)
            {
                return digit * MULTIPLIER_ODD_POSITIONS;
            }

            return digit;
        }
    }
}
