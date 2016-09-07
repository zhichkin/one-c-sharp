using Zhichkin.Integrator.Model;
using System.Collections.Generic;

namespace Zhichkin.Integrator.Services
{
    public interface IIntegratorService
    {
        IList<Entity> GetPublishers();
        void PublishChanges(Entity entity);
    }
}
