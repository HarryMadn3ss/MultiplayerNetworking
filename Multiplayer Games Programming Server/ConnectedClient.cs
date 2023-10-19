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

        public string? Read()
        {            
            return m_StreamReader.ReadLine();
        }

        public void Send(string message)
        {
            Console.WriteLine("Recived Message: " + message);

            m_StreamWriter.WriteLine("Logged In!");
            m_StreamWriter.Flush();
        }
    }
}
