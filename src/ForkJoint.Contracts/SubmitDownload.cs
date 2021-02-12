using System;

namespace ForkJoint.Contracts
{
    public interface SubmitDownload
    {
        Guid ChainId { get; }

        Guid OrderId { get; }

        Download[] Files { get; }
    }
    public interface SubmitParse
    {
        Guid ChainId { get; }

        Guid OrderId { get; }

        Parse[] Parses { get; }
    }

    public interface DownloadSubmitted
    {
        Guid StepId { get; }
    }
    public interface ParseSubmitted
    {
        Guid StepId { get; }
    }

}