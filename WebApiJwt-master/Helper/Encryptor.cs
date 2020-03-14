using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Daewoong.BI.Helper
{
    public class Encryptor
    {

        private RijndaelManaged _rManaged = new RijndaelManaged();
        private UTF8Encoding _utf8Encoder = new UTF8Encoding();

        public  Encryptor(string ivKey, string ivV)
        {
            MD5 _md5 = new MD5CryptoServiceProvider();

            _rManaged.KeySize = 256;
            _rManaged.BlockSize = 128;
            _rManaged.Mode = CipherMode.CBC;
            _rManaged.Padding = PaddingMode.PKCS7;
            _rManaged.Key = _md5.ComputeHash(_utf8Encoder.GetBytes(ivKey));
            _rManaged.IV = _md5.ComputeHash(_utf8Encoder.GetBytes(ivV));
        }

        /// <summary>암호화</summary>
        /// <param name="val">암호화할 값</param>
        /// <returns>암호화된 값</returns>
        public string Encrypt(string val)
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

        public static string Decrypt(string cipherData, string bKey, string bIV)
        {
            MD5 _md5 = new MD5CryptoServiceProvider();

            UTF8Encoding _utf8Encoder = new UTF8Encoding();

            byte[] key = _md5.ComputeHash(_utf8Encoder.GetBytes(bKey));
            byte[] iv = _md5.ComputeHash(_utf8Encoder.GetBytes(bIV));

            try
            {
                using (var rijndaelManaged =
                       new RijndaelManaged { Key = key, IV = iv, Mode = CipherMode.CBC, KeySize = 256, BlockSize = 128, Padding = PaddingMode.PKCS7 })
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
    }   
}
