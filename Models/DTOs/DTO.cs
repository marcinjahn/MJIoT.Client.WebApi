using MJIoT.Storage.Models.Enums;
using MJIoT_DBModel;
using MJIoT_WebAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MJIoT_WebAPI.Models.DTOs
{

    public class DeviceDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DeviceType { get; set; }
        public DeviceRole CommunicationType { get; set; }  //sender lub listener lub bi-directional
        public bool? IsConnected { get; set; }
        public List<PropertyListenersDTO> ConnectedListeners { get; set; } //zawiera nazwy urządzeń
        public List<PropertyDTO> Properties { get; set; }
    }

    public class PropertyDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsConfigurable { get; set; }
        //public string PropertyValue { get; set; }
        public bool IsListenerProperty { get; set; }
        public bool IsSenderProperty { get; set; }
        public PropertyFormat Format { get; set; }
    }

    public class SingleListenerDTO
    {
        public string DeviceId { get; set; }
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public PropertyFormat Format { get; set; }
        public ConnectionFilter Filter { get; set; }
        public string FilterValue { get; set; }
        public ConnectionCalculation Calculation { get; set; }
        public string CalculationValue { get; set; }
    }

    public class ConnectionDTO
    {
        public int Id { get; set; }
        public DevicePropertyPairDTO Sender { get; set; }
        public DevicePropertyPairDTO Listener { get; set; }
        public ConnectionFilter Filter { get; set; }
        public string FilterValue { get; set; }
        public ConnectionCalculation Calculation { get; set; }
        public string CalculationValue { get; set; }
    }

    public class DevicePropertyPairDTO
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public PropertyFormat PropertyFormat { get; set; }
    }

    public class PropertyListenersDTO
    {
        public int PropertyId { get; set; }
        public List<SingleListenerDTO> Listeners { get; set; }
    }



}