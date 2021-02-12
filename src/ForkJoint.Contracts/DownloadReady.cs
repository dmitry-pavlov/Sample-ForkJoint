using System;

namespace ForkJoint.Contracts
{
    public interface DownloadReady
    {
        Guid OrderId { get; }
        Guid OrderLineId { get; }
        int Size { get; }
    }
    public interface ParseReady
    {
        Guid OrderId { get; }
        Guid OrderLineId { get; }
        int Products { get; }
    }
}