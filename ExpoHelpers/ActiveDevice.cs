using Microsoft.Tools.WindowsDevicePortal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
//using Windows.Security.Cryptography.Certificates;
//using Windows.UI.Core;
//using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using Xamarin.Essentials;
using System.Security.Cryptography.X509Certificates;
using Xamarin.Forms;
//using Windows.Storage.Streams;
//using Windows.System;

namespace ExpoHelpers
{
    public enum AppLaunchStatus
    {
        None,
        CommandSent,
        Failed,
        Succeeded
    };

    public class InstalledApplicationInfo
    {
        public string AppId { get; private set; }
        public string PackageName { get; private set; }
        public string DisplayName { get; private set; }

        public InstalledApplicationInfo(string appId, string packageName, string displayName)
        {
            this.AppId = appId;
            this.PackageName = packageName;
            this.DisplayName = displayName;
        }
    }

    public delegate void LogEventHandler(bool isError, string deviceId, string message);

    public delegate void AppLaunchStatusHendler(AppLaunchStatus status);



    public class ActiveDevice : NotifyPropertyChangedBase
    {
        private DateTime lastHearbeatTime;
        private TimeSpan disconnectTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Instance of the IDevicePortalConnection used to connect to this device.
        /// </summary>
        private IDevicePortalConnection devicePortalConnection;

        /// <summary>
        /// Instance of the DevicePortal used to communicate with this device.
        /// </summary>
        private DevicePortal devicePortal;
        private bool isConnecting = false;


        public bool UseFakeBackend { get; set; }
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }

        public bool GetRunningProcessesOnHeartbeat { get; set; } = false;

        public event LogEventHandler Log;

        public event AppLaunchStatusHendler AppLaunchStatusChanged;

        private bool operationInProgress = false;
        public bool OperationInProgress
        {
            get { return this.operationInProgress; }
            private set { this.PropertyChangedHelper(ref this.operationInProgress, value); }
        }

        private bool closed = false;
        public bool Closed
        {
            get { return this.closed; }
            set { this.PropertyChangedHelper(ref this.closed, value); }
        }

        private string batteryStatus = string.Empty;
        public string BatteryStatus
        {
            get { return this.batteryStatus; }
            set { this.PropertyChangedHelper(ref this.batteryStatus, value); }
        }

        private bool lowPower = false;
        public bool LowPower
        {
            get { return this.lowPower; }
            set { this.PropertyChangedHelper(ref this.lowPower, value); }
        }

        private bool middlePower = false;
        public bool MiddlePower
        {
            get { return this.middlePower; }
            set { this.PropertyChangedHelper(ref this.middlePower, value); }
        }

        private bool charged = false;
        public bool Charged
        {
            get { return this.charged; }
            set { this.PropertyChangedHelper(ref this.charged, value); }
        }

        private bool charging = false;
        public bool Charging
        {
            get { return this.charging; }
            private set { this.PropertyChangedHelper(ref this.charging, value); }
        }

        private bool connected = false;
        public bool Connected
        {
            get { return this.connected; }
            private set { this.PropertyChangedHelper(ref this.connected, value); }
        }

        private List<InstalledApplicationInfo> installedApplications;
        public List<InstalledApplicationInfo> InstalledApplications
        {
            get { return this.installedApplications; }
            set { this.PropertyChangedHelper(ref this.installedApplications, value); }
        }

        private DeviceConnectionStatus deviceConnectionStatus = DeviceConnectionStatus.None;
        public DeviceConnectionStatus DeviceConnectionStatus
        {
            get { return this.deviceConnectionStatus; }
            set { this.PropertyChangedHelper(ref this.deviceConnectionStatus, value); }
        }

        private DevicePortal.RunningProcesses runningProcesses = null;
        public DevicePortal.RunningProcesses RunningProcesses
        {
            get { return this.GetRunningProcessesOnHeartbeat ? this.runningProcesses : null; }
            private set { this.PropertyChangedHelper(ref this.runningProcesses, value); }
        }

        private string kioskModePackageName = null;
        public string KioskModeAppId
        {
            get { return this.kioskModePackageName; }
            set { this.PropertyChangedHelper(ref kioskModePackageName, value); }
        }

