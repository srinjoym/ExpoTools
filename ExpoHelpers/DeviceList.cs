using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ExpoHelpers
{
    /// <summary>
    /// Class to maintain a list of all the devices we will ever deal with.
    /// </summary>
    public class DeviceList : NotifyPropertyChangedBase
    {
        public delegate void LogEHandler(bool isError, string message);

        public event LogEHandler Log;

        private IList<DeviceInformation> deviceInfos;
        public IList<DeviceInformation> DeviceInfos { get { return this.deviceInfos; } private set { this.PropertyChangedHelper(ref deviceInfos, value); } }

        public void LoadDeviceList(Stream deviceListStream)
        {
            List<ConnectionInformation> connectionInfos = null;

            if(deviceListStream != null)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<ConnectionInformation>));
                    connectionInfos = serializer.Deserialize(deviceListStream) as List<ConnectionInformation>;
                }
                catch(InvalidOperationException e)
                {
                    this.Log?.Invoke(true, "Unable to read device list file\r\n" + e.InnerException.Message);
                    connectionInfos = null;
                }

                // Do some valididty checks on the list.  Make sure all addresses are unique and that none contain a |
                HashSet<string> allAddresses = new HashSet<string>();
                foreach (var connection in connectionInfos)
                {
                    if (connection.Address.Contains('|'))
                    {
                        this.Log?.Invoke(true, $"Invalid data in device list file.  Device {connection.Address} contains reserved character |");
                        connectionInfos = null;
                        break;
                    }

                    var normalizedAddress = connection.Address.ToLowerInvariant();
                    if (allAddresses.Contains(normalizedAddress))
                    {
                        this.Log?.Invoke(true, $"DeviceList.LoadDeviceList - duplicate address {connection.Address}");
                        connectionInfos = null;
                        break;
                    }
                    allAddresses.Add(normalizedAddress);
                }
            }

            if (connectionInfos == null)
            {
                connectionInfos = new List<ConnectionInformation>();
            }

            var newDeviceList = new List<DeviceInformation>(connectionInfos.Count);
            foreach(var connection in connectionInfos)
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

            this.DeviceInfos = newDeviceList;

            this.Log?.Invoke(false, "Successfully loaded device list");
        }

        public DeviceInformation TryFind(string address)
        {
            foreach(var info in this.DeviceInfos)
            {
                if(info.Address == address)
                {
                    return info;
                }
            }
            return null;
        }

        public void ReplaceDeviceList(IList<DeviceInformation> devices)
        {
            // Do a deep copy to our list.  Callers will have their
            // own deep copy.
            var newDeviceInfos = new List<DeviceInformation>(devices.Count);
            foreach(var info in devices)
            {
                var newDeviceInfo = new DeviceInformation();
                newDeviceInfo.UpdateFrom(info);
                newDeviceInfos.Add(info);
            }
            this.DeviceInfos = newDeviceInfos;
        }

        /// <summary>
        /// Helper for UI that wants to display and edit a copy of the DeviceList.
        /// Makes a deep copy so changes in the UI don't change the main DeviceList
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<DeviceInformation> CloneDeviceList()
        {
            var retval = new ObservableCollection<DeviceInformation>();
            foreach(var info in this.DeviceInfos)
            {
                var newInfo = new DeviceInformation();
                newInfo.UpdateFrom(info);
                retval.Add(newInfo);
            }
            return retval;
        }

    }
}
