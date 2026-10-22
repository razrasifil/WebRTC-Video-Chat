# WebRTC Spring Chat

A simple real-time video calling prototype built with **WebRTC, JavaScript, and a custom C# WebSocket signaling server**. The application provides a spring-themed interface for camera access and peer-to-peer video communication.

## Features

- 📷 Local camera and microphone access
- 🎥 Peer-to-peer audio and video communication using WebRTC
- 🔗 Custom WebSocket signaling server built with C#
- 🌐 STUN server configuration for ICE candidate discovery
- 🌸 Responsive interface with a spring-inspired design

## Technologies

- **Frontend:** HTML5, CSS3, JavaScript
- **Communication:** WebRTC, WebSocket
- **Backend:** C# / .NET 9
- **Networking:** TCP sockets, STUN, ICE

## Getting Started

### Prerequisites

- .NET 9 SDK
- A modern browser with WebRTC support
- A working camera and microphone

### Run the Signaling Server

1. Navigate to the directory containing the C# project.
2. Start the server:

   ```bash
   dotnet run
   ```

3. The signaling server listens on port `8080`.

### Run the Frontend

1. Serve `index.html` and `main.js` using a local HTTP development server.
2. Open the page in your browser.
3. Allow camera and microphone access.
4. Start the camera and initiate a call.

> **Note:** The current implementation is a prototype. The signaling server broadcasts messages to connected clients without session management or message-type validation. A complete implementation requires proper WebSocket frame parsing, offer/answer routing, ICE candidate handling, and connection management. HTTPS/WSS is required for secure deployment, and camera access generally requires HTTPS or localhost.

## Project Structure

```text
WebRTC-Spring-Chat/
├── Program.cs
├── WebRTC-Spring-Chat.csproj
├── index.html
├── main.js
└── README.md
```

## Project Goal

To explore real-time browser-based communication, peer-to-peer media streaming, and signaling server development using WebRTC and C#.

## License

This project is licensed under the MIT License.
