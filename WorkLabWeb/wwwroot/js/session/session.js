$(document).ready(async () => {
    await hubConnection.start();

    $('#session-time').attr('data-badge-caption', getCurrentDateTime());

    //addEventListener('beforeunload', askUserBeforeUnload);

    $('#end-btn').click(async () => {
        await hubConnection.invoke('EndSessionForAll', getCurrentDateTime(), sessionKey);
        leaveSession();
    });

    $('#leave-btn').click(() => {
        leaveSession();
    });

    $('#copy-session-key').click(() => {
        navigator.clipboard.writeText($('#session-key-chip').text());
        M.toast({ html: 'Copied to clipboard', classes: 'rounded' });
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
        //removeEventListener('beforeunload', askUserBeforeUnload);

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

    let fileSaveTimeout = undefined;

    function configSessionFileUpdate(content) {
        if (!sessionCurrentFile) return;

        if (fileSaveTimeout) clearTimeout(fileSaveTimeout);

        fileSaveTimeout = setTimeout(async () => {
            const url = `/WorkSpace/Session/UpdateFileContent?filePath=${sessionCurrentFile.filePath}&sessionKey=${sessionKey}`;
            const response = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(content)
            });

            if (response.status !== 200)
                showAlert('Error', 'something went wrong while saving the file', true, 'OK');
        }, 3000);
    }

    function configureContentChangeEvent() {
        if (fileType === 'document') {
            editor.model.document.on('change:data', () => {
                configSessionFileUpdate(editor.getData());
            });
        } else if (fileType === 'spreadsheet') {
            s.change(async data => {
                const spreadSheetContent = JSON.stringify(data);
                await hubConnection.invoke('SendSpreadSheetContent', spreadSheetContent, sessionKey);

                configSessionFileUpdate(JSON.stringify(data));
            });
        }
	}

    hubConnection.on('ReceiveNewSessionInfo', async (user, _sessionKey, type) => {
        sessionKey = _sessionKey;
        addUser(user);
        updateParticipantCount();
        fileType = type;

        $('#session-key-chip').text(_sessionKey);

        const url = `/WorkSpace/Session/SaveSession?startDateTime=${getCurrentDateTime()}&sessionKey=${_sessionKey}&type=${type}`;
        const response = await fetch(url, { method: 'POST' });

        if (response.status !== 200)
            showAlert('Error', 'something went wrong while creating the session', true, 'OK');
        else
            sessionCurrentFile = await response.json();
        
        configureContentChangeEvent();

        if (fileType === 'document') {
            configSessionFileUpdate(editor.getData());

            $('#btn_Download')
                .attr('href', `/WorkSpace/Session/DownloadDoc?fileId=${sessionCurrentFile.fileId}`);
                //.click(() => {
                //    removeEventListener('beforeunload', askUserBeforeUnload)
                //    setTimeout(() => addEventListener('beforeunload', askUserBeforeUnload), 3000);
                //});
        } else if (fileType === 'spreadsheet') {
            configSessionFileUpdate($('#editor-content').text());
        }

        $('#file-title').trigger('change');
    });

    hubConnection.on('ReceiveJoinSessionInfo', async users => {
        sessionUsers = users;
        showUsers(users);
        updateParticipantCount();


        const url = `/WorkSpace/Session/SaveParticipant?userName=${$('#session-username').val()}&sessionKey=${$('#session-key').val()}`;
        const response = await fetch(url, { method: 'POST' });

        if (response.status !== 200)
            showAlert('Error', 'something went wrong while joining the session', true, 'OK');
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
        M.toast({ html: notification, classes: 'rounded' });
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

    const ft = $('#ft').val() ? $('#ft').val() : 'Untitled File';

    $('#file-title')
        .val(ft)
        .width($('#file-title-placeholder')
            .text(ft)
            .width())
        .show();

    $('#file-title')
        .on('input', function () {
            let rectifiedName = '';

            for (const char of $(this).val()) {
                if (!((/[a-zA-Z]/).test(char) || !isNaN(char) || [' ', '(', ')', '_', '-', ',', '.'].includes(char)))
                    continue;

                rectifiedName += char;
            }

            $(this).val(rectifiedName);

            $(this).width($('#file-title-placeholder').text($(this).val()).width());
        })
        .change(async function () {
            if (!sessionCurrentFile)
                return;

            if (!$(this).val()) {
                $(this).width($('#file-title-placeholder').text("Untitled File").width());
                $(this).val('Untitled File');
            }

            sessionCurrentFile.fileTitle = $(this).val();

            const url = `/WorkSpace/Session/UpdateFileTitle?fileId=${sessionCurrentFile.fileId}&fileTitle=${sessionCurrentFile.fileTitle}`;
            const response = await fetch(url, { method: 'POST' });

            if (response.status !== 200)
                showAlert('Error', 'something went wrong while renaming the file', true, 'OK');
        });
});