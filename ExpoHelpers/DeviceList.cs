using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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

        private IList<DeviceInformation> deviceInfos = new List<DeviceInformation>();
        public IList<DeviceInformation> DeviceInfos { get { return this.deviceInfos; } private set { this.PropertyChangedHelper(ref deviceInfos, value); } }

        public void LoadDeviceListFromString(string deviceListXml)
        {
            if (string.IsNullOrEmpty(deviceListXml))
            {
                this.Log?.Invoke(true, "Empty device list string");
                return;
            }

            List<ConnectionInformation> connectionInfos = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<ConnectionInformation>));
                connectionInfos = serializer.Deserialize(new StringReader(deviceListXml)) as List<ConnectionInformation>;
            }
            catch(Exception e)
            {
                if (e is InvalidOperationException)
                {
                    this.Log?.Invoke(true, "Unable to read device list string\r\n" + e.InnerException.Message);
                    return;
                }
                else if (e is FormatException)
                {
                    this.Log?.Invoke(true, "Unable to read device list string\r\n" + e.Message);
                    return;
                }
                throw;
            }

            this.TrySetDeviceList(connectionInfos);
        }

        public void LoadDeviceListFromStream(Stream deviceListStream)
        {
            List<ConnectionInformation> connectionInfos = null;

            if(deviceListStream == null)
            {
                this.Log?.Invoke(true, "Empty device list stream");
                return;
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<ConnectionInformation>));
                connectionInfos = serializer.Deserialize(deviceListStream) as List<ConnectionInformation>;
            }
            catch(InvalidOperationException e)
            {
                this.Log?.Invoke(true, "Unable to read device list file\r\n" + e.InnerException.Message);
                return;
            }

            this.TrySetDeviceList(connectionInfos);
        }

        private bool TrySetDeviceList(List<ConnectionInformation> connectionInfos)
        {
            // Do some valididty checks on the list
            HashSet<string> allAddresses = new HashSet<string>();
            foreach (var connection in connectionInfos)
            {
                var normalizedAddress = connection.Address.ToLowerInvariant();
                if (allAddresses.Contains(normalizedAddress))
                {
                    this.Log?.Invoke(true, $"DeviceList.LoadDeviceList - duplicate address {connection.Address}");
                    return false;
                }
                allAddresses.Add(normalizedAddress);
            }

            var newDeviceList = new List<DeviceInformation>(connectionInfos.Count);
            foreach (var connection in connectionInfos)
            {
                newDeviceList.Add(new DeviceInformation()
                {
                    Name = connection.Name,
                    Address = connection.Address,
                    Id = connection.Id,
                    UserName = connection.UserName,
                    Password = connection.Password,
                    IsChecked = connection.IsChecked,
                });
            }

            this.DeviceInfos = newDeviceList;
            this.Log?.Invoke(false, "Successfully loaded device list");
            return true;
        }

        public string GetDeviceListString()
        {
            // Create a list of ConnectionInfo classes to serialize out
            var connectionInfos = new List<ConnectionInformation>(this.deviceInfos.Count);
            foreach (var deviceInfo in this.DeviceInfos)
            {
                var connectionInfo = new ConnectionInformation()
                {
                    Address = deviceInfo.Address,
                    Id = deviceInfo.Id,
                    Name = deviceInfo.Name,
                    Password = deviceInfo.Password,
                    UserName = deviceInfo.UserName,
                    IsChecked = deviceInfo.IsChecked,
                };
                connectionInfos.Add(connectionInfo);
            }

            var stringWriter = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(List<ConnectionInformation>));
            serializer.Serialize(stringWriter, connectionInfos);
            return stringWriter.ToString();
        }

        public int GetCheckedDevicesCount()
        {
            return this.DeviceInfos.Count((info) => info.IsChecked);
        }

        public DeviceInformation[] GetCheckedDevices()
        {
            var checkedDevices = new List<DeviceInformation>();
            foreach (var item in this.DeviceInfos)
            {
                if (item.IsChecked)
                {
                    checkedDevices.Add(item);
                }
            }
            return checkedDevices.ToArray();
        }

        public bool IsDeviceChecked(string address)
        {
            foreach (var item in this.DeviceInfos)
            {
                if (item.Address == address)
                {
                    return item.IsChecked;
                }
            }
            return false;
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

        public void ReplaceDeviceList(IList<DeviceInformation> devices)
        {
            // Do a deep copy to our list.  Callers will have their
            // own deep copy.
            var newDeviceInfos = new List<DeviceInformation>(devices.Count);
            foreach (var info in devices)
            {
                var newDeviceInfo = new DeviceInformation();
                newDeviceInfo.UpdateFrom(info);
                newDeviceInfos.Add(info);
            }
            this.DeviceInfos = newDeviceInfos;
        }


    }
}