        public string KioskModePackageName
        {
            get
            {
                // try to find the app id in the installed apps and return the package name
                return this.InstalledApplications?.Find((a) => a.AppId == this.KioskModeAppId)?.PackageName;
            }
        }

        public ActiveDevice(DeviceInformation deviceInformation)
        {
            this.Id = deviceInformation.Id;
            this.Name = deviceInformation.Name;
            this.Address = deviceInformation.Address;
            this.UserName = string.IsNullOrEmpty(deviceInformation.UserName) ? AppSettings.DefaultUserName : deviceInformation.UserName;
            this.Password = string.IsNullOrEmpty(deviceInformation.Password) ? AppSettings.DefaultPassword : deviceInformation.Password;
            if (this.Address.StartsWith("fake", StringComparison.CurrentCultureIgnoreCase))
            {
                this.UseFakeBackend = true;
            }

            this.lastHearbeatTime = DateTime.UtcNow;
        }

        public async Task Heartbeat()
        {
            if(this.Closed)
            {
                return;
            }

            if(this.operationInProgress)
            {
                return;
            }

            using (new StatusHelper(this))
            {
                if (this.UseFakeBackend)
                {
                    await Task.Delay(1000);
                    var rand = new Random();

                    int powerLevel = (int)(rand.NextDouble() * 100.0);
                    this.BatteryStatus = powerLevel + "%";
                    this.LowPower = powerLevel <= AppSettings.LowPowerLevel;
                    this.Charged = powerLevel >= AppSettings.ChargedPowerLevel;
                    this.MiddlePower = !this.LowPower && !this.Charged;

                    this.lastHearbeatTime = DateTime.UtcNow - TimeSpan.FromSeconds(((int)(rand.NextDouble() * 1.5) * this.disconnectTimeout.TotalSeconds));

                    this.Charging = rand.NextDouble() > 0.5;

                    // TODO - grab the list of installed applications once - it shouldn't change normally.
                    if (this.InstalledApplications == null)
                    {
                        var apps = new List<InstalledApplicationInfo>();
                        apps.Add(new InstalledApplicationInfo("id1", "package1", "Application One"));
                        apps.Add(new InstalledApplicationInfo("id2", "package2", "Application Two"));
                        apps.Add(new InstalledApplicationInfo("id3", "package3", "Application Three"));
                        this.InstalledApplications = apps;
                    }
                }
                else
                {
                    try
                    {
                        await this.EnsureConnectionAsync();

                        var batteryState = await this.devicePortal.GetBatteryStateAsync();
                        this.BatteryStatus = (int)(batteryState.Level) + "%";
                        this.LowPower = (int)(batteryState.Level) <= AppSettings.LowPowerLevel;
                        this.Charged = (int)(batteryState.Level) >= AppSettings.ChargedPowerLevel;
                        this.MiddlePower = !this.LowPower && !this.Charged;
                        this.Charging = batteryState.IsCharging;

                        if(this.GetRunningProcessesOnHeartbeat)
                        {
                            this.RunningProcesses = await this.devicePortal.GetRunningProcessesAsync();
                        }

                        if (this.InstalledApplications == null)
                        {
                            var installedAppPackages = await this.devicePortal.GetInstalledAppPackagesAsync();
                            var apps = new List<InstalledApplicationInfo>();

                            foreach(var package in installedAppPackages.Packages)
                            {
                                /// PackageOrigin_Unknown            = 0,
                                /// PackageOrigin_Unsigned           = 1,
                                /// PackageOrigin_Inbox              = 2,
                                /// PackageOrigin_Store              = 3,
                                /// PackageOrigin_DeveloperUnsigned  = 4,
                                /// PackageOrigin_DeveloperSigned    = 5,
                                /// PackageOrigin_LineOfBusiness     = 6

                                if (package.PackageOrigin > 2)
                                {
                                    var info = new InstalledApplicationInfo(package.AppId, package.FullName, package.Name);
                                    apps.Add(info);
                                }
                            }
                            this.InstalledApplications = apps;

                            var kioskModeStatus = await this.devicePortal.GetKioskModeStatusAsync();
                            this.KioskModeAppId = kioskModeStatus.StartupAppPackageName;
                        }

                        this.lastHearbeatTime = DateTime.UtcNow;
                    }
                    catch(Exception)
                    {
                        this.LogMessage(true, "Heartbeat failed");
                        // immediately time us out
                        this.lastHearbeatTime = DateTime.UtcNow - (disconnectTimeout + TimeSpan.FromSeconds(1));

                        this.RunningProcesses = null;
                        this.BatteryStatus = string.Empty;
                        this.LowPower = false;
                        this.MiddlePower = false;
                        this.Charged = false;
                        this.Charging = false;

                        // Usually you are here because you can't connect
                        this.DeviceConnectionStatus = DeviceConnectionStatus.Failed;

                        // TODO: maybe forget all our connection stuff so we get a fresh start next time

                        
                    }
                }
            }

            this.Connected = (DateTime.UtcNow - disconnectTimeout) < this.lastHearbeatTime;
        }

