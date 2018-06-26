using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MjIot.Client.WebApi.Helpers
{
    public interface IDevicesAvailabilityService
    {
        Task<List<Boolean>> AreDevicesOnline(List<string> ids);
        Task<Boolean> IsDeviceOnline(string deviceId);
    }
}