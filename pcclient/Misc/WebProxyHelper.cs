using System;
using System.Net;
using log4net;

namespace pcclient
{
    public static class WebProxyHelper
    {
        private static Properties.Settings AppSettings = Properties.Settings.Default;
        private static readonly ILog logger = LogManager.GetLogger("Witty.Logging");

        public static IWebProxy GetConfiguredWebProxy()
        {   
            WebProxy proxy = null;
            if (AppSettings.UseProxy)
            {
                try
                {
                    proxy = new WebProxy(AppSettings.ProxyServer, AppSettings.ProxyPort);

                    string[] user = AppSettings.ProxyUsername.Split('\\');

                    if (user.Length == 2)
                        proxy.Credentials = new NetworkCredential(user[1], DecryptString(AppSettings.ProxyPassword), user[0]);
                    else
                        proxy.Credentials = new NetworkCredential(AppSettings.ProxyUsername, DecryptString(AppSettings.ProxyPassword));
                }
                catch (UriFormatException ex)
                {
                    logger.Debug(ex.ToString());
                }
            }
            return proxy;
        }



        // REMARK: This is same encryption scheme that is used in TwitterNet class.  Should it
        //         be abstracted into a utility class?
        private static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("WittyPasswordSalt");
        private static string DecryptString(string encryptedData)
        {
            if (encryptedData.StartsWith("WittyEncrypted:"))
                encryptedData = encryptedData.Substring(15);

            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return System.Text.Encoding.Unicode.GetString(decryptedData);
            }
            catch
            {
                return String.Empty;
            }
        }
    }
}
