using System;
using System.ComponentModel;

namespace ExpoHelpers
{
    public class DeviceCheckListItemViewModel : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;
        private bool PropertyChangedHelper<T>(ref T storage, T newValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (IEquatable<T>.Equals(newValue, storage))
            {
                return false;
            }
            storage = newValue;

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            return true;
        }
    }
}
