using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace G24_BWallet_Backend.Repository
{
    public class Format
    {
        //private readonly IConfiguration _configuration;

        //public Format(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //}
        //public string MoneyFormat(double money)
        //{
        //    string reverse = Reverse(money.ToString());
        //    var sb = new StringBuilder();
        //    var length = reverse.Length;
        //    sb.Append(reverse);
        //    if (reverse.Length >= 4)
        //    {
        //        for (int i = 3; ; i += 4)
        //        {
        //            if (i >= sb.Length)
        //                break;
        //            sb.Insert(i, ".");
        //        }
        //    }
        //    return Reverse(sb.ToString()) + " VND";
        //}

        public  string DateFormat(DateTime dateTime)
        {
            // Theo văn hóa Việt Nam.
            CultureInfo viVn = new CultureInfo("vi-VN");
            // ==> 12/20/2015 (dd/MM/yyyy)
            string dateStr = dateTime.ToString("g", viVn);
            return dateStr;
        }

        public string MoneyFormat(double money)
        {
            //return money.ToString("{0:c}") + " ₫";
            //var info = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
            //return String.Format(info, "{0:c}", money);
            NumberFormatInfo nfi = new CultureInfo("vi-VN", false).NumberFormat;
            return money.ToString("C0", nfi);
        }

        public string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public async Task<string> EncryptAsync(string password)
        {
            var _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            //var precision = configuration.GetValue<int>("Formatting:Number:Precision");
            string key = _configuration["KeyEncrypt"];
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(password);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public async Task<string> DecryptAsync(string password)
        {
            var _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            string key = _configuration["KeyEncrypt"];
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(password);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }

        }
    }
}
