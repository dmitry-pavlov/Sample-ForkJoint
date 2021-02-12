using System;

namespace ForkJoint.Contracts
{
    public interface SubmitChain 
    {
        Guid ChainId { get; }

        string[] FileNames { get; }

    }
    public interface TriggerChain 
    {
        Guid ChainId { get; }
    }
    public class TestSignal : TriggerChain
    {
        public Guid ChainId { get; set;  }
    }
    public interface ChainStatus
    {
        Chain Chain { get; }
    }
}