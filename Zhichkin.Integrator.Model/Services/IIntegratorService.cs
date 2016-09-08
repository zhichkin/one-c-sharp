using Zhichkin.Integrator.Model;
using System.Collections.Generic;

namespace Zhichkin.Integrator.Services
{
    public interface IIntegratorService
    {
        IList<Publisher> GetPublishers();
        void PublishChanges(Publisher entity);
    }
}
