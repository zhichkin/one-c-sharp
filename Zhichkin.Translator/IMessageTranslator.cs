namespace Zhichkin.Integrator.Translator
{
    public interface IMessageTranslator<TMessage>
    {
        TMessage Translate(TMessage message);
    }
}
