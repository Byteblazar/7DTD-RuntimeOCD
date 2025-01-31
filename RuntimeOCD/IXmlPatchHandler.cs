namespace RuntimeOCD
{
    public interface IXmlPatchHandler
    {
        string Name { get; }
        void Run(PatchInfo args);
    }
}
