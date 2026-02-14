using System;
using PCL.Core.MZMC.API;

namespace PCL.Core.MZMC.Yggdrasil;

public class Yggdrasil
{
    public const string ServerUrl = "https://api.mzmc.top";
    public static string GetToken()
    {
        var result = SDK.GetTokenWithCode(SDK.GetCodeWithOauth());
        if(result.OK==false)
            throw new Exception(result.MessageText);
        return result.Data.access_token.Value;
    }
}