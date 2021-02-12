using System;

namespace ForkJoint.Contracts
{
    public interface HandleDownload : OrderLine
    {
        Download File { get; }
    }
    public interface HandleParse : OrderLine
    {
        Parse Parse { get; }
    }
}