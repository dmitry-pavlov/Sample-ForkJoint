using System;
using System.Collections.Generic;
using MassTransit;

namespace ForkJoint.Contracts
{
    public interface DownloadFaulted :
        FutureFaulted
    {
        Guid OrderId { get; }

        IDictionary<Guid, DownloadCompleted> LinesCompleted { get; }

        IDictionary<Guid, Fault<OrderLine>> LinesFaulted { get; }
    }
}