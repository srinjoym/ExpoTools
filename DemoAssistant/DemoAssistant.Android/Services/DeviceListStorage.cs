using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DemoAssistant.Services;
using Xamarin.Forms;

namespace DemoAssistant.Droid
{
    /// <summary>
    /// Class to get the list of devices.  Will get re-done since
    /// the list can't be part of the package for regular use.
    /// </summary>
    class DeviceListStorage : IDeviceListStorage
    {
        public static Context Context { get; set; }

        public async Task<System.IO.Stream> GetDeviceListStreamAsync()
        {
            await Task.CompletedTask; // quiet the compiler warning.  Maybe this doesn't need to be async?

            AssetManager assets = DeviceListStorage.Context.Assets;
            Stream stream = assets.Open("Test Devices.xml");
            return stream;
        }
    }
}