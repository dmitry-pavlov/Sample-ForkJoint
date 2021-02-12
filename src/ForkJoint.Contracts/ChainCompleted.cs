using System;

namespace ForkJoint.Contracts
{
    public interface ChainCompleted :
        FutureCompleted
    {
        Guid ChainId { get; }

        string Description { get; }
    }
}