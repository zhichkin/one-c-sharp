namespace Zhichkin.Translator
{
    public interface IMessageTranslator<TMessage>
    {
        TMessage Translate(TMessage message);
    }
}
