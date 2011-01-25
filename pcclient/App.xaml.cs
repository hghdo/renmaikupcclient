using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using RenmeiLib;
using log4net;
using log4net.Config;

namespace pcclient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static User LoggedInUser = new User();
        public static log4net.ILog Logger = LogManager.GetLogger("pcclient.Logging");
        protected override void OnStartup(StartupEventArgs e)
        {
            object o = ConfigurationSettings.GetConfig("log4net");
            log4net.Config.DOMConfigurator.Configure(o as System.IO.FileInfo);
            //DOMConfigurator.Configure();

            Logger.Info("Renmei client is starting.");

            base.OnStartup(e);
        }
    }
}
