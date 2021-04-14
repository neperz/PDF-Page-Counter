using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PDF_Page_Counter.Cripto
{
    public class Cryptography
    {
        private static byte[] m_Key;

        private static byte[] m_IV;

        private UTF8Encoding m_oEncoding = new UTF8Encoding();

        public static string InternalError { get; private set; }

        static Cryptography()
        {
            Cryptography.m_Key = new byte[8];
            Cryptography.m_IV = new byte[8];
        }

        public Cryptography()
        {
        }

        public string DecryptData(string strKey, string strData)
        {
            byte[] numArray;
            string str;
            if (!Cryptography.InitKey(strKey))
            {
                throw new Exception("Error. Fail to generate key for decryption");
            }
            try
            {
                numArray = Convert.FromBase64CharArray(strData.ToCharArray(), 0, strData.Length);
            }
            catch (Exception exception)
            {
                throw new Exception("Error. Input Data is not base64 encoded.", exception);
            }
            try
            {
                str = this.m_oEncoding.GetString(this.InternalDecryptData(numArray)).Substring(5);
            }
            catch (Exception exception1)
            {
                throw new Exception("Error. Decryption Failed. Possibly due to incorrect Key or corrputed data", exception1);
            }
            return str;
        }

        public byte[] DecryptData(string strKey, byte[] data)
        {
            byte[] numArray;
            if (!Cryptography.InitKey(strKey))
            {
                throw new Exception("Error. Fail to generate key for decryption");
            }
            try
            {
                numArray = this.InternalDecryptData(data);
            }
            catch (Exception exception)
            {
                throw new Exception("Error. Decryption Failed. Possibly due to incorrect Key or corrputed data", exception);
            }
            return numArray;
        }

        public string EncryptData(string strKey, string strData)
        {
            string str;
            if (strData.Length > 50000000)
            {
                str = "Error. Data String too large. Keep within 50Mb.";
                throw new Exception(str);
            }
            if (this.m_oEncoding.GetType().Equals(typeof(ASCIIEncoding)))
            {
                int num = 0;
                int length = strData.Length;
                while (num < length)
                {
                    if (strData[num] >= '\u0080')
                    {
                        str = string.Format("Error. Character '{0}' is not supported for encryption.", strData[num]);
                        throw new Exception(str);
                    }
                    num++;
                }
            }
            if (!Cryptography.InitKey(strKey))
            {
                str = "Error. Fail to generate key for encryption";
                throw new Exception(str);
            }
            strData = string.Concat(string.Format("{0,5:00000}", Math.Min(strData.Length, 99999)), strData);
            ArraySegment<byte> nums = Cryptography.InternalEncryptData(this.m_oEncoding.GetBytes(strData));
            str = (nums.Array == null || nums.Count == 0 ? "" : Convert.ToBase64String(nums.Array, 0, nums.Count));
            return str;
        }

        public byte[] EncryptData(string strKey, byte[] inData)
        {
            if ((int)inData.Length > 50000000)
            {
                throw new Exception("Error. Data String too large. Keep within 50Mb.");
            }
            if (!Cryptography.InitKey(strKey))
            {
                throw new Exception("Error. Fail to generate key for encryption");
            }
            ArraySegment<byte> nums = Cryptography.InternalEncryptData(inData);
            return nums.Array.SubArray<byte>(0, nums.Count);
        }

        public string Hash2EncryptData(string strData)
        {
            byte[] numArray = (new SHA512Managed()).ComputeHash(this.m_oEncoding.GetBytes(strData));
            StringBuilder stringBuilder = new StringBuilder((int)numArray.Length);
            for (int i = 0; i < (int)numArray.Length; i++)
            {
                stringBuilder.Append(numArray[i].ToString("X2"));
            }
            return stringBuilder.ToString();
        }

        private static bool InitKey(string strKey)
        {
            int i;
            bool flag;
            try
            {
                byte[] numArray = new byte[strKey.Length];
                (new ASCIIEncoding()).GetBytes(strKey, 0, strKey.Length, numArray, 0);
                byte[] numArray1 = (new SHA1CryptoServiceProvider()).ComputeHash(numArray);
                for (i = 0; i < 8; i++)
                {
                    Cryptography.m_Key[i] = numArray1[i];
                }
                for (i = 8; i < 16; i++)
                {
                    Cryptography.m_IV[i - 8] = numArray1[i];
                }
                flag = true;
            }
            catch (Exception exception)
            {
                flag = false;
                InternalError = exception.Message;
            }
            return flag;
        }

        private byte[] InternalDecryptData(byte[] bPlain)
        {
            byte[] array;
            ICryptoTransform cryptoTransform = (new DESCryptoServiceProvider()).CreateDecryptor(Cryptography.m_Key, Cryptography.m_IV);
            CryptoStream cryptoStream = new CryptoStream(new MemoryStream(bPlain), cryptoTransform, CryptoStreamMode.Read);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                {
                    streamWriter.Write((new StreamReader(cryptoStream)).ReadToEnd());
                    streamWriter.Flush();
                    streamWriter.Close();
                    array = memoryStream.ToArray();
                }
            }
            return array;
        }

        private static ArraySegment<byte> InternalEncryptData(byte[] inData)
        {
            ArraySegment<byte> nums;
            ICryptoTransform cryptoTransform = (new DESCryptoServiceProvider()).CreateEncryptor(Cryptography.m_Key, Cryptography.m_IV);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(inData, 0, (int)inData.Length);
                    cryptoStream.FlushFinalBlock();
                    ArraySegment<byte> nums1 = new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                    cryptoStream.Close();
                    nums = nums1;
                }
            }
            return nums;
        }
    }
}