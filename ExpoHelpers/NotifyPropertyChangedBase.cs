using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ExpoHelpers
{
    /// <summary>
    /// Helper class that implements INotifyPropertyChanged for your class
    /// Breaks the "favor composition over inheritance" guildeline but prevents
    /// a bunch of copy+paste code.
    /// </summary>
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool PropertyChangedHelper<T>(ref T storage, T newValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (IEquatable<T>.Equals(newValue, storage))
            {
                return false;
            }
            storage = newValue;

            this.SendPropertyChanged(propertyName);

            return true;
        }

        protected void SendPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
