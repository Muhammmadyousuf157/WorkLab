using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WorkLabWeb.HubModels;
using WorkLabLibrary.DataAccess;

namespace WorkLabWeb.Hubs
{
    public class SessionHub : Hub<ISessionClient>
    {
        [Authorize]
        public async Task CreateSession(string type)
        {
            var sessionKey = Guid.NewGuid().ToString();
            var userName = Context.User.Identity.Name;

            var user = new ConnectedUser { UserId = Context.ConnectionId, UserName = userName };

            SessionInformation.SessionInfo.Add(sessionKey, (type, new StringBuilder(), Context.ConnectionId, new List<ConnectedUser> { user }));

            await Groups.AddToGroupAsync(Context.ConnectionId, sessionKey)
                .ConfigureAwait(false);

            await Clients.Caller.ReceiveNewSessionInfo(user, sessionKey, type)
                .ConfigureAwait(false);

            await Clients.Caller.NotifyUser(NotificationMessage.GetWelcomeMessage(userName))
                .ConfigureAwait(false);
        }

        public async Task JoinSession(string userName, string sessionKey)
        {
            var user = new ConnectedUser { UserId = Context.ConnectionId, UserName = userName };

            SessionInformation.SessionInfo[sessionKey].connectedUsers.Insert(0, user);

            var users = SessionInformation.SessionInfo[sessionKey].connectedUsers;

            await Groups.AddToGroupAsync(Context.ConnectionId, sessionKey)
                .ConfigureAwait(false);

            await Clients.Caller.ReceiveJoinSessionInfo(users, SessionInformation.SessionInfo[sessionKey].type)
                .ConfigureAwait(false);

            await Clients.OthersInGroup(sessionKey).AddUser(user)
                .ConfigureAwait(false);

            await Clients.Caller.NotifyUser(NotificationMessage.GetWelcomeMessage(userName))
                .ConfigureAwait(false);

            await Clients.OthersInGroup(sessionKey).NotifyUser(NotificationMessage.GetUserJoinMessage(userName))
                .ConfigureAwait(false);
        }

        public async Task SendEditorContent(string editorContent, string sessionKey)
        {
            await Clients.OthersInGroup(sessionKey).ReceiveEditorContent(editorContent)
                .ConfigureAwait(false);
        }

        public async Task SendSpreadSheetContent(string spreadSheetContent, string sessionKey)
        {
            await Clients.OthersInGroup(sessionKey).ReceiveSpreadSheetContent(spreadSheetContent)
                .ConfigureAwait(false);
        }

        [Authorize]
        public async Task EndSessionForAll(string endDateTime, string sessionKey)
        {
            if (!string.Equals(Context.ConnectionId, SessionInformation.SessionInfo[sessionKey].hostId))
                return;

            await Clients.OthersInGroup(sessionKey).EndSession()
                .ConfigureAwait(false);

            var email = Context.User.FindFirst(x => x.Type == ClaimTypes.Email).Value;

            await SessionManager.EndSession(email, endDateTime, sessionKey).ConfigureAwait(false);

            SessionInformation.SessionInfo.Remove(sessionKey);
        }

        public async Task SendMessage(string message, string sessionKey)
        {
            string userName = SessionInformation.SessionInfo[sessionKey].connectedUsers.Find(user => user.UserId == Context.ConnectionId).UserName;

            await Clients.OthersInGroup(sessionKey).ReceiveMessage(message, userName)
                .ConfigureAwait(false);
        }

        public async Task StartedMessageTyping(string sessionKey, string userName)
        {
            await Clients.OthersInGroup(sessionKey).StartMessageTypingIndication(userName)
                .ConfigureAwait(false);
        }

        public async Task StoppedMessageTyping(string sessionKey)
        {
            await Clients.OthersInGroup(sessionKey).StopMessageTypingIndication()
                .ConfigureAwait(false);
        }

        public async Task StartedTyping(string sessionKey)
        {

            await Clients.OthersInGroup(sessionKey).StartTypingIndication(Context.ConnectionId)
                .ConfigureAwait(false);
        }

        public async Task StoppedTyping(string sessionKey)
        {

            await Clients.OthersInGroup(sessionKey).StopTypingIndication(Context.ConnectionId)
                .ConfigureAwait(false);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var item in SessionInformation.SessionInfo)
            {
                int index = item.Value.connectedUsers.FindIndex(x => x.UserId == Context.ConnectionId);

                if (index != -1)
                {
                    var user = item.Value.connectedUsers[index];

                    item.Value.connectedUsers.RemoveAt(index);

                    if (item.Value.connectedUsers.Count == 0)
                    {
                        SessionInformation.SessionInfo.Remove(item.Key);
                    }
                    else
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, item.Key)
                        .ConfigureAwait(false);

                        await Clients.OthersInGroup(item.Key).RemoveUser(user)
                            .ConfigureAwait(false);

                        await Clients.OthersInGroup(item.Key).NotifyUser(NotificationMessage.GetUserLeaveMessage(user.UserName))
                            .ConfigureAwait(false);

                        await Clients.OthersInGroup(item.Key).CloseVideoCall(Context.ConnectionId)
                            .ConfigureAwait(false);
                    }

                    break;
                }
            }

            await base.OnDisconnectedAsync(exception)
                .ConfigureAwait(false);
        }

        public async Task SendPeerId(string peerId, string sessionKey)
        {
            await Clients.OthersInGroup(sessionKey).ReceivePeerId(peerId, Context.ConnectionId)
                .ConfigureAwait(false);
        }

    }
}