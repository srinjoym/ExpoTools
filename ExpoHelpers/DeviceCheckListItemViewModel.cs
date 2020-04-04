using System;
using System.ComponentModel;

namespace ExpoHelpers
{
    public class DeviceCheckListItemViewModel : NotifyPropertyChangedBase
    {
        private bool isChecked = false;
        public bool IsChecked
        {
            set { this.PropertyChangedHelper(ref isChecked, value); }
            get { return this.isChecked; }
        }

        public string DisplayName { get { return $"{this.Device.Id}  -  {this.Device.Name}"; } }

        public DeviceInformation Device { get; private set; }

        public DeviceCheckListItemViewModel(DeviceInformation device)
        {
            this.Device = device;
        }
    }
}
