namespace Microsoft.HpcAcm.Common.Dto
{
    using System.Collections.Generic;

    public enum JobState
    {
        Queued,
        Running,
        Finished,
        Failed,
        Canceled,
    }

    public enum JobType
    {
        ClusRun,
        Diagnostics,
    }

    public class Job
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public JobState State { get; set; }
        public JobType Type { get; set; }
        public int Progress { get; set; }
        public bool IsTaskExclusive { get; set; }
        public int RequeueCount { get; set; } = 0;

        public IList<DiagnosticsTest> DiagnosticTests { get; set; }
        public string CommandLine { get; set; }
        public string[] TargetNodes { get; set; }
    }
}