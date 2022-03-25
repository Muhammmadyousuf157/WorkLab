using System.Collections.Generic;
using System.Text;

namespace WorkLabWeb.HubModels
{
    public static class SessionInformation
    {
        public static Dictionary<string, (string type, StringBuilder fileContent, string hostId, List<ConnectedUser> connectedUsers)> SessionInfo { get; set; } = new();
    }
}