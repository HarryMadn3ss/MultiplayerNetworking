using System;
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
		NetworkStream m_Stream;
		StreamReader m_StreamReader;
		StreamWriter m_StreamWriter;

		public int m_index;

		PaddleNetworkController m_Controller;

		//events
		//public dictoionary < int, Action<vector2> m_playerpostions


		NetworkManager()
		{
			m_TcpClient = new TcpClient();
		}

		public bool Connect(string ip, int port)
		{
			try
			{
				m_TcpClient.Connect(ip, port);
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
						switch (packet.Type)
						{
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
								if(pp != null)
								{
									//update postion
									m_Controller.UpdatePosition(new Vector2(pp.X, pp.Y));
								}
								break;

							case PacketType.LOGINPACKET:
								//save assigned index
								//put an if statemnet in the game scene to set the controls for the correct paddle
								LoginPacket lp = (LoginPacket)packet;
								//if (lp != null)
								//{
									m_index = lp.m_index;
								//}
								
								break;

							default:
								Debug.WriteLine($"Packet type invaild: NM! {packet.Type}");
								break;
						}
					}

				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		public void TCPSendMessage(Packet? packet)
		{
			string? packetToSend = packet.Serialize();	


			m_StreamWriter.WriteLine(packetToSend);					
			m_StreamWriter.Flush();
		}

		public void Login()
		{
			LoginPacket message = new LoginPacket(m_index);
			//string msgToSend = message.Serialize();

			TCPSendMessage(message);
		}
	}
}
