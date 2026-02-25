using System;
using PCL.Core.MZMC.Helper;
using PCL.Core.MZMC.API;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using PCL.Core.App;
using Sentry;
using System.Windows.Threading;
using System.Windows;
using PCL.Core.App.IoC;
using PCL.Core.Utils.OS;

namespace PCL.Core.MZMC
{
    [LifecycleService(LifecycleState.BeforeLoading, Priority = 0)]
    public sealed class MZMCService : GeneralService
    {
        #region ILifecycleService 实现

        private static LifecycleContext? _context;
        public static LifecycleContext Context => _context!;
        private MZMCService() : base("MZMCService", "MZMC 相关服务", false) { _context = ServiceContext; }


        public override void Start()
        {
            // 初始化
            if (Config.AppSettings.Settings["UserToken"] != null)
                User.LoginByToken(Config.AppSettings.Settings["UserToken"].Value);

            StartSentry();
        }

        #endregion

        public static HttpListener GlobalHttpListener = new HttpListener();

        public static Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        public static SDK.User User = new SDK.User();

        void StartSentry()
        {
            // Sentry
            SentrySdk.Init(o =>
            {
                var telemetryKey = EnvironmentInterop.GetSecret("TELEMETRY_KEY");
                if (string.IsNullOrWhiteSpace(telemetryKey)) return;
                o.Dsn = $"https://{telemetryKey}@sentry.tsxc.xyz/2";
                // When configuring for the first time, to see what the SDK is doing:
                #if DEBUG
                o.Debug = true;
                #endif
                // Set TracesSampleRate to 1.0 to capture 100% of transactions for tracing.
                // We recommend adjusting this value in production.
                o.TracesSampleRate = 1.0;
                o.Release = Basics.VersionName;
            });
        }


        public static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException(e.Exception);

            // If you want to avoid the application from crashing:
            // e.Handled = true;
        }
    }
}