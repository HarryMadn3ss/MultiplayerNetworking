using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using Multiplayer_Games_Programming_Packet_Library;
using System.Linq.Expressions;
using System.Text;
using System;
using System.Security.Cryptography;
using System.Reflection;

namespace Multiplayer_Games_Programming_Server
{
	internal class Server
	{
		struct GameLobby
		{
			public ConnectedClient playerOne;
			public ConnectedClient playerTwo;
			public bool full;

            //score
            public int playerOneScore;
            public int playerTwoScore;
        };

		GameLobby[] m_gameLobbies = new GameLobby[2];

		

		TcpListener m_TcpListener; //listens for tcp connections on certain ip addresses
		UdpClient m_UdpListener; //listen for udp responses

		

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
					ConnectedClient conClient = new ConnectedClient(index, socket);                    
                    m_Clients.GetOrAdd(index, conClient);
                    Thread thread = new Thread(() => { ClientMethod(index); });
					thread.Name = "Player Index: " + index.ToString();
					thread.Start();
					conClient.m_lobbyNumber = FillGameLobbies(conClient);
					conClient.m_index = index;
                    conClient.Send(index, new LoginPacket(index, conClient.m_publicKey, conClient.m_lobbyNumber), false);					
					index++;
				}

				//threads

				//ClientMethod(socket);
			}			
			catch(Exception ex) //if not catch the error
			{
				Console.WriteLine(ex.Message);
				throw;
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
				throw;
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
						if (pp.m_playerNumber == 1)
						{
							//ConnectedClient? receiver;
							//if (m_Clients.TryGetValue(index + 1, out receiver))
							//{
							m_gameLobbies[pp.m_lobbyNumber].playerTwo.Send(index, pp);
							//}
						}
						else
						{
                            //ConnectedClient? receiver;
                            //if (m_Clients.TryGetValue((index - 1), out receiver))
                            //{
                            m_gameLobbies[pp.m_lobbyNumber].playerOne.Send(index, pp);
                            //}
                        }
                        break;
                    case PacketType.LOGINPACKET:
                        LoginPacket lp = (LoginPacket)packet;
						
						m_Clients[index].AssignKey(lp.m_key);
                        
                        break;

                    case PacketType.BALLPACKET:
                        BallPacket bp = (BallPacket)packet;

						if(bp.m_playerNumber == 1)
						{
							m_gameLobbies[bp.m_lobbyNumber].playerTwo.Send(index, bp);
						}
                        //if (index == 0)
                        //{
                        //    ConnectedClient? receiver;
                        //    if (m_Clients.TryGetValue(index + 1, out receiver))
                        //    {
                        //        receiver.Send(index, new BallPacket(bp.X, bp.Y));
                        //    }
                        //}
                        //else
                        //{
                        //    ConnectedClient? receiver;
                        //    if (m_Clients.TryGetValue((index - 1), out receiver))
                        //    {
                        //        m_Clients[index - 1].Send(index, new BallPacket(bp.X, bp.Y));
                        //    }
                        //}
                        break;
                    case PacketType.SCOREPACKET:
                        ScorePacket sp = (ScorePacket)packet;
						lock(sp)
						{
							if (sp.m_playerNumber == 2)
							{
							    m_gameLobbies[sp.m_lobbyNumber].playerOneScore++;
							}
							else if (sp.m_playerNumber == 1)
							{
                                m_gameLobbies[sp.m_lobbyNumber].playerTwoScore++;
							}
							m_gameLobbies[sp.m_lobbyNumber].playerOne.Send(index, new ScorePacket(sp.m_lobbyNumber, m_gameLobbies[sp.m_lobbyNumber].playerOneScore, m_gameLobbies[sp.m_lobbyNumber].playerTwoScore));
							m_gameLobbies[sp.m_lobbyNumber].playerTwo.Send(index, new ScorePacket(sp.m_lobbyNumber, m_gameLobbies[sp.m_lobbyNumber].playerOneScore, m_gameLobbies[sp.m_lobbyNumber].playerTwoScore));
							//m_Clients[index].Send(index, new ScorePacket(playerOneScore, playerTwoScore));
							//ConnectedClient? receiverClient;
							//if (m_Clients.TryGetValue(index + 1, out receiverClient))
							//{
							//    receiverClient.Send(index, new ScorePacket(playerOneScore, playerTwoScore));
							//}
						}                       
                        break;
					case PacketType.TIMERPACKET:
						TimerPacket tp = (TimerPacket)packet;

						m_gameLobbies[tp.m_lobbyNumber].playerTwo.Send(index, tp);
       //                     ConnectedClient? timerReceiver;
       //                     if (m_Clients.TryGetValue((index + 1), out timerReceiver))
							//{
							//	timerReceiver?.Send(index, new TimerPacket(tp.m_gameTimer, tp.m_restartTimer));
							//}
												
						break;

					case PacketType.SERVERSTATUSPACKET:
						ServerStatusPacket ssp = (ServerStatusPacket)packet;
						if (m_gameLobbies[ssp.m_serverNumber].full)
						{
							ServerStatusPacket serverResponse = new ServerStatusPacket(ssp.m_serverNumber, true);
							m_gameLobbies[ssp.m_serverNumber].playerOne.Send(index, serverResponse, false);
							m_gameLobbies[ssp.m_serverNumber].playerTwo.Send(index, serverResponse, false);

						}
						break;

					case PacketType.GAMESTARTPACKET:
						GameStartPacket gsp = (GameStartPacket)packet;
						lock(gsp)
						{
                            if (gsp.m_startGame)
                            {
                                m_gameLobbies[m_Clients[gsp.m_index].m_lobbyNumber].playerOne.Send(index, new GameStartPacket(true));
                                m_gameLobbies[m_Clients[gsp.m_index].m_lobbyNumber].playerTwo.Send(index, new GameStartPacket(true));
                            }
                        }						
						break;

                    case PacketType.GAMECOUNTDOWN:
                        GameCountdownPacket gcd = (GameCountdownPacket)packet;
                        m_gameLobbies[gcd.m_lobbyNumber].playerOne.Send(index, gcd);
                        m_gameLobbies[gcd.m_lobbyNumber].playerTwo.Send(index, gcd);                        
                        break;

                    default: break;
                } 
            }
			catch (Exception ex)
			{
				Console.WriteLine("Error *HandleTcp* " + ex.Message);
				throw;
			}
		}

		async Task UDPListen()
		{
			while(true)
			{
				UdpReceiveResult receiveResult = await m_UdpListener.ReceiveAsync();
				byte[] receivedData = receiveResult.Buffer;

				string message = Encoding.UTF8.GetString(receivedData, 0, receivedData.Length);

				Packet? packetToRead = Packet.Deserialize(message);

				switch (packetToRead.Type)
				{
					case PacketType.MESSAGEPACKET:
						MessagePacket mp = (MessagePacket)packetToRead;
						Console.WriteLine("UDP msg Recieved: " + mp.m_message);
						MessagePacket sendResponse = new MessagePacket("Message has been Recieved");
						SendUDP(sendResponse, receiveResult);
						break;
					case PacketType.LOGINPACKET:
						LoginPacket lp = (LoginPacket)packetToRead;
						m_Clients[lp.m_index].SetUDPAddress(receiveResult);
						MessagePacket sendLoginResponse = new MessagePacket("Message has been Recieved: Address Saved");
						SendUDP(sendLoginResponse, m_Clients[lp.m_index].GetUDPAddress());

						if (m_Clients[lp.m_index].m_lobbyReady)
						{
							LobbyPacket lobbyPacket = new LobbyPacket(true);
							m_gameLobbies[lp.m_index].playerOne.Send(lp.m_index, lobbyPacket, false);
							m_gameLobbies[lp.m_index].playerTwo.Send(lp.m_index, lobbyPacket, false);
						}

                        break;
					case PacketType.GAMESTATEPACKET:
						GameStatePacket gsp = (GameStatePacket)packetToRead;
						GameStatePacket SendGameStatePacket = new GameStatePacket(gsp.m_lobbyNumber, gsp.m_playerNumber, gsp.m_gameState, gsp.m_winnerState);
						if(gsp.m_gameState == 0)
						{
                            m_gameLobbies[gsp.m_lobbyNumber].playerOneScore = 0;
                            m_gameLobbies[gsp.m_lobbyNumber].playerTwoScore = 0;
						}

						m_gameLobbies[gsp.m_lobbyNumber].playerTwo.SendUDP(SendGameStatePacket, m_UdpListener);

						//ConnectedClient receiverClient;
						//if (m_Clients.TryGetValue(gsp.m_index + 1, out receiverClient))
						//{
						//	SendUDP(SendGameStatePacket, receiverClient.GetUDPAddress());
						//}
                            break;
					default:
                        break;
                }
			}
		}

        public void SendUDP(Packet packet, UdpReceiveResult receiveResult)
        {
			string packetToSend = packet.Serialize();
            byte[] bytes = Encoding.UTF8.GetBytes(packetToSend);
			m_UdpListener.SendAsync(bytes, bytes.Length, receiveResult.RemoteEndPoint);
        }		

		public int FillGameLobbies(ConnectedClient client)
		{
            if (!m_gameLobbies[0].full)
            {
                if (m_gameLobbies[0].playerOne == null)
                {
					client.m_playerNumber = 1;
                    m_gameLobbies[0].playerOne = client;
                    LobbyPacket lobbyPacket = new LobbyPacket(client.m_lobbyNumber, client.m_playerNumber);
					client.Send(client.m_index, lobbyPacket, false);
                }
                else
                {
					client.m_playerNumber = 2;
                    m_gameLobbies[0].playerTwo = client;
					m_gameLobbies[0].full = true;
					LobbyPacket lobbyPacket = new LobbyPacket(client.m_lobbyNumber, client.m_playerNumber);
                    client.Send(client.m_index, lobbyPacket, false);
                }
                return 0;
            }
			else
			{
                if (!m_gameLobbies[1].full)
                {
                    if (m_gameLobbies[1].playerOne == null)
                    {
                        client.m_playerNumber = 1;
                        m_gameLobbies[1].playerOne = client;
                        LobbyPacket lobbyPacket = new LobbyPacket(client.m_lobbyNumber, client.m_playerNumber);
                        client.Send(client.m_index, lobbyPacket, false);
                    }
                    else
                    {
                        client.m_playerNumber = 2;
                        m_gameLobbies[1].playerTwo = client;
                        m_gameLobbies[1].full = true;
                        LobbyPacket lobbyPacket = new LobbyPacket(client.m_lobbyNumber, client.m_playerNumber);
                        client.Send(client.m_index, lobbyPacket, false);
                    }
                }
				return 1;
            }			
        }
    }
}
