namespace WorkLabWeb.HubModels
{
    public static class NotificationMessage
    {
        public static string GetWelcomeMessage(string userName)
        {
            return $"<span>Welcome to WORKLAB <span class=\"yellow-text\"> {userName}</span></span>";
        }

        public static string GetUserJoinMessage(string userName)
        {
            return $"<span><span class=\"yellow-text\"> {userName}</span> just joined the session</span>";
        }

        public static string GetUserLeaveMessage(string userName)
        {
            return $"<span><span class=\"yellow-text\"> {userName}</span> has left the session</span>";
        }
    }
}