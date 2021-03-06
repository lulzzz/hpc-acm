namespace Microsoft.HpcAcm.Common.Dto
{
    public enum NodeHealth
    {
        OK,
        Warning,
        Error,
    }

    public enum NodeState
    {
        Online,
        Offline,
    }

    public class Node   
    {
        public string Name { get; set; }
        public NodeHealth Health { get; set; }
        public NodeState State { get; set; }
        public int RunningJobCount { get; set; }
        public int EventCount { get; set; }
        public ComputeClusterRegistrationInformation NodeRegistrationInfo { get; set; }
    }
}