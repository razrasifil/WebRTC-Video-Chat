using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Linq;

class WebRTCServer {
    static List<TcpClient> clients = new List<TcpClient>();

    static async Task Main() {
        TcpListener server = new TcpListener(IPAddress.Any, 8080);
        server.Start();
        Console.WriteLine(">>> Sinyalleşme Sunucusu 8080'de Hazır...");

        while (true) {
            TcpClient client = await server.AcceptTcpClientAsync();
            lock(clients) { clients.Add(client); }
            _ = HandleClient(client);
        }
    }

    static async Task HandleClient(TcpClient client) {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[65536];
        try {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            if (Regex.IsMatch(request, "^GET", RegexOptions.IgnoreCase)) {
                string key = Regex.Match(request, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                string acceptKey = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                string response = "HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: " + acceptKey + "\r\n\r\n";
                await stream.WriteAsync(Encoding.UTF8.GetBytes(response));
                Console.WriteLine("--- Bağlantı Onaylandı ---");
            }

            while (true) {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead <= 0) break;

                // 1. Maskeyi Çözme ve Veri Uzunluğunu Doğru Hesaplama
                bool mask = (buffer[1] & 0b10000000) != 0;
                long msgLen = buffer[1] & 0b01111111;
                int offset = 2;

                if (msgLen == 126) {
                    msgLen = BitConverter.ToUInt16(new byte[] { buffer[3], buffer[2] }, 0);
                    offset = 4;
                } else if (msgLen == 127) {
                    msgLen = (long)BitConverter.ToUInt64(buffer.Skip(2).Take(8).Reverse().ToArray(), 0);
                    offset = 10;
                }

                if (mask) {
                    byte[] masks = new byte[4] { buffer[offset], buffer[offset + 1], buffer[offset + 2], buffer[offset + 3] };
                    offset += 4;
                    for (int i = 0; i < (int)msgLen; ++i) buffer[offset + i] = (byte)(buffer[offset + i] ^ masks[i % 4]);
                }

                // 2. Veriyi Temizle (JSON hatasını önlemek için Null karakterleri sil)
                byte[] decodedData = new byte[msgLen];
                Array.Copy(buffer, offset, decodedData, 0, (int)msgLen);
                
                // 3. Veriyi Tekrar Paketle (Frame Oluştur)
                byte[] frame = CreateFrame(decodedData);

                TcpClient[] targets;
                lock(clients) { targets = clients.ToArray(); }
                foreach (var other in targets) {
                    if (other != client && other.Connected) {
                        try { 
                            await other.GetStream().WriteAsync(frame, 0, frame.Length); 
                        } catch { }
                    }
                }
            }
        } catch { }
        finally { 
            lock(clients) { clients.Remove(client); } 
            client.Close();
            Console.WriteLine("--- Bir Kullanıcı Ayrıldı ---"); 
        }
    }

    // Tarayıcının "Unterminated string" hatası almaması için en sağlam paketleme yöntemi
    static byte[] CreateFrame(byte[] data) {
        List<byte> frame = new List<byte>();
        frame.Add(0x81); // Text frame (0x81)
        
        if (data.Length <= 125) {
            frame.Add((byte)data.Length);
        } else if (data.Length <= 65535) {
            frame.Add(126);
            frame.Add((byte)((data.Length >> 8) & 255));
            frame.Add((byte)(data.Length & 255));
        } else {
            frame.Add(127);
            byte[] lenBytes = BitConverter.GetBytes((long)data.Length).Reverse().ToArray();
            frame.AddRange(lenBytes);
        }
        
        frame.AddRange(data);
        return frame.ToArray();
    }
}