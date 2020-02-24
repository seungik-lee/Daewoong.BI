using Daewoong.BI.Controllers;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {

       

        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            //Encr95/Hs4BeUQBinPO6WLRDNxleLZAwHH4L4TXojn47Fcs=
            //var test = AESEncrypter.Encrypt("bkyim");

            //var d = AESEncrypter.Encrypt("changwolf@daewoong.co.kr");
            //var d = AESEncrypter.Encrypt("apsproject@idstrust.com");
            //D3xhtcLMM+ThdD15Vkxq64J8yQwsRvMm//u5UPJ//r4=
            //r+wvRtlSAkHqT/MavbJnAn5O1uu6qyD6e5ZQ8OMjMeM=
            ////cwfPNUeDQ/U3hXpS/oSV1xbnKLuV1CKnr9upflfJi1w=
            //var d = AESEncrypter.Decrypt("cwfPNUeDQ/U3hXpS/oSV1xbnKLuV1CKnr9upflfJi1w=");
            //var d = AESEncrypter.Decrypt("D3xhtcLMM+ThdD15Vkxq64J8yQwsRvMm//u5UPJ//r4="); 
            //var d = AESEncrypter.Decrypt("r%2BwvRtlSAkHqT%2FMavbJnAn5O1uu6qyD6e5ZQ8OMjMeM%3D"); 
            //Console.WriteLine("d === "+ d);

            var d = AESEncrypter.Decrypt("D3xhtcLMM%2BThdD15Vkxq64J8yQwsRvMm%2F%2Fu5UPJ%2F%2Fr4%3D"); //bkyim@idstrust.com
            //"D3xhtcLMM+ThdD15Vkxq64J8yQwsRvMm//u5UPJ//r4="
            Console.WriteLine("d === " + d);


            //var d = AESEncrypter.Decrypt("KDGqfJEKhiEB%2FQG9zYJwLUK7yfzcgG66mt8Go58b8aY%3D"); //indy761@daewoong.co.kr
            //"KDGqfJEKhiEB/QG9zYJwLUK7yfzcgG66mt8Go58b8aY="


            CreateUsers();
        }

        private static void CreateUsers()
        {
                
        }
    }

    public static class AESEncrypter
    {
        private static RijndaelManaged _rManaged = new RijndaelManaged();
        private static UTF8Encoding _utf8Encoder = new UTF8Encoding();

        public static string Decrypt(string cipherData)
        {
            MD5 _md5 = new MD5CryptoServiceProvider();
              //string fineKey = "~gw-biadvancement-SecretKey";
              //string fineIV = "~gw-biadvancement-InitVector";


            string fineKey = "~gw-biportal-SecretKey";
            string fineIV = "~gw-biportal-InitVector";

            //key = "~gw-biportal-SecretKey";
            //    iv = "~gw-biportal-InitVector";


            //byte[] key = _md5.ComputeHash(_utf8Encoder.GetBytes("~gw-biportal-SecretKey"));
            //byte[] iv = _md5.ComputeHash(_utf8Encoder.GetBytes("~gw-biportal-InitVector"));

            byte[] key = _md5.ComputeHash(_utf8Encoder.GetBytes(fineKey));
            byte[] iv = _md5.ComputeHash(_utf8Encoder.GetBytes(fineIV));

            try
            {
                using (var rijndaelManaged =
                       new RijndaelManaged { Key = key, IV = iv, Mode = CipherMode.CBC, KeySize=256, BlockSize=128, Padding = PaddingMode.PKCS7 }) 
                using (var memoryStream =
                       new MemoryStream(Convert.FromBase64String(cipherData)))
                using (var cryptoStream =
                       new CryptoStream(memoryStream,
                           rijndaelManaged.CreateDecryptor(key, iv),
                           CryptoStreamMode.Read))
                {
                    return new StreamReader(cryptoStream).ReadToEnd();
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
        }


        static AESEncrypter()
        {   
            MD5 _md5 = new MD5CryptoServiceProvider();


            string fineKey = "~gw-biportal-SecretKey";
            string fineIV = "~gw-biportal-InitVector";



            _rManaged.KeySize = 256;
            _rManaged.BlockSize = 128;
            _rManaged.Mode = CipherMode.CBC;
            _rManaged.Padding = PaddingMode.PKCS7;
            _rManaged.Key = _md5.ComputeHash(_utf8Encoder.GetBytes(fineKey));
            _rManaged.IV = _md5.ComputeHash(_utf8Encoder.GetBytes(fineIV));
        }
        /// <summary>암호화</summary>
        /// <param name="val">암호화할 값</param>
        /// <returns>암호화된 값</returns>
        public static string Encrypt(string val)
        {
            string _resVal = string.Empty;
            byte[] _utf8Val = null;
            byte[] _encryptVal = null;
            ICryptoTransform tForm = _rManaged.CreateEncryptor();

            try
            {
                if (!string.IsNullOrEmpty(val))
                {
                    _utf8Val = _utf8Encoder.GetBytes(val);
                    _encryptVal = tForm.TransformFinalBlock(_utf8Val, 0, _utf8Val.Length);
                    _resVal = Convert.ToBase64String(_encryptVal);
                }
                else
                    _resVal = string.Empty;
            }
            catch
            {
                _resVal = string.Empty;
            }

            return _resVal;
        }
    }
}
