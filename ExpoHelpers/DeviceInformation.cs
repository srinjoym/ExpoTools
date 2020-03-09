using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ExpoHelpers
{
    public class DeviceInformation
    {
        public string Address { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public override bool Equals(object obj)
        {
            DeviceInformation other = obj as DeviceInformation;
            if (other == null) return false;
            return this.Address == other.Address;
        }

        public override int GetHashCode()
        {
            return (this.Address == null) ? 0 : this.Address.GetHashCode();
        }
    }
}
