using PCL.Core.MZMC.API;
using Microsoft.Extensions.Logging;
using PCL.Core.MZMC.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using PCL.Core.MZMC;
using System;

namespace PCL.Test
{
    [TestClass]
    public class TestMZMC
    {
        [TestMethod]
        public async Task Tester()
        {
            PCL.Core.MZMC.Init.Main();
            Log.Logger.LogInformation("用户登录测试");
            var access = true;
            var user = new SDK.User();
            var message = new Message(true,"");

            message = user.Login("tsxc_java", "332303301");
            Assert.IsTrue(message.OK, message.ToString());
            message = user.IfLogin();
            Assert.IsTrue(message.OK, message.ToString());
            message = user.GetProfile();
            Assert.IsTrue(message.OK, message.ToString());
            message = user.GetQQ();
            Assert.IsTrue(message.OK, message.ToString());
            message = user.BindQQ("123456789");
            Assert.IsTrue(message.OK, message.ToString());
            message = user.Logout();
            Assert.IsTrue(message.OK, message.ToString());

            Log.Logger.LogInformation("用户登录测试结束");
            
        }
    }
}