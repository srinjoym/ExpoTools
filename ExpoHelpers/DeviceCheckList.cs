using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ExpoHelpers
{
    public class DeviceCheckList
    {
        private List<DeviceCheckListItemViewModel> checkListItems;

        public const char DevicesSeperator = '|';

        public List<DeviceCheckListItemViewModel> Items { get { return this.checkListItems; } }

        public event EventHandler CheckChanged;

        public void Reset(IList<DeviceInformation> devices)
        {
            int count = devices.Count;
            this.checkListItems = new List<DeviceCheckListItemViewModel>(count);
            foreach (var device in devices)
            {
                var newItem = new DeviceCheckListItemViewModel(device);
                newItem.PropertyChanged += CheckListItemPropertyChanged;
                this.checkListItems.Add(newItem);
            }
        }

        private void CheckListItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeviceCheckListItemViewModel.IsChecked))
            {
                this.CheckChanged?.Invoke(this, null);
            }
        }

        public void UpdateFromString(string settingsString)
        {
            if (!string.IsNullOrEmpty(settingsString))
            {
                var addresses = settingsString.Split(DevicesSeperator);
                foreach(var item in this.checkListItems)
                {
                    string address = addresses.FirstOrDefault((s) => (item.Device.Address == s));
                    item.IsChecked = (address != default(string));
                }
            }
        }

        public string GetSettingsString()
        {
            var sb = new StringBuilder();
            foreach(var item in this.checkListItems)
            {
                if (item.IsChecked)
                {
                    sb.Append(item.Device.Address);
                    sb.Append(DevicesSeperator);
                }
            }

            if(sb.Length > 0)
            {
                // remove the extra seperator
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        public DeviceInformation[] GetCheckedDevices()
        {
            int checkedDeviceCount = this.GetCheckedDeviceCount();
            var retval = new DeviceInformation[checkedDeviceCount];
            int index = 0;
            foreach(var item in this.checkListItems)
            {
                if(item.IsChecked)
                {
                    retval[index++] = item.Device;
                }
            }
            return retval;
        }

        public int GetCheckedDeviceCount()
        {
            return this.checkListItems.Count((d) => d.IsChecked);
        }

    }
}
