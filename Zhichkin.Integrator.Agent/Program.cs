using System.ServiceProcess;

namespace Zhichkin.Integrator.PublisherAgent
{
    static class Program
    {
        static void Main()
        {
            ServiceBase.Run(new ServiceBase[]
            {
                new PublisherAgent()
            });
        }
    }
}
