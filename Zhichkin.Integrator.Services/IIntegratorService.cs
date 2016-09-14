using Zhichkin.Metadata.Model;
using Zhichkin.Integrator.Model;
using System.Collections.Generic;

namespace Zhichkin.Integrator.Services
{
    public interface IIntegratorService
    {
        IList<Publisher> GetPublishers();
        int PublishChanges(Publisher publisher);
        int ProcessMessages(Subscription subscription);

        Subscription CreateSubscription(Publisher publisher, Entity subscriber);
        void DeleteSubscription(Subscription subscription);
    }
}
