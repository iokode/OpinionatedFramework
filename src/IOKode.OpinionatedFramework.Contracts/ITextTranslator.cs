namespace IOKode.OpinionatedFramework.Contracts;

[AddToFacade("Text")]
public interface ITextTranslator
{
    public string Translate(string key);
}