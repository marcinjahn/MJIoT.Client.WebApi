using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MJIoT_WebAPI.Helpers
{
    public interface IDevicesAvailabilityService
    {
        Task<List<Boolean>> AreDevicesOnline(List<string> ids);
        Task<Boolean> IsDeviceOnline(string deviceId);
    }
}