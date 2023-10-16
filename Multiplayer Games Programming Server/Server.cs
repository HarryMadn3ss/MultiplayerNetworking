using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using Multiplayer_Games_Programming_Packet_Library;
using System.Linq.Expressions;
using System.Text;

namespace Multiplayer_Games_Programming_Server
{
	internal class Server
	{
		TcpListener m_TcpListener; //listens for tcp connections on certain ip addresses

		ConcurrentDictionary<int, ConnectedClient> m_Clients;

		public Server(string ipAddress, int port)
		{
			IPAddress ip = IPAddress.Parse(ipAddress);
			m_TcpListener = new TcpListener(ip, port);
		}

		public void Start()
		{
			try //try this code
			{
				m_TcpListener.Start();
				Console.WriteLine("Server Started.....");

				Socket socket = m_TcpListener.AcceptSocket(); //blocking call so if no connections we will wait
				Console.WriteLine("Connection Has Been Made");

				ClientMethod(socket);
			}			
			catch(Exception ex) //if not catch the error
			{
				Console.WriteLine(ex.Message);
			}
		}

		public void Stop()
		{
			m_TcpListener?.Stop(); //? if statment if exists stop
		}

		private void ClientMethod(Socket index)
		{
			//client infomation
			try
			{
				string message;

				NetworkStream stream = new NetworkStream(index, false);

				StreamReader reader = new StreamReader(stream, Encoding.UTF8);

				StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);

				while ((message = reader.ReadLine()) != null) //aslong as there is a msg then read //blocking that it must recieve from the client first
				{
					Console.WriteLine("Recived Message: " + message);

					writer.WriteLine("Logged In!");
					writer.Flush();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
;			}
			finally
			{
				index.Close();
			}

		}
	}
}
