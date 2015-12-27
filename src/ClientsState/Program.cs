using Microsoft.Azure.WebJobs;

namespace ClientsState
{
    class Program
    {
        static void Main()
        {
            var host = new JobHost();
            host.CallAsync(typeof(Functions).GetMethod("MonitorClientsState"));            
            host.RunAndBlock();
        }
    }
}
