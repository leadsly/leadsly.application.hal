namespace Leadsly.Application.Model
{
    public static class RabbitMQConstants
    {
        public const string DeliveryCount = "x-delivery-count";
        public const string Quorum = "quorum";
        public const string Classic = "classic";
        public const string QueueType = "x-queue-type";

        public static class NetworkingConnections
        {
            public const string QueueName = "networking.connections";
            public const string RoutingKey = "networking-connections";
            public const string NetworkingType = "networking-type";
            public const string ProspectList = "ProspectList";
            public const string SendConnectionRequests = "SendConnectionRequests";
        }

        public static class MonitorNewAcceptedConnections
        {
            public const string QueueName = "monitor.new.accepted.connections";
            public const string RoutingKey = "monitor-new-accepted-connections";
            public const string ExecuteType = "execution-type";
            public const string ExecuteOffHoursScan = "OffHoursScan";
            public const string ExecutePhase = "ExecutePhase";
        }

        public static class Networking
        {
            public const string QueueName = "networking";
            public const string RoutingKey = "networking-phase";
        }

        public static class FollowUpMessage
        {
            public const string QueueName = "follow.up.message";
            public const string RoutingKey = "follow-up-message";
        }

        public static class ScanProspectsForReplies
        {
            public const string QueueName = "scan.prospects.for.replies";
            public const string RoutingKey = "scan-prospects-for-replies";
            public const string ExecutionType = "execution-type";
            public const string ExecuteDeepScan = "DeepScan";
            public const string ExecutePhase = "ExecutePhase";
        }

        //public static class SendConnections
        //{
        //    public const string QueueName = "send.connection.requests";
        //    public const string RoutingKey = "send-connection-requests";
        //}

        //public static class ProspectList
        //{
        //    public const string QueueName = "prospect.list";
        //    public const string RoutingKey = "prospect-list";
        //}
    }
}
