using IOKode.OpinionatedFramework.Facades;

namespace IOKode.OpinionatedFramework.Contracts;

[AddToFacade("i18n")]
public interface ITextTranslator
{
    public string Translate(string key);
}