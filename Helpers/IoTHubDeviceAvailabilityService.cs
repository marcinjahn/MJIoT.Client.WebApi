using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MJIoT_WebAPI.Helpers
{

    public class IoTHubDeviceAvailabilityService : IDevicesAvailabilityService
    {
        ServiceClient _serviceClient;
        string connectionString = "HostName=MJIoT-Hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=SzQKdF1y6bAEgGfZei2bmq1Jd83odc+B2x197n2MtxA=";

        public IoTHubDeviceAvailabilityService()
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
        }

        public async Task<List<Boolean>> AreDevicesOnline(List<string> devicesIds)
        {
            List<Task<Boolean>> results = new List<Task<bool>>();

            //Console.WriteLine(DateTime.Now.ToString());
            foreach (var id in devicesIds)
            {
                results.Add(IsDeviceOnline(id));
            }

            await Task.WhenAll(results.ToArray());

            //Task.WaitAll(results.ToArray());
            //Console.WriteLine(DateTime.Now.ToString());

            //Console.WriteLine("RESULTS:");
            var statuses = new List<Boolean>();
            foreach (var result in results)
            {
                statuses.Add(result.Result);
            }
            //Console.WriteLine(res.Result);
            //Console.WriteLine("Press Enter to exit.");
            //Console.ReadLine();

            return statuses;
        }

        public async Task<Boolean> IsDeviceOnline(string deviceId)
        {
            var methodInvocation = new CloudToDeviceMethod("conn") { ResponseTimeout = TimeSpan.FromSeconds(5) };
            CloudToDeviceMethodResult response;
            try
            {
                response = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                return false;
            }

            return true;
        }
    }
}