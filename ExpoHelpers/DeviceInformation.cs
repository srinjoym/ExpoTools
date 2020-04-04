using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ExpoHelpers
{
    public class DeviceInformation : NotifyPropertyChangedBase
    {
        private string address;
        public string Address { get { return this.address; } set { this.PropertyChangedHelper(ref this.address, value); } }

        private string name;
        public string Name { get { return this.name; } set { this.PropertyChangedHelper(ref this.name, value); } }

        private string id;
        public string Id { get { return this.id; } set { this.PropertyChangedHelper(ref this.id, value); } }

        private string userName;
        public string UserName { get { return this.userName; } set { this.PropertyChangedHelper(ref this.userName, value); } }

        private string password;
        public string Password { get { return this.password; } set { this.PropertyChangedHelper(ref this.password, value); } }


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

        public void UpdateFrom(DeviceInformation other)
        {
            this.Address = other.Address;
            this.Name = other.Name;
            this.Id = other.Id;
            this.UserName = other.UserName;
            this.Password = other.Password;
        }
    }
}
