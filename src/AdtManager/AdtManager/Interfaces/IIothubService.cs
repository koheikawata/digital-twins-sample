using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdtManager.Interfaces
{
    public interface IIothubService
    {
        public Task<Device> RegisterDeviceAsync(string deviceId);
        public Task<Twin> GetTwinAsync(string deviceId);
        public Task UpdateTwinAsync(string deviceId, Twin twinPatch, string twinEtag);
    }
}
