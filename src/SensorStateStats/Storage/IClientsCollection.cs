using SensorStateStats.Models;
using System.Collections.Generic;

namespace SensorStateStats.Storage
{
    public interface IClientsCollection
    {
        List<Client> GetClients();
    }
}
