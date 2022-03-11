$(document).ready(() => {
    const $chatLink = $('#chat-box');

    let isChatBox = false;
    let chatBox = null;

    $chatLink.click(() => {
        if (!isChatBox) {
            chatBox = new WinBox('Chat', {
                index: 999,
                class: ['no-max', 'no-full', 'no-resize'],
                root: document.body,
                background: "#388e3c",
                border: 4,
                x: 'center',
                y: 'center',
                width: 505,
                height: 605,
                mount: document.querySelector('#chat-box-markup').firstElementChild,
                onclose: () => {
                    isChatBox = false
                    chatBox = null;
                },
                onresize: () => {
                    scrollBottom();

                    $chatLink.find('span.badge').fadeOut(function () {
                        $(this).remove();
                    });
                }
            });

            chatBox.maximize = () => { };

            isChatBox = true;
            scrollBottom();
        }
        else {
            chatBox.minimize(false);
        }

        $('#chat-message-input').focus();
    });

    let typing = false;
    let timeout = undefined;

    async function timeoutFunction() {
        typing = false;
        await hubConnection.invoke('StoppedMessageTyping', sessionKey);
    }

    $('#chat-message-input').keyup(async e => {
        if (e.keyCode === 13)
            return;

        if (typing == false) {
            typing = true
            await hubConnection.invoke('StartedMessageTyping', sessionKey, $('.participant-list ul li:eq(0) a').text());
            timeout = setTimeout(timeoutFunction, 1000);
        } else {
            clearTimeout(timeout);
            timeout = setTimeout(timeoutFunction, 1000);
        }
    });

    $('.chat-message form').submit(async function (e) {
        e.preventDefault();

        const $messageInput = $(this).find('#chat-message-input');
        const messageText = $messageInput.val().trim();

        if (!messageText)
            return;

        displayMessage('Me', messageText, 1);

        scrollBottom();

        $messageInput.val('');

        await hubConnection.invoke('SendMessage', messageText, sessionKey);
    });

    function getCurrentTime() {
        return new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    function scrollBottom() {
        const chatHistory = document.querySelector('.chat-history');
        chatHistory.scrollTop = chatHistory.scrollHeight;
    }

    function displayMessage(senderName, messageText, type = 0) {
        let first = '';
        let second = 'my-message';
        let third = '';

        if (type === 1) {
            first = 'right-align';
            second = 'other-message';
            third = 'right';
        }

        const markup = `
            <li class="clearfix">
                <div class="message-data ${first}">
                    <span id="sender-name">${senderName}</span>
                    <span class="message-data-time">${getCurrentTime()}</span>
                </div>
                <div class="message ${second} ${third} black-text">${messageText}</div>
            </li>
        `;

        $('.chat-history ul').append(markup);
    }

    hubConnection.on('StartMessageTypingIndication', userName => {
        $('.wb-title').html(`Chat <span class="yellow-text text-accent-2 chat-typing-indication">${userName} is typing...</span>`);
    });

    hubConnection.on('StopMessageTypingIndication', () => {
        $('.wb-title').find('span.chat-typing-indication').remove();
    });

    hubConnection.on('ReceiveMessage', (message, userName) => {
        displayMessage(userName, message);

        scrollBottom();

        if (!(!chatBox || chatBox.min))
            return;

        const $badge = $chatLink.find('span.badge');

        if ($badge.length)
            $badge.text(parseInt($badge.text()) + 1);
        else
            $('<span class="new badge black pulse">1</span>').hide().prependTo($chatLink).fadeIn();
    });
});