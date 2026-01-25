using Newtonsoft.Json;
using PCL.Core.MZMC;
using PCL.Core.MZMC.Helper;
using RestSharp;
using System;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using PCL.Core.App;

namespace PCL.Core.MZMC.API
{
	public class SDK
	{
		private const string BaseUrl= "https://api.mzmc.top";

		private const string ClientId = "5"; // 客户端ID
		private const string ClientSecret = "oIZryQhWCuoKEG985nSNHALSoFnl4kBt"; // 客户端密钥
		public const string RedirectUrl = "http://localhost:49287/callback/"; // Oauth回调地址
		public static string OAuthUrl => $"{BaseUrl}/user/oauth2/authorize?client_id={ClientId}";

		/// <summary>
		/// 发送请求
		/// </summary>
		/// <param name="request">请求参数</param>
		/// <returns></returns>
		public static Message Request(RestRequest request)
		{
			var client = new RestClient(BaseUrl);
			var result = client.Execute(request);
			if(result.ResponseStatus==ResponseStatus.Error)
				return new Message(false,result.Content);
			dynamic content=JsonConvert.DeserializeObject(result.Content);
			return new Message(result.IsSuccessStatusCode,content.msg.Value,content.data);
		}

		public class User
		{

            private long _id = -1;
			private string _username = "";
			private string _email = "";
            private string _token = "";

			private string _realname = "";
            private string _nickname = "";



            public string Username{get => _username;}

            private void _SaveUserInfo()
            {
	            if(MZMCService.Config.AppSettings.Settings["UserToken"]==null)
		            MZMCService.Config.AppSettings.Settings.Add("UserToken", this._token);
	            else
		            MZMCService.Config.AppSettings.Settings["UserToken"].Value = this._token;
	            MZMCService.Config.Save(ConfigurationSaveMode.Modified);
            }

			public Message IfLogin()
			{
                if (_token == "")
                    return new Message(false, "未登录");
                var request = new RestRequest("/user/auth/check", Method.Post).AddHeader("Authorization", $"Bearer {_token}");
                var message = Request(request);

				if (message.OK == false)
					_token = "";

                _SaveUserInfo();

                return message;
            }

			/// <summary>
			/// 登录（用户名）
			/// </summary>
			/// <param name="username">用户名</param>
			/// <returns>Message，不带Data</returns>
			public Message Login(string username, string password)
			{
				MZMCService.Context.Debug("尝试登录（用户名）");
				var sha256=Converter.ComputeSHA256(password).ToLower();
                var request = new RestRequest("/user/auth/login", Method.Post).AddHeader("Authorization", $"Bearer {_token}");
                var message = Request(request.AddJsonBody(new { username = username, password = sha256 }));

				if (message.OK == true)
				{
					_token = message.Data.access_token.Value;
                    GetProfile();
                }

				return new Message(message.OK, message.MessageText);
            }

            /// <summary>
            /// 登录（Token）
            /// </summary>
            /// <param name="token">Token</param>
            /// <returns>Message，不带Data</returns>
            public Message LoginByToken(string token)
            {
                _token = token;
                var message = GetProfile();

                return new Message(message.OK, message.MessageText);
            }

            /// <summary>
            /// 登录（OAuth）
            /// 注意需要通过OAuth提前获得code才能使用该登录方式
            /// </summary>
            /// <param name="code"></param>
            /// <returns></returns>
            public Message OAuthLogin(string code)
            {
	            var request = new RestRequest("/user/oauth2/token", Method.Post);
	            request.AddJsonBody(new { client_id = ClientId, client_secret = ClientSecret, code = code });
	            var client = new RestClient(BaseUrl);
	            var result = client.Execute(request);
	            if(result.ResponseStatus==ResponseStatus.Error||result.Content.Contains("error"))
		            return new Message(false,result.Content);
	            dynamic content=JsonConvert.DeserializeObject(result.Content);
	            var message = new Message(result.IsSuccessStatusCode,"操作成功",content);

	            if (message.OK == true)
	            {
		            _token = message.Data.access_token.Value;
		            GetProfile();
	            }

	            return new Message(message.OK, message.MessageText);
            }

            /// <summary>
            /// 登出
            /// </summary>
            /// <returns>Message，无data</returns>
            public Message Logout()
			{
				if (IfLogin().OK == false)
					return new Message(false, "未登录");
                var request = new RestRequest("/user/auth/logout", Method.Post).AddHeader("Authorization", $"Bearer {_token}");
                var message = Request(request);
				_token = "";
				return message;
			}

			public Message GetProfile()
            {
                if (IfLogin().OK == false)
                    return new Message(false, "未登录");
				var request = new RestRequest("/player/profile", Method.Get).AddHeader("Authorization", $"Bearer {_token}");
				var message = Request(request);

				if (message.OK == true)
                {
                    _id = message.Data.id.Value;
                    _username = message.Data.username.Value;
                    _realname = message.Data.realname.Value;
                    _nickname = message.Data.nickname.Value;
                    _email = message.Data.email.Value;

                }
				return message;
			}


            public Message GetQQ()
            {
                if (IfLogin().OK == false)
                    return new Message(false, "未登录");
                var request = new RestRequest("/user/bind/qq", Method.Get).AddHeader("Authorization", $"Bearer {_token}");
                var message = Request(request);
                return message;
            }

            public Message BindQQ(string QQ)
            {
                if (IfLogin().OK == false)
                    return new Message(false, "未登录");
                var request = new RestRequest("/user/bind/qq", Method.Post).AddHeader("Authorization", $"Bearer {_token}");
                var message = Request(request.AddJsonBody(new { qq_id = QQ }));
                return message;
            }
        }
	}
}
