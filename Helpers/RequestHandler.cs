using MJIoT_DBModel;
using MJIoT_WebAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using MJIoT.Storage.PropertyValues;
using System;
using MJIoT.Storage.Models;
using MJIoT_WebAPI.Models.DTOs;
using System.Data.Entity;

namespace MJIoT_WebAPI.Helpers
{
    public class RequestHandler
    {
        IPropertyStorage _propertyStorage;
        IUnitOfWork _unitOfWork;
        IoTHubDeviceAvailabilityService _iotHubServices;

        Dictionary<DeviceType, List<PropertyDTO>> _deviceProperties;

        //string BadUserMessage = "You do not have access to MJ IoT System! (User not recognized)";
        //string PropertyNonExistentMessage = "This property does not exist in the system nad cannot be changed!";

        public RequestHandler(IUnitOfWork unitOfWork, IPropertyStorage propertyStorage)
        {
            _unitOfWork = unitOfWork;
            _propertyStorage = propertyStorage;

            _deviceProperties = new Dictionary<DeviceType, List<PropertyDTO>>();
        }

        //public User DoUserCheck(string login, string password)
        //{
        //    var user = _unitOfWork.Users.Get(login, password);
        //    if (user == null)
        //        ThrowUnauthorizedResponse();

        //    return user;
        //}

        public async Task<List<DeviceDTO>> GetDevices(int userId, bool includeListeners, bool includeDevicesAvailability, bool includeProperties)
        {
            var devices = _unitOfWork.Devices.GetDevicesOfUser(userId);

            if (includeDevicesAvailability)
                _iotHubServices = new IoTHubDeviceAvailabilityService();

            List<DeviceDTO> result = new List<DeviceDTO>();

            foreach (var device in devices)
            {
                DeviceDTO deviceData = await GetDeviceDTO(device, userId, includeListeners, includeDevicesAvailability, includeProperties);
                result.Add(deviceData);
            }

            return result;
        }

        public List<PropertyDTO> GetProperties(int userId, int deviceId)
        {
            var deviceType = _unitOfWork.Devices.GetDeviceType(deviceId);
            var properties = _unitOfWork.PropertyTypes.GetPropertiesOfDevice(deviceType);

            List<PropertyDTO> result = new List<PropertyDTO>();
            foreach (var property in properties)
            {
                result.Add(new PropertyDTO
                {
                    Id = property.Id,
                    IsConfigurable = property.UIConfigurable,
                    Name = property.Name,
                    IsListenerProperty = property.IsListenerProperty,
                    IsSenderProperty = property.IsSenderProperty,
                    Format = property.Format
                });
            }


            //Getting value of each property - turned off - it's seperate API's job
            //List<Task> tasks = new List<Task>();
            //foreach (var entry in result)
            //{
            //    tasks.Add(
            //        Task.Run(async () =>
            //        {
            //            entry.PropertyValue =
            //            await _propertyStorage.GetPropertyValueAsync(
            //                deviceId,
            //                entry.Name);
            //        })
            //    );
            //}
            //await Task.WhenAll(tasks);

            return result;

            //List<Task<string>> valuesTasks = new List<Task<string>>();
            //foreach (var property in properties)
            //{
            //    valuesTasks.Add(
            //        _propertyStorage.GetPropertyValueAsync(
            //            parameters.DeviceId, 
            //            property.Name)
            //    );
            //}


        }

