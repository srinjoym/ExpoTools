using DemoAssistant.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DemoAssistant.Views
{
    public partial class SelectOptionalButtonsPage : ContentPage
    {
        public Command MoveUpCommand { get; }

        public Command MoveDownCommand { get; }

        public ObservableCollection<OptionalButtonInfo> CheckListItems { get; }

        public SelectOptionalButtonsPage(ObservableCollection<OptionalButtonInfo> optionalButtons)
        {
            this.CheckListItems = optionalButtons;

            this.MoveUpCommand = new Command((param) =>
            {
                var info = (OptionalButtonInfo)param;
                int index = this.CheckListItems.IndexOf(info);
                if(index > 0)
                {
                    this.CheckListItems.Move(index, index - 1);
                }
            });

            this.MoveDownCommand = new Command((param) =>
            {
                var info = (OptionalButtonInfo)param;
                int index = this.CheckListItems.IndexOf(info);
                if (index >= 0 && index < this.CheckListItems.Count - 2)
                {
                    this.CheckListItems.Move(index, index + 1);
                }
            });

            InitializeComponent();
            this.BindingContext = this;
        }


    }
}
