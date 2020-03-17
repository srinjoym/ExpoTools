using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace DemoAssistant.Services
{
    class LoggingService : ILoggingService
    {
        private class LogEntry
        {
            public DateTime Time { get; private set; }

            public bool IsError { get; private set; }

            public string DeviceId { get; private set; }

            public string Message { get; private set; }

            public LogEntry(bool isError, string deviceId, string message)
            {
                this.Time = DateTime.UtcNow;
                this.IsError = isError;
                this.DeviceId = deviceId;
                this.Message = message;
            }

            public override string ToString()
            {
                string error = this.IsError ? "Error " : string.Empty;
                string deviceId = string.IsNullOrEmpty(this.DeviceId) ? string.Empty : $"Device:{this.DeviceId} ";
                return error + deviceId + this.Message;
            }
        }


        private List<LogEntry> entries = new List<LogEntry>();

        private object logLock = new object();

        public event EventHandler LogChanged;

        public void LogMessage(bool isError, string message)
        {
            this.LogMessageInternal(isError, null, message);
        }

        public void LogDeviceMessage(bool isError, string deviceId, string message)
        {
            this.LogMessageInternal(isError, deviceId, message);
        }

        private void LogMessageInternal(bool isError, string deviceId, string message)
        {
            var newEntry = new LogEntry(isError, deviceId, message);
            lock (this.logLock)
            {
                this.entries.Add(newEntry);
            }
            Debug.WriteLine(newEntry);
            this.OnLogChanged();
        }

        public void Clear()
        {
            lock(this.logLock)
            {
                this.entries.Clear();
            }
            this.OnLogChanged();
        }

        public string[] GetLogTail(int requestedEntryCount)
        {
            lock(this.logLock)
            {
                int actualEntryCount = Math.Min(requestedEntryCount, this.entries.Count);
                var entryStrings = new string[actualEntryCount];
                for(int index = 0; index < actualEntryCount; ++index)
                {
                    entryStrings[index] = this.entries[this.entries.Count - actualEntryCount + index].ToString();
                }
                return entryStrings;
            }
        }

        public string GetLogString()
        {
            var sb = new StringBuilder();

            foreach(var entry in this.entries)
            {
                sb.Append(entry.Time.ToString("HH:mm:ss"));
                sb.Append(Environment.NewLine);
                sb.Append(entry.ToString());
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        public void AddLogSpans(FormattedString formattedString, int requestedEntryCount)
        {
            lock (this.logLock)
            {
                int actualEntryCount = Math.Min(requestedEntryCount, this.entries.Count);

                if (requestedEntryCount < this.entries.Count)
                {
                    formattedString.Spans.Add(new Span() { Text = "..." + Environment.NewLine, TextColor = Color.Red });
                }

                for (int index = 0; index < actualEntryCount; ++index)
                {
                    var entry = this.entries[this.entries.Count - actualEntryCount + index];
                    formattedString.Spans.Add(new Span()
                    {
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 9,
                        Text = entry.Time.ToString("HH:mm:ss") + Environment.NewLine
                    });
                    formattedString.Spans.Add(new Span()
                    {
                        FontAttributes = FontAttributes.None,
                        FontSize = 20,
                        ForegroundColor = entry.IsError ? Color.Red : Color.Black,
                        Text = entry.Message + Environment.NewLine
                    });
                }
            }
        }


        private void OnLogChanged()
        {
            this.LogChanged?.Invoke(this, null);
        }
    }
}
