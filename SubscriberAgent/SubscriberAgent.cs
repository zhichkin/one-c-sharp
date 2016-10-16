using System;
using System.Timers;
using System.Diagnostics;
using System.Configuration;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using Zhichkin.Integrator.Model;
using Zhichkin.Integrator.Services;

namespace SubscriberAgent
{
    public partial class SubscriberAgent : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        private readonly Timer timer = new Timer();
        private static bool is_busy = false;
        private static object sync_root = new object();
        private readonly EventLog log = new EventLog();
        private const string CONST_ServiceName = "Z-Integrator Subscriber Service";
        private readonly IntegratorService integrator = new IntegratorService();

        public SubscriberAgent()
        {
            InitializeComponent();
            InitializeLog();
        }
        private void InitializeLog()
        {
            log.Source = CONST_ServiceName;
            log.Log = string.Empty;
            this.AutoLog = false;
        }
        private bool IsBusy
        {
            get
            {
                return is_busy;
            }
            set
            {
                if (!value)
                {
                    is_busy = false;
                }
                else
                {
                    lock (sync_root)
                    {
                        if (is_busy)
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                        is_busy = true;
                    }
                }
            }
        }
        protected override void OnStart(string[] args)
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 60000; // 60 seconds
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            try
            {
                StartService();
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(ex);
                this.Stop();
                return;
            }

            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            log.WriteEntry("Service start.");
        }
        private void StartService()
        {
            timer.Interval = GetShedule();
            timer.Elapsed += DoWork;
            timer.Start();
        }
        protected override void OnStop()
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 60000; // 60 seconds
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            StopService();

            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            log.WriteEntry("Service stop.");
        }
        private void StopService()
        {
            // try to stop pending operations here
        }
        private int GetShedule()
        {
            int interval = 60; // seconds
            string schedule = ConfigurationManager.AppSettings["Schedule"];
            if (!string.IsNullOrWhiteSpace(schedule))
            {
                int.TryParse(schedule, out interval);
            }
            return interval * 1000;
        }
        private void DoWork(object sender, ElapsedEventArgs args)
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true; // TODO: this.CanStop = false;
                this.ExecuteServiceTask();
            }
            catch (Exception ex) // the agent is already running
            {
                WriteExceptionToLog(ex); return;
            }
            finally
            {
                IsBusy = false; // TODO: this.CanStop = true;
            }
        }
        private void WriteExceptionToLog(Exception ex)
        {
            log.WriteEntry(ex.Message, EventLogEntryType.Error);
        }
                
        private void ExecuteServiceTask()
        {
            int messagesProcessed = 0;
            foreach (Publisher publisher in integrator.GetPublishers())
            {
                try
                {
                    messagesProcessed = integrator.ProcessMessages(publisher);
                    if (messagesProcessed != 0)
                    {
                        log.WriteEntry(GetSuccessText(publisher, messagesProcessed), EventLogEntryType.Information);
                    }
                }
                catch (Exception ex)
                {
                    log.WriteEntry(GetErrorText(ex), EventLogEntryType.Error);
                }
            }
        }
        private string GetErrorText(Exception ex)
        {
            string errorText = string.Empty;
            Exception error = ex;
            while (error != null)
            {
                errorText += (errorText == string.Empty) ?
                    error.Message :
                    Environment.NewLine + "-" + Environment.NewLine + error.Message;
                error = error.InnerException;
            }
            return errorText;
        }
        private string GetSuccessText(Publisher publisher, int messagesProcessed)
        {
            return string.Format("Publisher \"{0}\": {1} messages processed.",
                publisher.Name,
                messagesProcessed.ToString());
        }
    }
}
