using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using log4net;

namespace pcclient
{
    public class SingleInstanceManager : IDisposable
    {
        private static readonly ILog logger = LogManager.GetLogger("Witty.Logging");

        private bool disposed = false;

        [DllImport("kernel32.dll")]
        static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);
        [DllImport("kernel32.dll")]
        static extern bool SetEvent(IntPtr hEvent);
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenEvent(UInt32 dwDesiredAccess, bool bInheritable, string lpName);
        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr hHandle);
        [DllImport("kernel32.dll")]
        static extern bool ResetEvent(IntPtr hEvent);

        private const uint INFINITE = 0xFFFFFFFF;
        private const string EVENT_NAME = "UM_USER_SHOW_WINDOW";
        private const uint SYNCHRONIZE = 0x00100000;
        private const uint EVENT_MODIFY_STATE = 0x0002;

        private IntPtr m_EventHandle = IntPtr.Zero;		// unmanaged

        private Window m_Instance;

        public delegate void ShowApplicationCallback();
        private ShowApplicationCallback m_callback;

        // constructor
        public SingleInstanceManager(Window instance, ShowApplicationCallback callback)
        {
            m_Instance = instance;
            m_callback = callback;

            //try to our event
            m_EventHandle = OpenEvent(EVENT_MODIFY_STATE | SYNCHRONIZE, false, EVENT_NAME);
            if (m_EventHandle == IntPtr.Zero) //if it doesn't exist
            {
                //create our event
                m_EventHandle = CreateEvent(IntPtr.Zero, true, false, EVENT_NAME);
                if (m_EventHandle != IntPtr.Zero) //if successfull
                {
                    Thread thread = new Thread(new ThreadStart(WaitForSignal)) { Name = "Single instance manager" };
                    thread.Start();
                }
            }
            else
            {
                SetEvent(m_EventHandle);
                MessageBox.Show("There is already an instance of Witty");
                Environment.Exit(0);
            }
        }

        // destructor
        ~SingleInstanceManager()
        {
            Dispose(false);
        }

        // an instance calls this when it detects that it is
        // the second instance.  Then it exits
        public void SignalEvent()
        {
            if (m_EventHandle != IntPtr.Zero)
            {
                SetEvent(m_EventHandle);
            }
        }

        // thread method will wait on the event, which will signal
        // if another instance tries to start
        private void WaitForSignal()
        {
            while (true)
            {
                uint result = WaitForSingleObject(m_EventHandle, INFINITE);

                if (result == 0)
                {
                    m_Instance.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, m_callback);
                    ResetEvent(m_EventHandle);
                }
                else
                {
                    return;
                }
            }
        }

        #region IDisposable Members

        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (!this.disposed)
            {
                // dispose unmanaged resources
                if (m_EventHandle != IntPtr.Zero)
                {
                    CloseHandle(m_EventHandle);
                }
                m_EventHandle = IntPtr.Zero;

                disposed = true;
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}