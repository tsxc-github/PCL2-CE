using System;
using PCL.Core.MZMC.Helper;
using PCL.Core.MZMC.API;
using System.Configuration;
using System.Threading.Tasks;
using PCL.Core.App;


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
        }

        #endregion

        public static Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        public static SDK.User User = new SDK.User();
    }
}