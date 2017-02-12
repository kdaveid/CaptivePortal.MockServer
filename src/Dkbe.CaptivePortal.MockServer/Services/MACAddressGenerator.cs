using System;
using System.Text;

namespace Dkbe.CaptivePortal.MockServer.Services
{
    /// <summary>
    /// Quelle: http://stackoverflow.com/questions/10161291/generate-a-random-mac-address
    /// Quelle: https://social.msdn.microsoft.com/Forums/en-US/369abcdb-c201-43fd-a555-0afbe5409d78/generate-random-ip-addresss?forum=csharpgeneral
    /// </summary>
    public static class FakeDataGenerator
    {
        /// <summary>
        /// Generates random MAC address for fake data
        /// </summary>
        /// <returns></returns>
        public static string GenerateMACAddress()
        {
            var sBuilder = new StringBuilder();
            var r = new Random();
            int number;
            byte b;
            for (int i = 0; i < 6; i++)
            {
                number = r.Next(0, 255);
                b = Convert.ToByte(number);
                if (i == 0)
                {
                    b = setBit(b, 6); //--> set locally administered
                    b = unsetBit(b, 7); // --> set unicast 
                }
                sBuilder.Append(number.ToString("X2"));
            }
            return sBuilder.ToString().ToUpper();
        }

        private static byte setBit(byte b, int BitNumber)
        {
            if (BitNumber < 8 && BitNumber > -1)
            {
                return (byte)(b | (byte)(0x01 << BitNumber));
            }
            else
            {
                throw new InvalidOperationException(
                "Der Wert für BitNumber " + BitNumber.ToString() + " war nicht im zulässigen Bereich! (BitNumber = (min)0 - (max)7)");
            }
        }

        private static byte unsetBit(byte b, int BitNumber)
        {
            if (BitNumber < 8 && BitNumber > -1)
            {
                return (byte)(b | (byte)(0x00 << BitNumber));
            }
            else
            {
                throw new InvalidOperationException(
                "Der Wert für BitNumber " + BitNumber.ToString() + " war nicht im zulässigen Bereich! (BitNumber = (min)0 - (max)7)");
            }
        }

        private static Random _randomForIp = new Random();
        public static string GenerateRandomIp()
        {
            return string.Format("{0}.{1}.{2}.{3}", _randomForIp.Next(0, 255), _randomForIp.Next(0, 255), _randomForIp.Next(0, 255), _randomForIp.Next(0, 255));
        }

    }
}
