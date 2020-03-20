using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DemoAssistant.Services;
using DemoAssistant.Views;
using ExpoHelpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DemoAssistant
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<DeviceList>();
            DependencyService.Register<ActiveDeviceList>();
            DependencyService.Register<LoggingService>();
            DependencyService.Register<OptionalButtons>();

            MainPage = new MainPage();

            DependencyService.Get<ActiveDeviceList>().Log += DependencyService.Get<ILoggingService>().LogDeviceMessage;
            
            Task.Run(this.InitAsync);
        }

        private async Task InitAsync()
        {
            var deviceList = DependencyService.Get<DeviceList>();
            var activeDeviceList = DependencyService.Get<ActiveDeviceList>();

            var testDevicesStream = await DependencyService.Get<IDeviceListStorage>().GetDeviceListStreamAsync();
            deviceList.LoadDeviceList(testDevicesStream);

            var deviceCheckList = new DeviceCheckList();
            deviceCheckList.Reset(deviceList.Devices);
            deviceCheckList.UpdateFromString(AppSettings.SelectedDevices);

            await activeDeviceList.UpdateActiveDevicesAsync(false, deviceCheckList.GetCheckedDevices());
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
