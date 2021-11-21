using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkLabWeb.HubModels
{
    public interface ISessionClient
    {
        Task ReceiveNewSessionInfo(ConnectedUser user, string sessionKey);

        Task ReceiveJoinSessionInfo(List<ConnectedUser> users);

        Task AddUser(ConnectedUser user);

        Task RemoveUser(ConnectedUser user);

        Task NotifyUser(string notification);

        Task ReceiveEditorContent(string editorContent);

        Task ReceiveSpreadSheetContent(string spreadSheetContent);
    }
}