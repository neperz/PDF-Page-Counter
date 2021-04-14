using System;
using System.Collections.Specialized;

namespace PDF_Page_Counter.Cripto
{
    [Serializable]
    public class CAppSettings : NameValueCollection
    {
        private static string m_sCryptKey;
        private static string m_sCryptPrefix;

        static CAppSettings()
        {

            CAppSettings.m_sCryptKey = "B20E89B5-74AA-4DB3-ABB4-6A605537A915";
            CAppSettings.m_sCryptPrefix = "CRYPT.1:";
        }

        public CAppSettings()
        {
        }

        //public abstract void AddHashTableParams(Hashtable htParameters);

        public static string DecryptString(string sKey, string sValue)
        {
            string str;
            try
            {
                str = CAppSettings.DecryptString(sValue);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                throw new Exception(string.Format("Unable to get key setting '{0}'. {1}", sKey, exception.Message), exception);
            }
            return str;
        }

        public static string DecryptString(string sValue)
        {
            if (!CAppSettings.IsEncrypted(sValue))
            {
                return sValue;
            }
            return (new Cryptography()).DecryptData(CAppSettings.m_sCryptKey, sValue.Substring(CAppSettings.m_sCryptPrefix.Length));
        }

        public static string EncryptString(string sValue)
        {
            string str;
            try
            {
                Cryptography cryptography = new Cryptography();
                str = string.Concat(CAppSettings.m_sCryptPrefix, cryptography.EncryptData(CAppSettings.m_sCryptKey, sValue));
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                throw new Exception(string.Format("Unable to encrypt text. {0}", exception.Message), exception);
            }
            return str;
        }
        public static bool IsEncrypted(string sValue)
        {
            if (sValue.Length < CAppSettings.m_sCryptPrefix.Length)
            {
                return false;
            }
            return sValue.Substring(0, CAppSettings.m_sCryptPrefix.Length) == CAppSettings.m_sCryptPrefix;
        }
    }
}