        internal async Task<List<ConnectionDTO>> GetConnections(int userId)
        {
            var connections = _unitOfWork.Connections.GetUserConnections(userId);
            var result = new List<ConnectionDTO>();

            var nameTasks = new List<Task<string>>();

            foreach (var connection in connections)
            {
                var senderName = _propertyStorage.GetPropertyValueAsync(connection.SenderDevice.Id, "Name");
                var listenerName = _propertyStorage.GetPropertyValueAsync(connection.ListenerDevice.Id, "Name");
                nameTasks.Add(senderName);
                nameTasks.Add(listenerName);

                result.Add(new ConnectionDTO
                {
                    Id = connection.Id,
                    Sender = new DevicePropertyPairDTO
                    {
                        DeviceId = connection.SenderDevice.Id,
                        PropertyId = connection.SenderProperty.Id,
                        PropertyName = connection.SenderProperty.Name,
                        PropertyFormat = connection.SenderProperty.Format
                    },
                    Listener = new DevicePropertyPairDTO
                    {
                        DeviceId = connection.ListenerDevice.Id,
                        PropertyId = connection.ListenerDevice.Id,
                        PropertyName = connection.ListenerProperty.Name,
                        PropertyFormat = connection.ListenerProperty.Format
                    },
                    Calculation = connection.Calculation,
                    CalculationValue = connection.CalculationValue,
                    Filter = connection.Filter,
                    FilterValue = connection.FilterValue
                });
            }

            await Task.WhenAll(nameTasks);

            for (var i = 0; i < result.Count; i++)
            {
                result[i].Sender.DeviceName = nameTasks[i * 2].Result;
                result[i].Listener.DeviceName = nameTasks[i * 2 + 1].Result;
            }

            return result;
        }

        internal PropertyListenersDTO GetPropertyListeners(int userId, int deviceId, int propertyId)
        {
            Device device = _unitOfWork.Devices.Get(deviceId);
            var connections = _unitOfWork.Connections.GetDeviceConnections(device)
                .Where(n => n.SenderProperty.Id == propertyId)
                .ToList();

            return GeneratePropertyListenersDTO(propertyId, connections);
        }

        public IEnumerable<PropertyListenersDTO> GetDeviceListeners(int userId, int deviceId)
        {
            var listeners = GenerateListenersData(_unitOfWork.Devices.Get(deviceId));
            return listeners;
        }

        public void SetConnections(int userId, IEnumerable<ConnectionInfo> connectionsData)
        {
            _unitOfWork.Connections.RemoveAll();
            _unitOfWork.Save();
            AddConnections(userId, connectionsData);
        }

        public void AddConnections(int userId, IEnumerable<ConnectionInfo> connectionsData)
        {
            foreach (var connection in connectionsData) {
                var connectionObject = CreateConnectionObject(connection, userId);
                if (!IsConnectionAlreadyExsisting(connectionObject))
                    _unitOfWork.Connections.Add(connectionObject);
            }

            _unitOfWork.Save();
        }

        private bool IsConnectionAlreadyExsisting(Connection connection)
        {
            if (_unitOfWork.Connections.FindDuplicate(connection) != null)
            {
                return true;
            }

            return false;
        }

        public void RemoveConnections(IEnumerable<int> connectionsIds)
        {
            foreach (var connectionId in connectionsIds)
            {
                _unitOfWork.Connections.Remove(_unitOfWork.Connections.Get(connectionId));
                //RemoveConnection(connection);
            }

            _unitOfWork.Save();
        }

        public void RemoveConnection(ConnectionInfo connectionData)
        {
            var senderDevice = _unitOfWork.Devices.Get(connectionData.Sender.DeviceId);
            var connections = _unitOfWork.Connections.GetDeviceConnections(senderDevice);

            //var connectionsToRemove = connections
            //    .Where(n =>
            //    {
            //        return (
            //            connectionData.Listeners
            //                .Select(b => b.DeviceId)
            //                .Contains(n.ListenerDevice.Id)
            //            &&
            //            connectionsData.Listeners
            //                .Select(b => b.PropertyId)
            //                .Contains(n.ListenerProperty.Id)
            //            &&
            //            connectionsData.Listeners
            //                .Select(b => (int)b.Condition)
            //                .Contains((int)n.Condition)
            //            &&
            //            connectionsData.Listeners
            //                .Select(b => b.ConditionValue)
            //                .Contains(n.ConditionValue)
            //        );
            //    })
            //    .ToList();

            var connectionsToRemove = connections
                .Where(n => n.SenderDevice.Id == connectionData.Sender.DeviceId && n.ListenerDevice.Id == connectionData.Listener.DeviceId && n.SenderProperty.Id == connectionData.Sender.PropertyId && n.ListenerProperty.Id == connectionData.Listener.PropertyId && n.Filter == connectionData.Filter && n.FilterValue == connectionData.FilterValue && n.Calculation == connectionData.Calculation && n.CalculationValue == connectionData.CalculationValue)
                .ToList();

            //można by wykorzytać CreateConnectionObject() a później FindDuplicate(), żeby znaleźć Connection do usunięcia

            _unitOfWork.Connections.RemoveRange(connectionsToRemove);
        }

