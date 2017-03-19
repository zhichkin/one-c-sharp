namespace Zhichkin.Metadata.Model
{
    public interface IDifferenceService
    {
        IDifferenceObject Compare(InfoBase target, InfoBase source);
        void Apply(IDifferenceObject differences);
    }
}
