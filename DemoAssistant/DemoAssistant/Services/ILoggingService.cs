using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DemoAssistant.Services
{
    interface ILoggingService
    {
        void LogMessage(bool isError, string message);
        void LogDeviceMessage(bool isError, string deviceId, string message);
        void Clear();
        string[] GetLogTail(int requestedEntryCount);
        string GetLogString();
        void AddLogSpans(FormattedString formattedString, int requestedEntryCount);
        event EventHandler LogChanged;
    }
}
