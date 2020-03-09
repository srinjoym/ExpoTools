using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DemoAssistant.Services
{
    public interface IDeviceListStorage
    {
        Task<Stream> GetDeviceListStreamAsync();
    }
}
