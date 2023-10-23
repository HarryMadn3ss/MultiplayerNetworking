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
			m_Clients = new ConcurrentDictionary<int, ConnectedClient>();
		}

		public void Start()
		{
			try //try this code
			{
				m_TcpListener.Start();
				Console.WriteLine("Server Started.....");
				int index = 0;

				while(true)
				{
                    Socket socket = m_TcpListener.AcceptSocket(); //blocking call so if no connections we will wait
					int tempIndex = index;
                    Console.WriteLine("Connection Has Been Made");
                    //Console.WriteLine(Thread.CurrentThread.Name);
					ConnectedClient conClient = new ConnectedClient(tempIndex, socket);
					index++;
                    Thread thread = new Thread(() => { ClientMethod(index); });
					thread.Name = "Player Index: " + index.ToString();
                    m_Clients.GetOrAdd(tempIndex, conClient);
					thread.Start();
				}

				//threads

				//ClientMethod(socket);
			}			
			catch(Exception ex) //if not catch the error
			{
				Console.WriteLine(ex.Message);
			}
		}

		public void Stop()
		{
			m_TcpListener?.Stop(); //'? if statment' if exists stop
		}

		private void ClientMethod(int index)
		{
			//wehn connected add to conncurency dictionary            


			//m_Clients.AddOrUpdate(index, new ConnectedClient(socket));
			try
			{
				string? message;
				while((message = m_Clients[index].Read()) != null)
				{
					m_Clients[index].Send(new Message(message));

				}
			}
			catch(Exception ex)
			{
			
			}
			
			

			

		}
	}
}
