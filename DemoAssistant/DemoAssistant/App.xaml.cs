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

            MainPage = new MainPage();
            
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

            // Temp code to be sure we have at least two devices - otherwise the settings menu button will be hidden 
            if(deviceCheckList.Items.Count > 1 && deviceCheckList.GetCheckedDeviceCount() == 0)
            {
                deviceCheckList.Items[0].IsChecked = true;
                deviceCheckList.Items[1].IsChecked = true;
            }

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