        /// <summary>
        /// Makes sure there is a connection to the device.  If device can't be contacted
        /// it will throw.  Does not try to ping the device.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Note that we let exceptions leave this function.  We depend on caller for any retry</remarks>
        private async Task EnsureConnectionAsync()
        {
            // Make sure we're not trying multiple connections.
            // This logic assumes one thread.  There will be a race condition
            // with the isConnecting field if multiple threads are executing this.
            if (this.isConnecting)
            {
                // we're already trying to connect on another task.  Wait
                await WaitForCondition(TimeSpan.FromSeconds(2.0), new CancellationToken(), () => !this.isConnecting);
                // TODO: maybe make this timeout configurable
            }
            this.isConnecting = true;

            try
            {
                if (this.devicePortalConnection == null)
                {
                    var address = FixupMachineAddress(this.Address, false);

                    this.devicePortalConnection = new DefaultDevicePortalConnection(
                            address,
                            this.UserName,
                            this.Password);
                    this.devicePortal = new DevicePortal(this.devicePortalConnection);
                    this.devicePortal.UnvalidatedCert += (sender, certificate, chain, sslPolicyErrors) => true; // Allow unvalidated certs
                    this.devicePortal.ConnectionStatus += DevicePortalConnectionStatus;
                }

                if (this.DeviceConnectionStatus != DeviceConnectionStatus.Connected || this.DeviceConnectionStatus != DeviceConnectionStatus.Connecting)
                {
                    X509Certificate2 certificate = await this.devicePortal.GetRootDeviceCertificateAsync();

                    // Establish the connection to the device.
                    await this.devicePortal.ConnectAsync(
                        ssid: null,
                        ssidKey: null,
                        updateConnection: false,
                        manualCertificate: certificate);
                }
            }
            finally
            {
                this.isConnecting = false;
            }
        }

        private void DevicePortalConnectionStatus(DevicePortal sender, DeviceConnectionStatusEventArgs args)
        {
            // This can get called back on random threads.  Marshal it back to the UI thread if needed

            if(MainThread.IsMainThread)
            {
                this.DeviceConnectionStatus = args.Status;
            }
            else
            {
                // Not clear if this is the right thing to do in the Xamarin world using TPL
                MainThread.BeginInvokeOnMainThread(() => { this.DevicePortalConnectionStatus(sender, args); });
            }
        }

        public async Task<uint> LaunchKioskModeApplicationAsync()
        {
            if(string.IsNullOrEmpty(this.KioskModeAppId))
            {
                this.LogMessage(true, $"LaunchKioskModeApplicationAsync failed - kiosk mode not set");
                return 0;
            }

            foreach (var app in this.installedApplications)
            {
                if(app.AppId == this.KioskModeAppId)
                {
                    return await this.LaunchApplicationAsync(app.AppId, app.PackageName);
                }
            }

            this.LogMessage(true, $"LaunchKioskModeApplicationAsync failed for package {this.KioskModeAppId}  - package not installed");
            return 0;
        }

        public async Task ExportMap()
        {
            await this.devicePortal.ExportMapManagerFileAsync();
        }

