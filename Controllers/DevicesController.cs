using MJIoT_WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using MJIoT_WebAPI.Helpers;

using MJIoT.Storage.PropertyValues;
using MJIoT.Storage.Models;
using System.Web.Http.Cors;
using MJIoT_WebAPI.Models.DTOs;

namespace MJIoT_WebAPI.Controllers
{
    //[Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = true)]
    public class DevicesController : ApiController
    {
        private RequestHandler _handler;

        public DevicesController()
        {
            _handler = new RequestHandler(new UnitOfWork(), new CosmosPropertyStorage());
        }

        //[HttpGet] 
        //[ActionName("GetDevicesWithListeners")]
        //public async Task<List<DeviceWithListenersDTO>> GetDevicesWithListeners()
        //{
        //    //var userCheck = _handler.DoUserCheck(parameters.User, parameters.Password);
        //    return await _handler.GetDevices(GetUserId(), true);
        //}

        [HttpGet]
        [ActionName("GetDevices")]
        public async Task<List<DeviceDTO>> GetDevices(bool includeConnections, bool includeAvailability, bool includeProperties)
        {
            //var userCheck = _handler.DoUserCheck(parameters.User, parameters.Password);
            return await _handler.GetDevices(GetUserId(), includeConnections, includeAvailability, includeProperties);
        }

        [HttpGet]
        [ActionName("GetProperties")]
        public List<PropertyDTO> GetProperties(int deviceId)
        {
            return _handler.GetProperties(GetUserId(), deviceId);
        }

        [HttpPost]
        [ActionName("SetConnections")]
        public IHttpActionResult SetListeners(IEnumerable<ConnectionInfo> connections)
        {
            _handler.SetConnections(GetUserId(), connections);

            return Ok();
        }

        [HttpPost]
        [ActionName("AddConnections")]
        public IHttpActionResult AddConnections(IEnumerable<ConnectionInfo> connections)
        {
            _handler.AddConnections(GetUserId(), connections);

            return Ok();
        }

        [HttpGet]
        [ActionName("GetConnections")]
        public async Task<List<ConnectionDTO>> GetConnections()
        {
            return await _handler.GetConnections(GetUserId()) as List<ConnectionDTO>;
        }

        [HttpGet]
        [ActionName("GetDeviceListeners")]
        public List<PropertyListenersDTO> GetDeviceListeners(int deviceId)
        {
            return _handler.GetDeviceListeners(GetUserId(), deviceId) as List<PropertyListenersDTO>;
        }

        [HttpGet]
        [ActionName("GetPropertyListeners")]
        public PropertyListenersDTO GetPropertyListeners(int deviceId, int propertyId)
        {
            return _handler.GetPropertyListeners(GetUserId(), deviceId, propertyId);
        }

        [HttpGet]
        [ActionName("RemoveConnection")]
        public IHttpActionResult RemoveConnection(int connectionId)
        {
            _handler.RemoveConnections(new int[] { connectionId });

            return Ok();
        }

        [HttpGet]
        [ActionName("RemoveConnections")]
        public IHttpActionResult RemoveConnections(IEnumerable<int> connectionsIds)
        {
            _handler.RemoveConnections(connectionsIds);

            return Ok();
        }

        private int GetUserId()
        {
            return int.Parse(Request.Properties["userId"].ToString());
        }

        //[HttpPost]
        //[ActionName("SetProperty")]
        //public HttpResponseMessage SetProperty(SetPropertyParams parameters)
        //{
        //    var userCheck = Helper.CheckUser(parameters.User, parameters.Password);

        //    if (userCheck == null)
        //    {
        //        HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        //        {
        //            Content = new StringContent(BadUserMessage)
        //        };
        //        throw new HttpResponseException(message);
        //    }

        //    using (var context = new MJIoTDBContext())
        //    {
        //        var property = context.DeviceProperties
        //            .Include("Device")
        //            .Where(n => n.Device.Id == parameters.DeviceId && n.Id == parameters.PropertyId)
        //            .FirstOrDefault();

        //        if (property == null)
        //        {
        //            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.NotFound)
        //            {
        //                Content = new StringContent(PropertyNonExistentMessage)
        //            };
        //            throw new HttpResponseException(message);
        //        }

        //        property.Value = parameters.Value;
        //        context.SaveChanges();
        //    }
        //    return new HttpResponseMessage(HttpStatusCode.OK);
        //}
    }
}
