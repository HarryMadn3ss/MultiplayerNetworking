using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Multiplayer_Games_Programming_Packet_Library;

namespace Multiplayer_Games_Programming_Server
{
	internal class ConnectedClient
	{
        Socket m_Socket;
        NetworkStream m_Stream;
        StreamReader m_StreamReader;
        StreamWriter m_StreamWriter;

        //security
        RSACryptoServiceProvider m_rsaProvider;
        public RSAParameters m_publicKey;
        RSAParameters m_privateKey;
        RSAParameters m_clientKey;

        public ConnectedClient(int index, Socket socket)
		{
            m_rsaProvider = new RSACryptoServiceProvider(1024); //must match the clients key size
            m_publicKey = m_rsaProvider.ExportParameters(false);
            m_privateKey = m_rsaProvider.ExportParameters(true);
            m_Socket = socket;

            m_Stream = new NetworkStream(socket, false);
            m_StreamReader = new StreamReader(m_Stream, Encoding.UTF8);
            m_StreamWriter = new StreamWriter(m_Stream, Encoding.UTF8);            
        }

		public void Close()
		{
			m_Socket.Close();
		}

        public Packet? Read()
        {
            //Message? msg = Message.Deserialize(message);   
            string? msg = m_StreamReader.ReadLine();
                        
            Packet? packet = Packet.Deserialize(msg);                
            return packet;            
        }

        public void Send(int index, Packet packet, bool sendEncrypted = true)
        {
            string message = packet.Serialize();

            if (sendEncrypted )
            {
                message = new EncryptedPacket(index, EncryptPacket(index, packet)).Serialize();
            }

            //Console.WriteLine("Send: " + message);
            
            m_StreamWriter.WriteLine(message);            
            m_StreamWriter.Flush();
        }


        public void Login(int index)
        {
            LoginPacket loginPacket = new LoginPacket(index, m_publicKey);
            //string msgToSend = message.Serialize();

            Send(index, loginPacket, false);
        }

        public byte[] EncryptPacket(int index, Packet? packet)
        {
            lock (m_rsaProvider)
            {                
                m_rsaProvider.ImportParameters(m_clientKey);
                string json = packet.Serialize();
                byte[] encrypted = m_rsaProvider.Encrypt(Encoding.UTF8.GetBytes(json), false);
                return encrypted;
            }
        }

        public Packet DecyrptPacket(byte[] data)
        {
            lock (m_rsaProvider)
            {
                m_rsaProvider.ImportParameters(m_privateKey);
                byte[] decyrpted = m_rsaProvider.Decrypt(data, false);
                string json = Encoding.UTF8.GetString(decyrpted);
                Packet packet = Packet.Deserialize(json);
                return packet;
            }
        }

        public void AssignKey(RSAParameters key)
        {
            m_clientKey = key;
        }
    }
}