        public async Task<uint> LaunchApplicationAsync(string appId, string packageName)
        {
            using (new StatusHelper(this))
            {
                if (this.UseFakeBackend)
                {
                    this.AppLaunchStatusChanged(AppLaunchStatus.CommandSent);
                    await Task.Delay(200);
                    bool failed = (new Random()).Next() % 2 == 0;
                    this.AppLaunchStatusChanged?.Invoke(failed ? AppLaunchStatus.Failed : AppLaunchStatus.Succeeded);
                    this.LogMessage(failed, $"Fake app launch.  Failed:{failed}");
                    return 47;
                }
                else
                {
                    try
                    {
                        this.AppLaunchStatusChanged?.Invoke(AppLaunchStatus.CommandSent);
                        uint retval = await this.devicePortal.LaunchApplicationAsync(appId, packageName);
                        this.LogMessage(false, $"LaunchApplicationAsync succeeded for package {packageName}");
                        this.AppLaunchStatusChanged?.Invoke(AppLaunchStatus.Succeeded);
                        return retval;
                    }
                    catch (Exception e)
                    {
                        LogException($"LaunchApplicationAsync failed for package {packageName}", e);
                        this.AppLaunchStatusChanged?.Invoke(AppLaunchStatus.Failed);
                        return 0;
                    }
                }
            }
        }

