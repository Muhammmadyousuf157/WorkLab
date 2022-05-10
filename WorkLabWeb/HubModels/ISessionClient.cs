using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkLabWeb.HubModels
{
    public interface ISessionClient
    {
        Task ReceiveNewSessionInfo(ConnectedUser user, string sessionKey, string type);

        Task ReceiveJoinSessionInfo(List<ConnectedUser> users);

        Task AddUser(ConnectedUser user);

        Task RemoveUser(ConnectedUser user);

        Task NotifyUser(string notification);

        Task ReceiveEditorContent(string editorContent);

        Task ReceiveSpreadSheetContent(string spreadSheetContent);

        Task ReceiveMessage(string message, string userName);

        Task StartMessageTypingIndication(string userName);
       
        Task StopMessageTypingIndication();

        Task StartTypingIndication(string userId);

        Task StopTypingIndication(string userId);

        Task EndSession();

        Task ReceivePeerId(string peerId, string userId);

        Task CloseVideoCall(string userId);
    }
}