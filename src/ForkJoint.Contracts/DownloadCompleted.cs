namespace ForkJoint.Contracts
{
    public interface DownloadCompleted :
        OrderLineCompleted
    {
        Download Download { get; }
    }
    public interface ParseCompleted :
        OrderLineCompleted
    {
        Parse Parse { get; }
    }
}