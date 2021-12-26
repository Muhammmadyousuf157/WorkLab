using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkLabWeb.HubModels;

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

            SessionInformation.SessionInfo.Add(sessionKey, (type, new List<ConnectedUser> { user }));

            await Groups.AddToGroupAsync(Context.ConnectionId, sessionKey)
                .ConfigureAwait(false);

            await Clients.Caller.ReceiveNewSessionInfo(user, sessionKey)
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

            await Clients.Caller.ReceiveJoinSessionInfo(users)
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
                    }

                    break;
                }
            }

            await base.OnDisconnectedAsync(exception)
                .ConfigureAwait(false);
        }
    
    }
}