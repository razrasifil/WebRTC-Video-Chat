let localStream;
let peerConnection;
let dataChannel;

const socket = new WebSocket('ws://localhost:8080');

socket.onopen = () => console.log("Sinyalleşme sunucusuna bağlandı.");
socket.onclose = () => alert("Sunucu bağlantısı koptu! C# terminalini kontrol et.");

const config = { iceServers: [{ urls: 'stun:stun.l.google.com:19302' }] };

document.getElementById('startButton').onclick = async () => {
    try {
        localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        document.getElementById('localVideo').srcObject = localStream;
        document.getElementById('callButton').disabled = false;
    } catch (e) { alert("Kamera hatası: " + e.message); }
};

async function setupPeerConnection() {
    if (peerConnection) return;
    peerConnection = new RTCPeerConnection(config);

    localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));

    peerConnection.ontrack = (event) => {
        document.getElementById('remoteVideo').srcObject = event.streams[0];
    };

    peerConnection.onicecandidate = (event) => {
        if (event.candidate) {
            sendMessage({ iceCandidate: event.candidate });
        }
    };
}

function sendMessage(message) {
    if (socket.readyState === WebSocket.OPEN) {
        socket.send(JSON.stringify(message));
    }
}

socket.onmessage = async (event) => {
    const data = JSON.parse(event.data);
    if (data.offer) {
        await setupPeerConnection();
        await peerConnection.setRemoteDescription(new RTCSessionDescription(data.offer));
        const answer = await peerConnection.createAnswer();
        await peerConnection.setLocalDescription(answer);
        sendMessage({ answer: answer });
    } else if (data.answer) {
        await peerConnection.setRemoteDescription(new RTCSessionDescription(data.answer));
    } else if (data.iceCandidate) {
        await peerConnection.addIceCandidate(new RTCIceCandidate(data.iceCandidate));
    }
};

document.getElementById('callButton').onclick = async () => {
    await setupPeerConnection();
    const offer = await peerConnection.createOffer();
    await peerConnection.setLocalDescription(offer);
    sendMessage({ offer: offer });
    console.log("Teklif gönderildi.");
};