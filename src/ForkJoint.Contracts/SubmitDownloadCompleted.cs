using System;
using System.Collections.Generic;

namespace ForkJoint.Contracts
{
    public interface SubmitDownloadCompleted :
        FutureCompleted
    {
        Guid ChainId { get; }

        Guid OrderId { get; }

        IDictionary<Guid, DownloadCompleted> LinesCompleted { get; }
    }
    public interface SubmitParseCompleted :
        FutureCompleted
    {
        Guid ChainId { get; }

        Guid OrderId { get; }

        IDictionary<Guid, ParseCompleted> LinesCompleted { get; }
    }
}