        private Connection CreateConnectionObject(ConnectionInfo connectionInfo, int userId)
        {
            return new Connection
            {
                SenderDevice = _unitOfWork.Devices.Get(connectionInfo.Sender.DeviceId),
                SenderProperty = _unitOfWork.PropertyTypes.Get(connectionInfo.Sender.PropertyId),
                ListenerDevice = _unitOfWork.Devices.Get(connectionInfo.Listener.DeviceId),
                ListenerProperty = _unitOfWork.PropertyTypes.Get(connectionInfo.Listener.PropertyId),
                Filter = connectionInfo.Filter,
                FilterValue = connectionInfo.FilterValue,
                Calculation = connectionInfo.Calculation,
                CalculationValue = connectionInfo.CalculationValue,
                User = _unitOfWork.Users.Get(userId)
            };
        }

        private async Task<DeviceDTO> GetDeviceDTO(MJIoT_DBModel.Device device, int userId, bool includeListeners, bool includeDevicesAvailability, bool includeProperties)
        {
            //var name = _modelStorage.GetDeviceName(device);
            var name = await _propertyStorage.GetPropertyValueAsync(device.Id, "Name");
            bool? isConnected = (includeDevicesAvailability) ? 
                (bool?)(await _iotHubServices.IsDeviceOnline(device.Id.ToString())) : 
                null;
            var deviceRole = _unitOfWork.Devices.GetDeviceRole(device);
            var type = device.DeviceType.Name;
            var connectedListeners =  includeListeners ? GenerateListenersData(device) : null;

            List<PropertyDTO> properties = null;
            if (includeProperties)
            {
                if (!_deviceProperties.ContainsKey(device.DeviceType))
                    _deviceProperties[device.DeviceType] = GetProperties(userId, device.Id);

                properties = _deviceProperties[device.DeviceType];
            }

            var item = new DeviceDTO
            {
                Id = device.Id,
                Name = name,
                DeviceType = type,
                CommunicationType = deviceRole,
                IsConnected = isConnected,
                ConnectedListeners = connectedListeners,
                Properties = properties
            };

            return item;
        }

        private List<PropertyListenersDTO> GenerateListenersData(MJIoT_DBModel.Device device)
        {
            var connections = _unitOfWork.Connections.GetDeviceConnections(device);
            var connectionGroups = connections
                .GroupBy(n => n.SenderProperty.Id);

            var result = new List<PropertyListenersDTO>();
            foreach (var group in connectionGroups)
            {
                //List<Connection> connections = new List<Connection>();

                PropertyListenersDTO propertyListener = GeneratePropertyListenersDTO(group.Key, group);
                result.Add(propertyListener);
            }

            return result;
        }

        private PropertyListenersDTO GeneratePropertyListenersDTO(int propertyId, IEnumerable<Connection> connections)
        {
            var propertyListener = new PropertyListenersDTO
            {
                PropertyId = propertyId,
                Listeners = new List<SingleListenerDTO>()
            };

            foreach (var connection in connections)
            {
                propertyListener.Listeners.Add(
                    new SingleListenerDTO
                    {
                        PropertyId = connection.ListenerProperty.Id,
                        PropertyName = connection.ListenerProperty.Name,
                        DeviceId = connection.ListenerDevice.Id.ToString(),
                        Filter = connection.Filter,
                        FilterValue = connection.FilterValue,
                        Calculation = connection.Calculation,
                        CalculationValue = connection.CalculationValue
                    }
                );
            }

            return propertyListener;
        }

        //private void ThrowUnauthorizedResponse()
        //{
        //    HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        //    {
        //        Content = new StringContent(BadUserMessage)
        //    };
        //    throw new HttpResponseException(message);
        //}
    }
}