using System;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace G24_BWallet_Backend.Repository
{
    public class Format
    {
        public string MoneyFormat(double money)
        {
            string reverse = Reverse(money.ToString());
            var sb = new StringBuilder();
            var length = reverse.Length;
            sb.Append(reverse);
            if (reverse.Length >= 4)
            {
                for (int i = 3; ; i += 4)
                {
                    if (i >= sb.Length)
                        break;
                    sb.Insert(i, ".");
                }
            }
            return Reverse(sb.ToString()) + " VND";
        }

        public string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
