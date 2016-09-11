using Zhichkin.Integrator.Model;

namespace Zhichkin.ChangeTracking
{
    public interface IMessageAdapter<TInput, TOutput>
    {
        IMessageAdapter<TInput, TOutput> Use(Subscription subscription);
        IMessageAdapter<TInput, TOutput> Input(TInput adaptee);
        void Output(TOutput target);
    }
}
