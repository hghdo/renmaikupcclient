using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Snarl;

namespace NativeWindowApplication
{

    // Summary description for WittySnarlMsgWnd.
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    
    
    public class WittySnarlMsgWnd : NativeWindow
    {
        CreateParams cp = new CreateParams();

        int SNARL_GLOBAL_MESSAGE;

        public WittySnarlMsgWnd()
        {
            // Create the actual window
            this.CreateHandle(cp);
            this.SNARL_GLOBAL_MESSAGE = Snarl.SnarlConnector.GetGlobalMsg();
        }

        protected override void WndProc(ref Message m)
        {

        if (m.Msg == this.SNARL_GLOBAL_MESSAGE)
        {
            if ((int)m.WParam == Snarl.SnarlConnector.SNARL_LAUNCHED)
            {
                // Snarl has been (re)started during Shaim already running
                // so let's register (again)
                Snarl.SnarlConnector.GetSnarlWindow(true);

            SnarlConnector.RegisterConfig(this.Handle, "Witty", Snarl.WindowsMessage.WM_USER + 55);

            SnarlConnector.RegisterAlert("Witty", "New tweet");
            SnarlConnector.RegisterAlert("Witty", "New tweets summarized");
            SnarlConnector.RegisterAlert("Witty", "New reply");
            SnarlConnector.RegisterAlert("Witty", "New direct message");

            }
        }
            base.WndProc(ref m);

        }

    }

}
