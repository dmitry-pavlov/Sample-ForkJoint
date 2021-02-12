using System;

namespace ForkJoint.Contracts
{
    public record Download
    {
        public Guid DownloadId { get; init; }
        public string FileName { get; init; }
        public bool Downloaded { get; init; }

        public override string ToString()
            => $"File {FileName} downloaded: {Downloaded}";
    }
    public record Parse
    {
        public Guid ParseId { get; init; }
        public string FileName { get; init; }
        public bool Parsed { get; init; }

        public override string ToString()
            => $"File {FileName} parsed: {Parsed}";
    }
}