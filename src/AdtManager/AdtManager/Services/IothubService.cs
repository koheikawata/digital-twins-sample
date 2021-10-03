using AdtManager.Interfaces;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdtManager.Services
{
    public class IothubService : IIothubService
    {
        private readonly RegistryManager registryManager;

        public IothubService(RegistryManager registryManager)
        {
            this.registryManager = registryManager;
        }

        public async Task<Device> RegisterDeviceAsync(string deviceId)
        {
            return await this.registryManager.AddDeviceAsync(new Device(deviceId)).ConfigureAwait(false);
        }

        public async Task<Twin> GetTwinAsync(string deviceId)
        {
            return await this.registryManager.GetTwinAsync(deviceId).ConfigureAwait(false);
        }

        public async Task UpdateTwinAsync(string deviceId, Twin twinPatch, string twinEtag)
        {
            await this.registryManager.UpdateTwinAsync(deviceId, twinPatch, twinEtag);
        }
    }
}
