using ExpoHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DemoAssistant.Views
{
    public partial class SelectActiveDevicesPage : ContentPage
    {
        public IList<DeviceCheckListItemViewModel> CheckListItems { get; }

        public SelectActiveDevicesPage(IList<DeviceCheckListItemViewModel> checkListItems)
        {
            this.CheckListItems = checkListItems;

            InitializeComponent();
            this.BindingContext = this;
        }
    }
}
