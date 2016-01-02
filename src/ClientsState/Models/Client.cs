using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;

namespace ClientsState.Models
{
    public class Client : Document
    {
        public bool IsOnline
        {
            get
            {
                return GetPropertyValue<bool>("isOnline");
            }
            set
            {
                SetPropertyValue("isOnline", value);
            }
        }

        public DateTime IsOnlineChanged
        {
            get
            {
                return GetPropertyValue<DateTime>("isOnlineChanged");
            }
            set
            {
                SetPropertyValue("isOnlineChanged", value);
            }
        }
    }
}