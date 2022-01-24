using System.Collections.Generic;

namespace WorkLabWeb.HubModels
{
    public static class SessionInformation
    {
        public static Dictionary<string, (string type, string hostId, List<ConnectedUser> connectedUsers)> SessionInfo { get; set; } = new();
    }
}