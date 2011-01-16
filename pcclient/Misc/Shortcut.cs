using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace pcclient
{
    internal class Shortcut
    {
        public static void SetStartupGroupShortcut(bool enable)
        {

            if (!pcclient.ClickOnce.Utils.IsApplicationNetworkDeployed)
            {
                RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (enable)
                {
                    runKey.SetValue(System.Windows.Forms.Application.ProductName, System.Windows.Forms.Application.ExecutablePath);
                }
                else
                {
                    runKey.DeleteValue(System.Windows.Forms.Application.ProductName, false);
                }
            }
            else
            {
                //TODO BSG: What happens on clickonce updates?  Does this change the required .appref-ms reference?
                Assembly assembly = Assembly.GetExecutingAssembly();

                // Get the startup shortcut and delete file to disable.
                string productName = GetProductName(assembly);
                string startupShortcut = GetStartupShortcut(productName);

                if (!enable) File.Delete(startupShortcut);

                // Copy program files shortcut into the startup folder to enable.
                string publisherName = GetPublisherName(assembly);
                string programShortcut = GetProgramShortcut(productName, publisherName);
                if (File.Exists(programShortcut))
                    File.Copy(programShortcut, startupShortcut);        
            }
        }
        
        private static string GetProgramShortcut(string product, string publisher)
        {
            string allProgramsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            string shortcutPath = Path.Combine(allProgramsPath, publisher);
            shortcutPath = Path.Combine(shortcutPath, product) + ".appref-ms";
            return shortcutPath;
        }

        private static string GetStartupShortcut(string product)
        {
            string startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            startupPath = Path.Combine(startupPath, product) + ".appref-ms";
            return startupPath;
        }

        private static string GetProductName(Assembly assembly)
        {
            if (!Attribute.IsDefined(assembly, typeof (AssemblyProductAttribute))) 
                return string.Empty;
            
            var product = (AssemblyProductAttribute) Attribute.GetCustomAttribute(assembly, typeof (AssemblyProductAttribute));

            return product.Product;
            
        }

        private static string GetPublisherName(Assembly assembly)
        {
            if (!Attribute.IsDefined(assembly, typeof(AssemblyCompanyAttribute))) 
                return string.Empty;
            
            var company = (AssemblyCompanyAttribute) Attribute.GetCustomAttribute(assembly, typeof (AssemblyCompanyAttribute));

            return company.Company;
            
        }
    }
}
