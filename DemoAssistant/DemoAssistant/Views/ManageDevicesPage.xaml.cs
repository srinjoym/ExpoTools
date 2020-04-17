using ExpoHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DemoAssistant.Views
{
    public partial class ManageDevicesPage : ContentPage
    {
        public Command MoveUpCommand { get; }

        public Command MoveDownCommand { get; }

        public ObservableCollection<DeviceInformation> Devices { get; private set; }

        public ManageDevicesPage(ObservableCollection<DeviceInformation> devices)
        {
            // Work on a deep copy of the deviceList so cancel will undo all changes to
            // the list and the devices it contains
            this.Devices = devices;

            this.MoveUpCommand = new Command((param) =>
            {
                var info = (DeviceInformation)param;
                int index = this.Devices.IndexOf(info);
                if (index > 0)
                {
                    this.Devices.Move(index, index - 1);
                }
            });

            this.MoveDownCommand = new Command((param) =>
            {
                var info = (DeviceInformation)param;
                int index = this.Devices.IndexOf(info);
                if (index >= 0 && index < this.Devices.Count - 2)
                {
                    this.Devices.Move(index, index + 1);
                }
            });


            this.BindingContext = this;
            InitializeComponent();
        }

        private async void AddClicked(object sender, EventArgs e)
        {
            // Code we run if the user clicks save in the DeviceInformationEditor
            Action<DeviceInformation> onSave = (info) =>
            {
                info.IsChecked = true; // assume they want the new device visible
                this.Devices.Add(info);
                this.DeviceListView.ScrollTo(info, ScrollToPosition.MakeVisible, true);
            };

            await Navigation.PushModalAsync(new NavigationPage(new DeviceInformationEditor(new DeviceInformation(), onSave)));
        }

        private async void EditClicked(object sender, EventArgs e)
        {
            var selectedItem = this.DeviceListView.SelectedItem as DeviceInformation;
            if (selectedItem != null)
            {
                // Code we run if the user clicks save in the DeviceInformationEditor
                Action<DeviceInformation> onSave = (info) =>
                {
                    selectedItem.UpdateFrom(info);
                };

                // Edit a copy so if the user cancels the editor
                // we haven't changed anything in the selected item
                var scratchDeviceInfo = new DeviceInformation();
                scratchDeviceInfo.UpdateFrom(selectedItem);

                await Navigation.PushModalAsync(new NavigationPage(new DeviceInformationEditor(scratchDeviceInfo, onSave)));
            }
        }

        private void DeleteClicked(object sender, EventArgs e)
        {
            var selectedItem = this.DeviceListView.SelectedItem as DeviceInformation;
            if(selectedItem != null)
            {
                this.Devices.Remove(selectedItem);
            }
        }

        private void ScanQrClicked(object sender, EventArgs e)
        {
            // TODO - should be fun!
        }
    }
}