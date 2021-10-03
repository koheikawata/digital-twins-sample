using AdtManager.Interfaces;
using AdtManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdtManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IotDeviceController : ControllerBase
    {
        private readonly IIothubService iothubService;
        private readonly IConfiguration configuration;

        public IotDeviceController(IIothubService iothubService, IConfiguration configuration)
        {
            this.iothubService = iothubService;
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] IotDevice iotDevice)
        {
            Device createdDevice = await this.iothubService.RegisterDeviceAsync(iotDevice.DeviceId).ConfigureAwait(false);
            IotHubConnectionStringBuilder iotHubConnectionStringBuilder = IotHubConnectionStringBuilder.Create(this.configuration.GetValue<string>("IotHub:IotHubConnectionString"));
            string iotHubHostName = iotHubConnectionStringBuilder.HostName;
            string deviceConnectionString = "HostName=" + iotHubHostName + ";DeviceId=" + iotDevice.DeviceId + ";SharedAccessKey=" + createdDevice.Authentication.SymmetricKey.PrimaryKey;
            
            return this.Created("deviceinfo", new { connectionString = deviceConnectionString, deviceId = iotDevice.DeviceId });
        }
    }
}
