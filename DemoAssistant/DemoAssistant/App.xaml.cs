using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DemoAssistant.Services;
using DemoAssistant.Views;
using ExpoHelpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DemoAssistant
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            // Register all the singletons that are
            // not specific to a platform
            DependencyService.Register<DeviceList>();
            DependencyService.Register<ActiveDeviceList>();
            DependencyService.Register<LoggingService>();
            DependencyService.Register<OptionalButtons>();

            // Wire-up all the notifications that go between the singletons.
            var deviceList = DependencyService.Get<DeviceList>();
            var activeDeviceList = DependencyService.Get<ActiveDeviceList>();
            var loggingService = DependencyService.Get<ILoggingService>();

            deviceList.PropertyChanged += async (o,args) =>
            {
                if(args.PropertyName == nameof(deviceList.DeviceInfos))
                {
                    await this.UpdateActiveDevicesAsync();
                }
            };

            deviceList.Log += loggingService.LogMessage;
            activeDeviceList.Log += loggingService.LogDeviceMessage;
            
            Task.Run(this.InitAsync);

            this.MainPage = new MainPage();
        }

        private async Task InitAsync()
        {
            var deviceList = DependencyService.Get<DeviceList>();

            var deviceListString = AppSettings.DeviceListString;
            if(!string.IsNullOrEmpty(deviceListString))
            {
                // Try loading the device list from settings
                deviceList.LoadDeviceListFromString(deviceListString);
            }

            if(deviceList.DeviceInfos.Count == 0)
            {
                // Couldn't load the device list from settings so try the
                // hard-coded list
                var testDevicesStream = await DependencyService.Get<IDeviceListStorage>().GetDeviceListStreamAsync();
                deviceList.LoadDeviceListFromStream(testDevicesStream);
            }

            // Update the selected devices from settings.  We don't just have this
            // in the DeviceList XML to keep it compatable with other apps that use it.
            // May not be important...
            deviceList.UpdateCheckedDevices(AppSettings.SelectedDevices);

            await this.UpdateActiveDevicesAsync();
        }

        /// <summary>
        /// Updates the ActiveDevices instance with devices from the
        /// DeviceList instance filtered by what devices the user has selected
        /// </summary>
        /// <returns></returns>
        private async Task UpdateActiveDevicesAsync()
        {
            var deviceList = DependencyService.Get<DeviceList>();
            var activeDeviceList = DependencyService.Get<ActiveDeviceList>();

            // Temp code to always have one device available so the menu is there.
            // Seems like a Xamarin Forms bug
            if (deviceList.GetCheckedDevicesCount() == 0 && deviceList.DeviceInfos.Count > 0)
            {
                deviceList.DeviceInfos[0].IsChecked = true;
            }

            await activeDeviceList.UpdateActiveDevicesAsync(false);
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
