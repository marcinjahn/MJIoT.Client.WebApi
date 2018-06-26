using MjIot.Storage.Models.EF6Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MjIot.Client.WebApi.Models
{
    public class UserParams
    {
        public string User { get; set; }
        public string Password { get; set; }
    }

    public class CheckUserParams
    {
    }

    public class GetDevicesParams
    {
    }



    //public class ConfigureListenersParams
    //{
    //    public int SenderId { get; set; }
    //    public int SenderPropertyId { get; set; }
    //    public IEnumerable<ListenerData> Listeners { get; set; }
    //}

    public class SetPropertyParams
    {
        public int DeviceId { get; set; }
        public int PropertyId { get; set; }
        public string Value { get; set; }
    }

    public class GetPropertiesParams
    {
        public int DeviceId { get; set; }
    }

    public class GetDeviceListenersParams : GetPropertiesParams { }

    public class GetPropertyListenersParams : GetDeviceListenersParams
    {
        public string SenderPropertyName { get; set; }
    }


    //public class ListenerData
    //{
    //    public int DeviceId { get; set; }
    //    public int PropertyId { get; set; }
    //    public ConnectionConditionType Condition { get; set; }
    //    public string ConditionValue { get; set; }
    //}

    public class ConnectionInfo
    {
        public DevicePropertyPair Sender { get; set; }
        public DevicePropertyPair Listener { get; set; }
        public ConnectionFilter Filter { get; set; }
        public string FilterValue { get; set; }
        public ConnectionCalculation Calculation { get; set; }
        public string CalculationValue { get; set; }
    }

    public class DevicePropertyPair
    {
        public int DeviceId { get; set; }
        public int PropertyId { get; set; }
    }

}