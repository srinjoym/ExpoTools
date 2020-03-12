using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DemoAssistant.Views
{
    public partial class ImageViewer : ContentPage
    {
        public ImageSource ImageSource { get; private set; }

        public ImageViewer(ImageSource source)
        {
            InitializeComponent();

            this.ImageSource = source;
            this.BindingContext = this;
        }
    }
}