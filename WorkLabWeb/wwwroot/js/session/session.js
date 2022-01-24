$(document).ready(async () => {
    await hubConnection.start();

    $('#session-time').attr('data-badge-caption', getCurrentDateTime());

    addEventListener('beforeunload', askUserBeforeUnload);

    $('#end-btn').click(async () => {
        await hubConnection.invoke('EndSessionForAll', getCurrentDateTime(), sessionKey);
        leaveSession();
    });

    $('#leave-btn').click(() => {
        leaveSession();
    });

    $('#copy-session-key').click(() => {
        navigator.clipboard.writeText($('#session-key-chip').text());
        M.toast({ html: 'Copied to clipboard' });
    });

    $('#participant-search').keyup(function () {
        const query = $(this).val().trim().toLowerCase();

        if (!query) {
            showUsers(sessionUsers);
            return;
        }

        const filteredUsers = sessionUsers.filter(user => user.UserName.toLowerCase().includes(query));

        showUsers(filteredUsers);
    });

    function showUsers(users) {
        const $list = $('.participant-list ul');

        if (users.length === 0) {
            $list.html('<li class="participant center"><a class="blue-grey-text">NO PARTICIPANT</a></li>');
            return;
        }

        let markup = '';

        for (const user of users) {
            markup += `<li class="participant"><a class="blue-text text-darken-4" data-userid="${user.UserId}">${user.UserName.toUpperCase()}</a></li>`;
        }

        $list.html(markup);
    }

    function askUserBeforeUnload(e) {
        e.preventDefault();
        e.returnValue = '';
    }

    function leaveSession() {
        removeEventListener('beforeunload', askUserBeforeUnload);

        if ($('#user-authenticated').val() === 'yes')
            location.replace('/WorkSpace/Session/Dashboard');
        else if ($('#user-authenticated').val() === 'no')
            location.replace('/');
    }

    function getCurrentDateTime() {
        return new Date().toLocaleDateString('en-US', {
            day: '2-digit',
            month: 'short',
            year: 'numeric',
            weekday: 'short',
            hour: '2-digit',
            minute: '2-digit',
        });
    }

    function addUser(user) {
        sessionUsers.push(user);
        $('.participant-list ul').append(`<li class="participant"><a class="blue-text text-darken-4" data-userid="${user.UserId}">${user.UserName.toUpperCase()}</a></li>`);
    }

    function removeUser(user) {
        sessionUsers = sessionUsers.filter(x => x.UserId !== user.UserId);
        $('.participant-list ul').find(`li a[data-userid="${user.UserId}"]`).parent().remove();
    }

    function updateParticipantCount() {
        $('#no-of-participants').text(sessionUsers.length);
    }

    hubConnection.on('ReceiveNewSessionInfo', (user, _sessionKey) => {
        sessionKey = _sessionKey;
        addUser(user);
        updateParticipantCount();

        $('#session-key-chip').text(_sessionKey);
    });

    hubConnection.on('ReceiveJoinSessionInfo', users => {
        sessionUsers = users;
        showUsers(users);
        updateParticipantCount();
    });

    hubConnection.on('AddUser', user => {
        addUser(user);
        updateParticipantCount();
    });

    hubConnection.on('RemoveUser', user => {
        removeUser(user);
        updateParticipantCount();
    });

    hubConnection.on('NotifyUser', notification => {
        M.toast({ html: notification });
    });

    hubConnection.on('EndSession', async () => {
        await hubConnection.stop();
        leaveSession();
    });

    if ($('#session-type').val() === 'new') {
        await hubConnection.invoke('CreateSession', $('#document-type').val());
    }
    else if ($('#session-type').val() === 'join') {
        const userName = $('#session-username').val();
        const _sessionKey = $('#session-key').val();

        sessionKey = _sessionKey;
        $('#session-key-chip').text(_sessionKey);

        await hubConnection.invoke('JoinSession', userName, _sessionKey);
    }
});