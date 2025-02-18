﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Multiplayer_Games_Programming_Packet_Library;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Xml.Xsl;
using System.Xml.Linq;
using Multiplayer_Games_Programming_Framework.GameCode.Components;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Reflection;


namespace Multiplayer_Games_Programming_Framework.Core
{
	internal class NetworkManager
	{
		private static NetworkManager Instance;

		public static NetworkManager m_Instance
		{
			get
			{
				if (Instance == null)
				{
					return Instance = new NetworkManager();
				}
			
				return Instance;
			}
		}

		TcpClient m_TcpClient;
		UdpClient m_UdpClient;

		NetworkStream m_Stream;
		StreamReader m_StreamReader;
		StreamWriter m_StreamWriter;

		public int m_index;
		public Vector2 m_positionUpdate;
		public Vector2 m_ballPositionUpdate;

		public int m_playerOneScore;
		public int m_playerTwoScore;

		//security
		RSACryptoServiceProvider m_RSAProvider;
		public RSAParameters m_publicKey; //clients public key
		RSAParameters m_privateKey; //clients private key
		RSAParameters m_serverPublicKey;

		//gamemodes
		public int m_gameState;
		public int m_gameWinner;
		public float m_gameTimer;
		public float m_gameRestartTimer;

		public int m_lobbyNumber;
		public int m_playerNumber;
		public bool m_lobbyReady;
		public bool m_gameStart;
		public float m_countDown;		
		


		NetworkManager()
		{
			m_TcpClient = new TcpClient();
			m_UdpClient = new UdpClient();
			//keys
			m_RSAProvider = new RSACryptoServiceProvider(1024); //number denotes how strong the encryption, the higher the number the slower -  this number cannot change between setting the keys
			m_publicKey = m_RSAProvider.ExportParameters(false); //false sets to public key
			m_privateKey = m_RSAProvider.ExportParameters(true); //true sets it to private by adding more parameters			
		}

		public bool Connect(string ip, int port)
		{
			try
			{
				m_TcpClient.Connect(ip, port);
				m_UdpClient.Connect(ip, port);

				m_Stream = m_TcpClient.GetStream();
				m_StreamReader = new StreamReader(m_Stream, Encoding.UTF8);
				m_StreamWriter = new StreamWriter(m_Stream, Encoding.UTF8);
				Run();
				return true;
			}
			catch(Exception ex)
			{
				Debug.WriteLine(ex.Message);//debug writes to vs
			}

			return false;
		}

		public void Run()
		{
            UdpProcessSeverResponse();
			
			//listen to the serverTcpClient
			Thread TcpThread = new Thread(new ThreadStart(TcpProcessServerResponse));
			TcpThread.Name = "TCP Thread";
			TcpThread.Start();
									
		}

