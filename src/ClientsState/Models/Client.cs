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
                return GetPropertyValue<bool>("IsOnline");
            }
            set
            {
                SetPropertyValue("IsOnline", value);
            }
        }

        public DateTime IsOnlineChanged
        {
            get
            {
                return GetPropertyValue<DateTime>("IsOnlineChanged");
            }
            set
            {
                SetPropertyValue("IsOnlineChanged", value);
            }
        }
    }
}