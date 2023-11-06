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
                    Console.WriteLine("Connection Has Been Made");
                    //Console.WriteLine(Thread.CurrentThread.Name);
					ConnectedClient conClient = new ConnectedClient(index, socket);
					conClient.Send(new LoginPacket(index));
                    Thread thread = new Thread(() => { ClientMethod(index); });
					thread.Name = "Player Index: " + index.ToString();
                    m_Clients.GetOrAdd(index, conClient);
					thread.Start();
					LoginPacket loginPacket = new LoginPacket(index);
					conClient.Send(loginPacket);
					index++;
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
				Packet? packet;
				while((packet = m_Clients[index].Read()) != null)
				{
					switch (packet.Type)
					{
						case PacketType.MESSAGEPACKET:
							MessagePacket mp = (MessagePacket)packet;
							Console.WriteLine("Recieved Message: " + mp.m_message);
							m_Clients[index].Send(new MessagePacket("Logged In!"));							
							break;
						case PacketType.POSITIONPACKET:
							PositionPacket pp = (PositionPacket)packet;
							Console.WriteLine($"postision: Index: {pp.Index} X:{pp.X} Y:{pp.Y}");
							if(index == 0)
							{
								ConnectedClient? receiver;
								if (m_Clients.TryGetValue(index + 1, out receiver))
								{
									receiver.Send(new PositionPacket(pp.Index, pp.X, pp.Y));
								}
                            }
							else
							{
								ConnectedClient? receiver;
								if(m_Clients.TryGetValue((index - 1), out receiver))
								{
									m_Clients[index - 1].Send(new PositionPacket(pp.Index, pp.X, pp.Y));
								}

							}
							break;
						case PacketType.LOGINPACKET:
							LoginPacket lp = (LoginPacket)packet;
							//Console.WriteLine($"login: {lp.m_index}");
							m_Clients[index].Send(new LoginPacket(index));

							break;
					}



				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("Error" + ex.Message);
			}
			
			

			

		}
	}
}
