namespace ForkJoint.Contracts
{
    public interface DownloadFile :
        OrderLine
    {
        Download File { get; }
    }
    public interface ParseFile:
        OrderLine
    {
        Parse Parse { get; }
    }
}