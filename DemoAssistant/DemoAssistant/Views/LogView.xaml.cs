using DemoAssistant.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DemoAssistant.Views
{
    public partial class LogView : ContentPage
    {
        ILoggingService loggingService;

        public LogView()
        {
            InitializeComponent();
            this.loggingService = DependencyService.Get<ILoggingService>();
            this.loggingService.LogChanged += LogChanged;
            Task.Run(this.UpdateLogText);
        }

        private async void LogChanged(object sender, EventArgs e)
        {
            // TODO: maybe add some timeout before actually updating
            await this.UpdateLogText();
        }

        private async Task UpdateLogText()
        {
            int lineCount = 20;

            var formattedString = new FormattedString();

            this.loggingService.AddLogSpans(formattedString, lineCount);

            this.LogText.FormattedText = formattedString;
            await this.LogScrollView.ScrollToAsync(this.LogText, ScrollToPosition.End, false);
        }

        public void ClearLogClick(object sender, EventArgs args)
        {
            this.loggingService.Clear();
        }

        public async void CopyLogClick(object sender, EventArgs args)
        {
            await Clipboard.SetTextAsync(this.loggingService.GetLogString());
        }

        public async void ShareLogClick(object sender, EventArgs args)
        {
            await Share.RequestAsync(new ShareTextRequest()
            {
                Text = this.loggingService.GetLogString(),
                Title = "Share Log"
            });
        }

        private void OnDisappearing(object sender, EventArgs e)
        {
            this.loggingService.LogChanged -= LogChanged;
        }
    }
}