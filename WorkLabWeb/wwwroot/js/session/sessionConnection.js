let sessionUsers = [];
let sessionKey = '';

const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl('/sessionHub', { transport: signalR.HttpTransportType.WebSockets })
    .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
    .build();

hubConnection.serverTimeoutInMilliseconds = 60000;
hubConnection.keepAliveIntervalInMilliseconds = 30000;