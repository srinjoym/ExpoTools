using ExpoHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DemoAssistant.Views
{

    public partial class DeviceInformationEditor : ContentPage
    {

        private Action<DeviceInformation> saveClicked;

        public DeviceInformationEditor(DeviceInformation info, Action<DeviceInformation> saveClickedParam)
        {
            InitializeComponent();
            this.BindingContext = info;
            this.saveClicked = saveClickedParam;
        }

        protected override bool OnBackButtonPressed()
        {
            return true; // stop back navigation
        }

        public async void SaveClicked(object sender, EventArgs args)
        {
            this.saveClicked?.Invoke(this.BindingContext as DeviceInformation);
            await Navigation.PopModalAsync();
        }     
        
        public async void CancelClicked(object sender, EventArgs args)
        {
            await Navigation.PopModalAsync();
        }

    }
}