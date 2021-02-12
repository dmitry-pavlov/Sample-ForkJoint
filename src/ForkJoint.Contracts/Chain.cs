using System;

namespace ForkJoint.Contracts
{
    public class Chain
    {
        public Guid ChainId { get; set; }

        public Guid? DownloadStepId { get; set; }
        public Guid? ParseStepId { get; set; }
        public Guid? PhotoStepId { get; set; }
        public Guid? ImportStepId { get; set; }

        public bool DownloadStepDone { get; set; }
        public bool ParseStepDone { get; set; }
        public bool PhotoStepDone { get; set; }
        public bool ImportStepDone { get; set; }
        public bool Done => DownloadStepDone && ParseStepDone && PhotoStepDone && ImportStepDone;
    }
}