		private void TcpProcessServerResponse()
		{
			//listen to incoming msg from the sever
			try
			{
				while(m_TcpClient.Connected)// while we are connected to the server
				{
					string message = m_StreamReader.ReadLine();

					Packet? packet = Packet.Deserialize(message);

					if (packet != null)
					{
						HandleTCPPacket(packet);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		private void HandleTCPPacket(Packet packet)
		{
            switch (packet.Type)
            {
                case PacketType.ENCRYPTEDPACKET:
                    EncryptedPacket encryptedPacket = (EncryptedPacket)packet;
					Packet? decryptedPacket = DecryptPacket(encryptedPacket.m_encryptedData);

                    if (decryptedPacket == null) return;

                    HandleTCPPacket(decryptedPacket);

                    break;
                case PacketType.MESSAGEPACKET:
                    //read msg and print to debug
                    MessagePacket mp = (MessagePacket)packet;
                    if (mp != null)
                    {
                        Debug.WriteLine($"Message: {mp.m_message}");
                    }
                    break;

                case PacketType.POSITIONPACKET:
                    //update pos of indexed paddle
                    PositionPacket pp = (PositionPacket)packet;
                    if (pp != null)
                    {
                        m_positionUpdate = new Vector2(pp.X, pp.Y);
                    }
                    break;

                case PacketType.LOGINPACKET:
                    LoginPacket lp = (LoginPacket)packet;
                    m_index = lp.m_index;					
                    m_serverPublicKey = lp.m_key;
					m_lobbyNumber = lp.m_lobbyNumber;
                    //Debug.WriteLine(m_index.ToString());
                    LoginPacket UdpLogin = new LoginPacket(m_index);
					UdpSendMessage(UdpLogin);
                    break;

                case PacketType.BALLPACKET:
                    //update pos of indexed paddle
                    BallPacket bp = (BallPacket)packet;
                    if (bp != null)
                    {
                        m_ballPositionUpdate = new Vector2(bp.X, bp.Y);
                    }
                    break;

                case PacketType.SCOREPACKET:
                    ScorePacket sp = (ScorePacket)packet;
                    m_playerOneScore = sp.m_playerOneScore;
                    m_playerTwoScore = sp.m_playerTwoScore;
                    break;

				case PacketType.TIMERPACKET:
					TimerPacket tp = (TimerPacket)packet;
					m_gameTimer = tp.m_gameTimer;
					m_gameRestartTimer = tp.m_restartTimer;
					break;

				case PacketType.LOBBYPACKET:
					LobbyPacket lobbyP = (LobbyPacket)packet;
					m_lobbyNumber = lobbyP.m_lobbyNumber;
					m_playerNumber = lobbyP.m_playerNumber;
					m_lobbyReady = lobbyP.m_lobbyReady;
					break;

				case PacketType.SERVERSTATUSPACKET:
					ServerStatusPacket ssp = (ServerStatusPacket)packet;
					m_lobbyReady = ssp.m_online;
					break;

				case PacketType.GAMESTARTPACKET:
					GameStartPacket gsp = (GameStartPacket)packet;
					m_gameStart = gsp.m_startGame;
					//m_countDown = 3;
					break;

				case PacketType.GAMECOUNTDOWN:
					GameCountdownPacket gcp = (GameCountdownPacket)packet;
					m_countDown = gcp.m_countdown;
					break;

                default:
                    Debug.WriteLine($"Packet type invaild: NM! {packet.Type}");
                    break;
            }
        }

		public void TCPSendMessage(Packet? packet, bool encrypted = true)
		{
			string? packetToSend = packet.Serialize();	

			if(encrypted)
			{
				packetToSend = new EncryptedPacket(m_index, EncyrptPacket(packet)).Serialize();
			}

			m_StreamWriter.WriteLine(packetToSend);					
			m_StreamWriter.Flush();
		}

		public void Login()
		{
			LoginPacket loginPacket = new LoginPacket(m_index, m_publicKey, m_lobbyNumber);
			//MessagePacket messagePacket = new MessagePacket("Client " + m_index + " - login Attempt");
			Debug.WriteLine(m_index.ToString());
			//UdpSendMessage(messagePacket);
            TCPSendMessage(loginPacket, false);
		}

		async Task UdpProcessSeverResponse()
		{
			try
			{
				while(m_TcpClient.Connected)
				{
					UdpReceiveResult receiveResult = await m_UdpClient.ReceiveAsync();
					byte[] receivedData = receiveResult.Buffer;

                    string message = Encoding.UTF8.GetString(receivedData, 0, receivedData.Length);
                    Packet? packet = Packet.Deserialize(message);

                    switch (packet.Type)
					{
						case PacketType.MESSAGEPACKET:
                            MessagePacket mp = (MessagePacket)packet;
                            Debug.WriteLine("UDP msg Recieved: " + mp.m_message);
                            break;
						case PacketType.GAMESTATEPACKET:
							GameStatePacket gsp = (GameStatePacket)packet;							
							m_gameState = gsp.m_gameState;
							m_gameWinner = gsp.m_winnerState;							
							break;

						default: break;
					}
                   

					//Console.WriteLine("UDP Msg Received: " + message);
				}
			}
			catch(SocketException e)
			{
				Console.WriteLine("UDP Client Read Err" + e.Message);
			}
		}

		public void UdpSendMessage(Packet packet)
		{
			string? packetToSend = packet.Serialize();

			byte[] bytes = Encoding.UTF8.GetBytes(packetToSend);
			m_UdpClient.Send(bytes, bytes.Length);
		}

		public byte[] EncyrptPacket(Packet? packet)
		{
			lock(m_RSAProvider)
			{
				m_RSAProvider.ImportParameters(m_serverPublicKey); //sets the key to the servers
				string json = packet.Serialize(); 
				byte[] encrypted = m_RSAProvider.Encrypt(Encoding.UTF8.GetBytes(json), false); //encrypt into a byte array
				return encrypted;
			}
		}

		public Packet DecryptPacket(byte[] data)
		{
			lock(m_RSAProvider)
			{
				m_RSAProvider.ImportParameters(m_privateKey); //sets the parameters to the private key of the client
				byte[] decyrpted = m_RSAProvider.Decrypt(data, false); //decyrpt the byte array
				string json = Encoding.UTF8.GetString(decyrpted); //get the string
				Packet packet = Packet.Deserialize(json);//deserialise into a packet
				return packet;
			}
		}

	}
}
