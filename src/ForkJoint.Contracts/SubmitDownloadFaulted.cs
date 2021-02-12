using System;
using System.Collections.Generic;
using MassTransit;

namespace ForkJoint.Contracts
{
    public interface SubmitDownloadFaulted :
        FutureFaulted
    {
        Guid OrderId { get; }

        IDictionary<Guid, DownloadCompleted> LinesCompleted { get; }

        IDictionary<Guid, Fault<DownloadFile>> LinesFaulted { get; }
    }
    public interface SubmitParseFaulted :
        FutureFaulted
    {
        Guid OrderId { get; }

        IDictionary<Guid, ParseCompleted> LinesCompleted { get; }

        IDictionary<Guid, Fault<ParseFile>> LinesFaulted { get; }
    }
}