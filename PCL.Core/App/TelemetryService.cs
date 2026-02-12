using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Win32;
using PCL.Core.Net;
using PCL.Core.Net.Dns;
using PCL.Core.Net.Http.Client;
using PCL.Core.Utils.OS;
using STUN.Client;
using Sentry;

namespace PCL.Core.App;

[LifecycleScope("Telemetry", "遥测")]
[LifecycleService(LifecycleState.Running)]
public sealed partial class TelemetryService
{

    // ReSharper disable UnusedAutoPropertyAccessor.Local

    private class TelemetryDeviceEnvironment
    {
        public required string Tag { get; set; }
        public required string Id { get; set; }
        [JsonPropertyName("OS")] public required int Os { get; set; }
        public required bool Is64Bit { get; set; }
        [JsonPropertyName("IsARM64")] public required bool IsArm64 { get; set; }
        public required string Launcher { get; set; }
        public required string LauncherBranch {get; set; }
        [JsonPropertyName("UsedOfficialPCL")] public required bool UsedOfficialPcl { get; set; }
        [JsonPropertyName("UsedCommunityPCL")] public required bool UsedCommunityPcl { get; set; }
        [JsonPropertyName("UsedHMCL")] public required bool UsedHmcl { get; set; }
        [JsonPropertyName("UsedBakaXL")] public required bool UsedBakaXl { get; set; }
        public required ulong Memory { get; set; }
        public required string? NatMapBehaviour { get; set; }
        public required string? NatFilterBehaviour { get; set; }
        [JsonPropertyName("IPv6Status")] public required string Ipv6Status { get; set; }
    }

    private const string STUN_SERVER_ADDR = "stun.miwifi.com";

    // ReSharper restore UnusedAutoPropertyAccessor.Local
    [LifecycleStart]
    private static async Task _StartAsync()
    {
        #if DEBUG
        return;
        #endif

        var telemetryKey = EnvironmentInterop.GetSecret("TELEMETRY_KEY");
        if (string.IsNullOrWhiteSpace(telemetryKey)) return;
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // stun test
        StunClient5389UDP? natTest = default;
        var miWifiIps = await DnsQuery.Instance.QueryForIpAsync(STUN_SERVER_ADDR).ConfigureAwait(false);
        try
        {
            miWifiIps ??= await Dns.GetHostAddressesAsync(STUN_SERVER_ADDR).ConfigureAwait(false);
        } catch(Exception) { /* Ignore dns error */ }

        if (miWifiIps != null && miWifiIps.Length != 0)
        {
            natTest = new StunClient5389UDP(new IPEndPoint(miWifiIps.First(), 3478),
                new IPEndPoint(IPAddress.Any, 0));
            await natTest.QueryAsync().ConfigureAwait(false);
        }

        var telemetry = new TelemetryDeviceEnvironment
        {
            Tag = "Telemetry",
            Id = Utils.Secret.Identify.LauncherId,
            Os = Environment.OSVersion.Version.Build,
            Is64Bit = Environment.Is64BitOperatingSystem,
            IsArm64 = RuntimeInformation.OSArchitecture.Equals(Architecture.Arm64),
            Launcher = Basics.VersionName,
            LauncherBranch = Config.Update.UpdateChannel switch
            {
                UpdateChannel.Release => "Release",
                UpdateChannel.Beta => "Beta",
                UpdateChannel.Dev => "Dev",
                _ => "Unknown"
            },
            UsedOfficialPcl =
                bool.TryParse(Registry.GetValue(@"HKEY_CURRENT_USER\Software\PCL", "SystemEula", "false") as string,
                    out var officialPcl) && officialPcl,
            UsedCommunityPcl = Directory.Exists(Path.Combine(appDataFolder, "PCLCE")),
            UsedHmcl = Directory.Exists(Path.Combine(appDataFolder, ".hmcl")),
            UsedBakaXl = Directory.Exists(Path.Combine(appDataFolder, "BakaXL")),
            Memory = KernelInterop.GetPhysicalMemoryBytes().Total,
            NatMapBehaviour = natTest?.State.MappingBehavior.ToString(),
            NatFilterBehaviour = natTest?.State.FilteringBehavior.ToString(),
            Ipv6Status = NetworkInterfaceUtils.GetIPv6Status().ToString()
        };

        var telemetryEvent = new SentryEvent();
        telemetryEvent.Level = SentryLevel.Info;
        telemetryEvent.Message = "设备环境调查数据";
        telemetryEvent.TransactionName = "Telemetry";
        foreach (var info in telemetry.GetType().GetProperties())
        {
            telemetryEvent.SetTag(info.Name, info.GetValue(telemetry).ToString());
        }
        SentrySdk.CaptureEvent(telemetryEvent);
        Context.Info("已发送设备环境调查数据");
        // Context.Error("设备环境调查数据发送失败，请检查网络连接以及使用的版本");
        Context.DeclareStopped();
    }
}