using SensorStateStats.Models;
using System.Collections.Generic;

namespace SensorStateStats.Storage
{
    interface IClientsCollection
    {
        List<Client> GetClients();
    }
}
