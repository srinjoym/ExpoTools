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
    /// </summary>
    public class DeviceList
    {
        public event DeviceListStatusDelegate StatusUpdate;

        public IList<DeviceInformation> Devices { get; private set; }


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
            OnStatusUpdate(DeviceListStatus.ListLoaded);
        }

        private void OnStatusUpdate(DeviceListStatus status, string message = null)
        {
            this.StatusUpdate?.Invoke(this, new DeviceListStatusArgs(status, message ?? string.Empty));
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
