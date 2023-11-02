using System.Net.Sockets;
using System.Text;
using Multiplayer_Games_Programming_Packet_Library;

namespace Multiplayer_Games_Programming_Server
{
	internal class ConnectedClient
	{
        Socket m_Socket;
        NetworkStream m_Stream;
        StreamReader m_StreamReader;
        StreamWriter m_StreamWriter;

        public ConnectedClient(int index, Socket socket)
		{
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

        public void Send(Packet packet)
        {
            string message = packet.Serialize();

            Console.WriteLine("Send: " + message);
            
            m_StreamWriter.WriteLine(packet);            
            m_StreamWriter.Flush();
        }
    }
}