        private void LogException(string message, Exception e)
        {
            var dpe = e as DevicePortalException;

            this.LogMessage(true, message);
            this.LogMessage(true, $"  Exception.Message: {e.Message}");
            if(dpe != null)
            {
                this.LogMessage(true, $"  DevicePortalException.Reason: {dpe.Reason}");
                this.LogMessage(true, $"  DevicePortalException.RequestUri: {dpe.RequestUri}");
                this.LogMessage(true, $"  DevicePortalException.HResult: 0x{dpe.HResult:X}");
            }
            this.LogMessage(true, $"  Exception.StackTrace:");
            this.LogMessage(true, e.StackTrace);
            if(e.InnerException != null)
            {
                this.LogMessage(true, $"  Inner Exception ${e.InnerException.Message}");
                this.LogMessage(true, $"  Inner Exception call stack: ${e.InnerException.StackTrace}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="forceKill"></param>
        /// <returns></returns>
        public async Task TerminateApplicationAsync(string packageName, bool forceKill)
        {
            using (new StatusHelper(this))
            {
                if (this.UseFakeBackend)
                {
                    await Task.Delay(2000);
                }
                else
                {
                    try
                    {
                        await this.devicePortal.TerminateApplicationAsync(packageName, forceKill);
                    }
                    catch(Exception e)
                    {
                        this.LogMessage(true, $"TerminateApplicationAsync failed for package {packageName} forceKill={forceKill}  Message={e.Message}");
                    }
                }
            }
        }

        public async Task TerminateProcessAsync(uint processId)
        {
            using (new StatusHelper(this))
            {
                if (this.UseFakeBackend)
                {
                    await Task.Delay(2000);
                }
                else
                {
                    try
                    {
                        await this.devicePortal.TerminateProcessAsync(processId);
                    }
                    catch (Exception e)
                    {
                        this.LogMessage(true, $"TerminateProcessAsync failed for pid {processId}    {e.Message}");
                    }
                }
            }
        }


        public async Task<ImageSource> TakeScreenshotAsync()
        {
            using (new StatusHelper(this))
            {
                if (this.UseFakeBackend)
                {
                    await Task.Delay(2000);
                    return null;
                }
                else
                {
                    try
                    {
                        await this.devicePortal.TakeMrcPhotoAsync(true, false); // don't include camera for perf and experience reasons
                        var mrcFileList = await this.devicePortal.GetMrcFileListAsync();
                        DevicePortal.MrcFileInformation mostRecentFileInfo = mrcFileList.Files[0];
                        foreach(DevicePortal.MrcFileInformation fileInfo in mrcFileList.Files)
                        {
                            if(mostRecentFileInfo.CreationTimeRaw < fileInfo.CreationTimeRaw)
                            {
                                mostRecentFileInfo = fileInfo;
                            }
                        }

                        byte[] fileData = await this.devicePortal.GetMrcFileDataAsync(mostRecentFileInfo.FileName);
                        await this.devicePortal.DeleteMrcFileAsync(mostRecentFileInfo.FileName);
                        
                        StreamImageSource imageSource = new StreamImageSource() { Stream = (async (c) =>
                        {
                            var memoryStream = new MemoryStream(fileData);
                            await Task.CompletedTask; // quiet compiler warning - probably should use some pragma or something
                            return memoryStream;
                        })};

                        return imageSource;
                    }
                    catch (Exception e)
                    {
                        this.LogMessage(true, $"TakeScreenshotAsync failed {e.Message}");

                        return null;
                    }
                }
            }
        }

        public async Task SendVkey(bool down, int code)
        {
            using (new StatusHelper(this))
            {
                if (this.UseFakeBackend)
                {
                    await Task.Delay(500);
                }
                else
                {
                    try
                    {
                        await this.devicePortal.SendVkey(down, code);
                    }
                    catch (Exception e)
                    {
                        this.LogMessage(true, $"SendVkey failed {e.Message}");
                    }
                }
            }
        }

        public async Task SendText(string text)
        {
            if (this.UseFakeBackend)
            {
                await Task.Delay(500);
            }
            else
            {
                try
                {
                    await this.devicePortal.SendText(text);
                }
                catch (Exception e)
                {
                    this.LogMessage(true, $"SendText failed {e.Message}");
                }
            }
        }

        public InstalledApplicationInfo TryFindPackage(string appId, string packageName)
        {
            foreach(var app in this.InstalledApplications)
            {
                if(app.AppId == appId && app.PackageName == packageName)
                {
                    return app;
                }
            }

            return null;
        }

        public async Task Close()
        {
            using (new StatusHelper(this))
            {
                this.Closed = true;
                await Task.Delay(0); // placeholder
            }
        }

        public async  Task LaunchDevicePortalAsync()
        {
            if (this.UseFakeBackend)
            {
                await Task.Delay(50);
            }
            else
            {
                try
                {
                    await Browser.OpenAsync(this.devicePortalConnection.Connection.AbsoluteUri);
                }
                catch(Exception e)
                {
                    this.LogMessage(true, $"LaunchDevicePortalAsync failed {e.Message}");
                }
            }
        }

        public async Task RebootAsync()
        {
            if (this.UseFakeBackend)
            {
                await Task.Delay(50);
            }
            else
            {
                try
                {
                    await this.devicePortal.RebootAsync();
                }
                catch (Exception e)
                {
                    this.LogMessage(true, $"RebootAsync failed {e.Message}");
                }
            }
        }

        private string FixupMachineAddress(string address, bool isWindowsDesktop)
        {
            address = address.ToLower();

            // Insert http if needed
            if (!address.StartsWith("http"))
            {
                string scheme = "https";

                address = string.Format(
                    "{0}://{1}",
                    scheme,
                    address);
            }

            if (isWindowsDesktop)
            {
                string s = address.Substring(address.IndexOf("//"));
                if (!s.Contains(":"))
                {
                    // Append the default Windows Device Portal port for Desktop PC connections.
                    address += ":50443";
                }
            }

            return address;
        }

        /// <summary>
        /// Helper function to wait for an expression to be true.
        /// </summary>
        /// <param name="timeout">Total time to wait before failing</param>
        /// <param name="cancellationToken">the CancellationToken</param>
        /// <param name="condition">condition to wait for</param>
        /// <returns></returns>
        private static async Task WaitForCondition(TimeSpan timeout, CancellationToken cancellationToken, Func<bool> condition)
        {
            DateTime giveUpTime = DateTime.UtcNow + timeout;
            while (!condition())
            {
                await Task.Delay(200, cancellationToken);
                if (DateTime.UtcNow >= giveUpTime)
                {
                    throw new TimeoutException("WaitForCondition - timed out");
                }
            }
        }

        private class StatusHelper : IDisposable
        {
            private ActiveDevice activeDevice;
            public StatusHelper(ActiveDevice device)
            {
                this.activeDevice = device;
                this.activeDevice.OperationInProgress = true;
            }

            public void Dispose()
            {
                this.activeDevice.OperationInProgress = false;
            }
        }

        private void LogMessage(bool isError, string message)
        {
            this.Log?.Invoke(isError, this.Id, message);
        }

        public override string ToString()
        {
            return this.Address;
        }


    }
}
