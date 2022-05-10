﻿export default function initializeVideoChat() {
    const videoList = document.querySelector('.video-list ul');
    const peers = {};
     const peer = new Peer(undefined, {
         host: 'worklabvideochat.herokuapp.com',
         port: 443,
         path: '/myapp',
         secure: true,
         config: {
             iceServers: [{ urls: ["stun:bn-turn1.xirsys.com"] }, { username: "BZs1OdXFWmSA5EkJvIN2hbWNpq0FLN7RRN_qoCp-w66rABKdqT1kZ4Vo13foSzrsAAAAAGJtY5lNdWhhbW1hZFlvdXN1Zg==", credential: "85fef31a-c8a2-11ec-b295-0242ac140004", urls: ["turn:bn-turn1.xirsys.com:80?transport=udp", "turn:bn-turn1.xirsys.com:3478?transport=udp", "turn:bn-turn1.xirsys.com:80?transport=tcp", "turn:bn-turn1.xirsys.com:3478?transport=tcp", "turns:bn-turn1.xirsys.com:443?transport=tcp", "turns:bn-turn1.xirsys.com:5349?transport=tcp"] }]
         },
         debug: 3
     });

    let selfPeerId = undefined;

    navigator.mediaDevices.getUserMedia({
        video: true,
        audio: true
    }).then(async stream => {
        toggleVideoStream(stream);
        toggleAudioStream(stream);

        const { li, div, video } = getVideoElements();
        video.muted = true;

        addVideoStream(li, div, video, stream);

        peer.on('call', call => {
            call.answer(stream);
            const { li, div, video } = getVideoElements();
            call.on('stream', userStream => addVideoStream(li, div, video, userStream));
            call.on('close', () => video.parentElement.parentElement.remove());

            peer.on('connection', peerConnection => {
                peerConnection.on('data', userId => peers[userId] = call);
            });
        });

        hubConnection.on('ReceivePeerId', (peerId, userId) => connectToNewPeer(peerId, stream, userId));

        controlMediaStream(stream);

        const interval = setInterval(async () => {
            if (selfPeerId) {
                await hubConnection.invoke('SendPeerId', selfPeerId, sessionKey);
                clearInterval(interval);
            }
        }, 1000);
    });

    hubConnection.on('CloseVideoCall', userId => {
        if (peers[userId]) {
            peers[userId].close();
            delete peers[userId];
        }
    });

    peer.on('open', peerId => selfPeerId = peerId);

    function connectToNewPeer(peerId, stream, userId) {
        const call = peer.call(peerId, stream);

        const { li, div, video } = getVideoElements();

        call.on('stream', userStream => addVideoStream(li, div, video, userStream));
        call.on('close', () => video.parentElement.parentElement.remove());

        const peerConnection = peer.connect(peerId);

        peerConnection.on('open', () => {
            peerConnection.send(sessionUsers[0].userId);
        });

        peers[userId] = call;
    }

    function getVideoElements() {
        return {
            li: document.createElement('li'),
            div: document.createElement('div'),
            video: document.createElement('video')
        };
    }

    function addVideoStream(li, div, video, stream) {
        video.srcObject = stream;
        video.addEventListener('loadedmetadata', () => video.play());

        li.className = 'video-item';
        div.className = 'video-box';
        video.className = 'z-depth-2';
        video.setAttribute('oncontextmenu', 'return false;');

        div.append(video);
        li.append(div);
        videoList.append(li);
    }

    function toggleVideoStream(stream) {
        stream.getVideoTracks()[0].enabled = !(stream.getVideoTracks()[0].enabled);
    }

    function toggleAudioStream(stream) {
        stream.getAudioTracks()[0].enabled = !(stream.getAudioTracks()[0].enabled);
    }

    function controlMediaStream(stream) {
        $('#camera').click(function () {
            toggleVideoStream(stream);

            $(this).find('div#center-div').toggleClass('hide');

            const $icon = $(this).find('i');

            if ($icon.text() === 'videocam_off')
                $icon.text('videocam');
            else if ($icon.text() === 'videocam')
                $icon.text('videocam_off');
        });

        $('#microphone').click(function () {
            toggleAudioStream(stream);

            $(this).find('div#center-div').toggleClass('hide');

            const $icon = $(this).find('i');

            if ($icon.text() === 'mic_off')
                $icon.text('mic');
            else if ($icon.text() === 'mic')
                $icon.text('mic_off');
        });
    }
}