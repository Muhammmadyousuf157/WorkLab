let sessionUsers = [];
let sessionKey = '';

const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl('/sessionHub')
    .build();