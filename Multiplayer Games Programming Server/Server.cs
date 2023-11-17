using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using Multiplayer_Games_Programming_Packet_Library;
using System.Linq.Expressions;
using System.Text;
using System;
using System.Security.Cryptography;

namespace Multiplayer_Games_Programming_Server
{
	internal class Server
	{
		TcpListener m_TcpListener; //listens for tcp connections on certain ip addresses
		UdpClient m_UdpListener; //listen for udp responses

		//score
		int playerOneScore = 0;
		int playerTwoScore = 0;

		ConcurrentDictionary<int, ConnectedClient> m_Clients;

		
		


		public Server(string ipAddress, int port)
		{
			

			IPAddress ip = IPAddress.Parse(ipAddress);
			m_TcpListener = new TcpListener(ip, port);
			m_UdpListener = new UdpClient(port);
			m_Clients = new ConcurrentDictionary<int, ConnectedClient>();
		}

		public void Start()
		{
			try //try this code
			{
				UDPListen();
				m_TcpListener.Start();
				Console.WriteLine("Server Started.....");
				int index = 0;

				while(true)
				{
                    Socket socket = m_TcpListener.AcceptSocket(); //blocking call so if no connections we will wait					
                    Console.WriteLine("Connection Has Been Made");
                    //Console.WriteLine(Thread.CurrentThread.Name);
					ConnectedClient conClient = new ConnectedClient(index, socket);
					conClient.Login(index);
                    Thread thread = new Thread(() => { ClientMethod(index); });
					thread.Name = "Player Index: " + index.ToString();
                    m_Clients.GetOrAdd(index, conClient);
					thread.Start();
					//LoginPacket loginPacket = new LoginPacket(index);
					//conClient.Send(index, loginPacket, false);
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
					if (packet == null) continue;

					HandleTCPPacket(index, packet);
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("Error" + ex.Message);
			}
		}

		private void HandleTCPPacket(int index, Packet packet)
		{
			try
			{
                switch (packet.Type)
                {
					case PacketType.ENCRYPTEDPACKET:
						EncryptedPacket encryptedPacket = (EncryptedPacket)packet;
						Packet? decryptedPacket = m_Clients[index].DecyrptPacket(encryptedPacket.m_encryptedData);

						if (decryptedPacket == null) return;

						HandleTCPPacket(index, decryptedPacket);

						break;
                    case PacketType.MESSAGEPACKET:
                        MessagePacket mp = (MessagePacket)packet;
                        Console.WriteLine("Recieved Message: " + mp.m_message);
                        m_Clients[index].Send(index, new MessagePacket("Logged In!"));
                        break;
                    case PacketType.POSITIONPACKET:
                        PositionPacket pp = (PositionPacket)packet;
                        //Console.WriteLine($"postision: Index: {pp.Index} X:{pp.X} Y:{pp.Y}");
                        if (index == 0)
                        {
                            ConnectedClient? receiver;
                            if (m_Clients.TryGetValue(index + 1, out receiver))
                            {
                                receiver.Send(index, new PositionPacket(pp.Index, pp.X, pp.Y));
                            }
                        }
                        else
                        {
                            ConnectedClient? receiver;
                            if (m_Clients.TryGetValue((index - 1), out receiver))
                            {
                                receiver.Send(index, new PositionPacket(pp.Index, pp.X, pp.Y));
                            }
                        }
                        break;
                    case PacketType.LOGINPACKET:
                        LoginPacket lp = (LoginPacket)packet;
						//m_Clients[index].Send(new LoginPacket(index, m_publicKey));
						m_Clients[index].AssignKey(lp.m_key);
                        //AssignKey(lp.m_index, lp.m_key);
                        break;

                    case PacketType.BALLPACKET:
                        BallPacket bp = (BallPacket)packet;
                        if (index == 0)
                        {
                            ConnectedClient? receiver;
                            if (m_Clients.TryGetValue(index + 1, out receiver))
                            {
                                receiver.Send(index, new BallPacket(bp.X, bp.Y));
                            }
                        }
                        else
                        {
                            ConnectedClient? receiver;
                            if (m_Clients.TryGetValue((index - 1), out receiver))
                            {
                                m_Clients[index - 1].Send(index, new BallPacket(bp.X, bp.Y));
                            }

                        }
                        break;
                    case PacketType.SCOREPACKET:
                        ScorePacket sp = (ScorePacket)packet;
                        if (sp.m_index == 1)
                        {
                            playerOneScore++;
                        }
                        else if (sp.m_index == 0)
                        {
                            playerTwoScore++;
                        }
                        m_Clients[index].Send(index, new ScorePacket(playerOneScore, playerTwoScore));
                        ConnectedClient? receiverClient;
                        if (m_Clients.TryGetValue(index + 1, out receiverClient))
                        {
                            receiverClient.Send(index, new ScorePacket(playerOneScore, playerTwoScore));
                        }
                        break;
                    default: break;
                } 
            }
			catch (Exception ex)
			{
				Console.WriteLine("Error *HandleTcp* " + ex.Message);
			}
		}

		async Task UDPListen()
		{
			while(true)
			{
				UdpReceiveResult receiveResult = await m_UdpListener.ReceiveAsync();
				byte[] receivedData = receiveResult.Buffer;

				string message = Encoding.UTF8.GetString(receivedData, 0, receivedData.Length);

				Packet? packet = Packet.Deserialize(message);

				byte[] bytes = Encoding.UTF8.GetBytes("Hello");

				m_UdpListener.SendAsync(bytes, bytes.Length, receiveResult.RemoteEndPoint);
				//switch (packet.Type)
				//{
				//	case PacketType.SCOREPACKET:
				//		ScorePacket sp = (ScorePacket)packet;
				//		if (sp.m_index == 1)
				//		{
				//			playerOneScore++;
				//		}
				//		else if (sp.m_index == 0)
				//		{
				//			playerTwoScore++;
				//		}
				//		byte[] bytes = Encoding.UTF8.GetBytes(new ScorePacket(playerOneScore, playerTwoScore));

				//		m_UdpListener.SendAsync(bytes, bytes.Length, receiveResult.RemoteEndPoint);

				//m_Clients[index].Send(new ScorePacket(playerOneScore, playerTwoScore));
				//ConnectedClient? receiverClient;
				//if (m_Clients.TryGetValue(index + 1, out receiverClient))
				//{
				//	receiverClient.Send(new ScorePacket(playerOneScore, playerTwoScore));
				//}

				//break;

				//	default: break;
				//}
				//Console.WriteLine("UDP msg Received: " + message);

				//byte[] bytes = Encoding.UTF8.GetBytes("Hello");


			}
		}

		

		

	}
}
