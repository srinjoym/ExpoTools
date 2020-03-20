using ExpoHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace DemoAssistant.Services
{
    public class OptionalButtonInfo : INotifyPropertyChanged
    {
        private bool isChecked = false;
        public bool IsChecked
        {
            set { this.PropertyChangedHelper(ref isChecked, value); }
            get { return this.isChecked; }
        }

        public string XamlName { get; private set; }
        public string Description { get; private set; }

        public OptionalButtonInfo(string xamlName, string description)
        {
            this.XamlName = xamlName;
            this.Description = description;
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

        public override string ToString()
        {
            return $"{(this.IsChecked ? "X" : "_")} {this.XamlName} \"{this.Description}\"";
        }
    }

    public class OptionalButtons
    {
        public List<OptionalButtonInfo> Buttons { get; private set; }

        public OptionalButtons()
        {
            this.Buttons = new List<OptionalButtonInfo>()
            {
                new OptionalButtonInfo("ScreenShotButton"   , "Screen Shot"),
                new OptionalButtonInfo("LeftArrowButton"    , "Left Arrow"),
                new OptionalButtonInfo("UpArrowButton"      , "Up Arrow"),
                new OptionalButtonInfo("DownArrowButton"    , "Down Arrow"),
                new OptionalButtonInfo("RightArrowButton"   , "Right Arrow"),
                new OptionalButtonInfo("TabKeyButton"       , "Tab Key"),
                new OptionalButtonInfo("CKeyButton"         , "C Key"),
                new OptionalButtonInfo("DKeyButton"         , "D Key"),
                new OptionalButtonInfo("GKeyButton"         , "G Key"),
                new OptionalButtonInfo("NKeyButton"         , "N Key"),
                new OptionalButtonInfo("PKeyButton"         , "P Key"),
                new OptionalButtonInfo("RKeyButton"         , "R Key"),
                new OptionalButtonInfo("StopAppAppsButton"  , "Stop All Apps"),
                new OptionalButtonInfo("RestartDeviceButton", "Restart Device"),
                new OptionalButtonInfo("DevicePortalButton" , "Device Portal"),
            };

            this.UpdateFromSettingsString(AppSettings.EnabledButtons);
        }

        /// <summary>
        /// Creates a deep-copy of the button infos.  The returned
        /// list is meant to be used in some UI where we don't want
        /// changes to take immediate effect
        /// </summary>
        /// <returns></returns>
        public IList<OptionalButtonInfo> CloneButtonsList()
        {
            var retval = new List<OptionalButtonInfo>(this.Buttons.Count);
            foreach(var info in this.Buttons)
            {
                retval.Add(new OptionalButtonInfo(info.XamlName, info.Description) { IsChecked = info.IsChecked });
            }
            return retval;
        }

        public void Update(IList<OptionalButtonInfo> list)
        {
            // Somple verification.  We could go deeper with checking the actual members
            if(list.Count != this.Buttons.Count)
            {
                throw new InvalidOperationException("OptionalButtons.Update - replacement list must be the same members as the old list");
            }

            // Do a deep copy.  May not be necessary but just in case
            // the caller re-uses OptionalButtonInfos in the list they handed us.
            this.Buttons = new List<OptionalButtonInfo>(list.Count);
            foreach(var info in list)
            {
                this.Buttons.Add(new OptionalButtonInfo(info.XamlName, info.Description) { IsChecked = info.IsChecked });
            }
        }

        /// <summary>
        /// Configures a Layout that already contains controls for the optional
        /// buttons to match the state of the buttons.  Both in visibility
        /// and in order of the buttons
        /// </summary>
        /// <param name="layout"></param>
        public void ApplyTo(FlexLayout layout)
        {
            foreach (var button in this.Buttons)
            {
                var child = layout.FindByName(button.XamlName) as View;
                if (child != null)
                {
                    child.IsVisible = button.IsChecked;
                    if (button.IsChecked)
                    {
                        layout.Children.Remove(child);
                        layout.Children.Add(child);
                    }
                }
            }

        }

        public string GetSettingsString()
        {
            var sb = new StringBuilder();
            foreach (var button in this.Buttons)
            {
                if (button.IsChecked)
                {
                    sb.Append(button.XamlName);
                    sb.Append(",");
                }
            }

            // Remove the trailing comma
            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        public void UpdateFromSettingsString(string settingsString)
        {
            var oldList = new List<OptionalButtonInfo>(this.Buttons);
            List<OptionalButtonInfo> newList = new List<OptionalButtonInfo>(this.Buttons.Count);

            var settingsNames = settingsString.Split(',');

            // Create new entries in the list for items specified in settings
            // in the order they appear
            foreach (var name in settingsNames)
            {
                var button = oldList.Find((b) => b.XamlName == name);
                if (button != null)
                {
                    newList.Add(button);
                    button.IsChecked = true;
                    oldList.Remove(button);
                }
            }

            // Copy over any remaining items as unchecked
            foreach(var button in oldList)
            {
                newList.Add(button);
                button.IsChecked = false;
            }

            this.Buttons = newList;
        }

    }
}
