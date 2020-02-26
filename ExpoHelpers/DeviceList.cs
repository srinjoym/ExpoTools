using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ExpoHelpers
{
    public enum DeviceListStatus
    {
        ListLoaded,
        UnableToReadDeviceListFile,
    }

    public class DeviceListStatusArgs : EventArgs
    {
        public DeviceListStatusArgs(DeviceListStatus status, string message)
        {
            this.DeviceListStatus = status;
            this.Message = message;
        }

        public DeviceListStatus DeviceListStatus { get; private set; }
        public string Message { get; private set; }
    }

    public delegate void DeviceListStatusDelegate(DeviceList sender, DeviceListStatusArgs args);


    /// <summary>
    /// Class to maintain a list of all the devices we will ever deal with.
    /// Also keeps "check lists" of devices for what the user selects in
    /// the UI.  A purist might put that functionality in a different class
    /// </summary>
    public class DeviceList
    {
        public event DeviceListStatusDelegate StatusUpdate;

        public IList<DeviceInformation> Devices { get; private set; }

        private int deviceCheckListCount = 0;
        public int DeviceCheckListCount
        {
            get { return this.deviceCheckListCount; }
            set
            {
                if(this.deviceCheckListCount != value)
                {
                    this.deviceCheckListCount = value;
                    this.CreateDeviceCheckLists();
                }
            }
        }

        private List<List<DeviceCheckListItemViewModel>> deviceCheckLists;

        public void LoadDeviceList(Stream deviceListStream)
        {
            List<ConnectionInformation> connections = null;

            if(deviceListStream != null)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<ConnectionInformation>));
                    connections = serializer.Deserialize(deviceListStream) as List<ConnectionInformation>;
                }
                catch(InvalidOperationException e)
                {
                    OnStatusUpdate(DeviceListStatus.UnableToReadDeviceListFile, "Unable to read device list file\r\n" + e.InnerException.Message);
                    connections = null;
                }

                // Do some valididty checks on the list.  Make sure all addresses are unique and that none contain a |
                HashSet<string> allAddresses = new HashSet<string>();
                foreach (var connection in connections)
                {
                    if (connection.Address.Contains('|'))
                    {
                        OnStatusUpdate(DeviceListStatus.UnableToReadDeviceListFile, $"Invalid data in device list file.  Device {connection.Address} contains reserved character |");
                        connections = null;
                        break;
                    }

                    var normalizedAddress = connection.Address.ToLowerInvariant();
                    if (allAddresses.Contains(normalizedAddress))
                    {
                        OnStatusUpdate(DeviceListStatus.UnableToReadDeviceListFile, $"DeviceList.LoadDeviceList - duplicate address {connection.Address}");
                        connections = null;
                        break;
                    }
                    allAddresses.Add(normalizedAddress);
                }
            }

            if (connections == null)
            {
                connections = new List<ConnectionInformation>();
            }

            var newDeviceList = new List<DeviceInformation>(connections.Count);
            foreach(var connection in connections)
            {
                newDeviceList.Add(new DeviceInformation()
                {
                    Name = connection.Name,
                    Address = connection.Address,
                    Id = connection.Id,
                    UserName = connection.UserName,
                    Password = connection.Password,
                });
            }

            this.Devices = newDeviceList;
            this.CreateDeviceCheckLists();
            OnStatusUpdate(DeviceListStatus.ListLoaded);
        }

        private void OnStatusUpdate(DeviceListStatus status, string message = null)
        {
            this.StatusUpdate?.Invoke(this, new DeviceListStatusArgs(status, message ?? string.Empty));
        }

        private void CreateDeviceCheckLists()
        {
            if(this.Devices == null)
            {
                // Haven't loaded things yet.
                // This will get called again after we load a device list.
                return;
            }

            this.deviceCheckLists = new List<List<DeviceCheckListItemViewModel>>(this.deviceCheckListCount);

            for (int deviceListIndex = 0; deviceListIndex < this.deviceCheckListCount; deviceListIndex++)
            {
                var checkList = new List<DeviceCheckListItemViewModel>(this.Devices.Count);
                foreach (var device in this.Devices)
                {
                    checkList.Add(new DeviceCheckListItemViewModel(device));
                }
                this.deviceCheckLists.Add(checkList);
            }
        }

        public void UpdateCheckListItems(int checkListIndex, string[] selectedDevices)
        {
            var checkList = this.deviceCheckLists[checkListIndex];

            foreach(var item in checkList)
            {
                item.IsChecked = selectedDevices.Contains(item.Device.Address);
            }
        }

        public IList<DeviceCheckListItemViewModel> GetDeviceCheckList(int deviceCheckListIndex)
        {
            return this.deviceCheckLists[deviceCheckListIndex];
        }

        public string[] GetCheckedItems(int deviceCheckListIndex)
        {
            var checkList = this.deviceCheckLists[deviceCheckListIndex];

            int checkedItemsCount = 0;
            foreach(var item in checkList)
            {
                checkedItemsCount += item.IsChecked ? 1 : 0;
            }

            var retval = new string[checkedItemsCount];
            foreach(var item in checkList)
            {
                if (item.IsChecked)
                {
                    retval[--checkedItemsCount] = item.Device.Address;
                }
            }
            return retval;
        }

        public DeviceInformation TryFind(string address)
        {
            foreach(var device in this.Devices)
            {
                if(device.Address == address)
                {
                    return device;
                }
            }
            return null;
        }
    }
}
