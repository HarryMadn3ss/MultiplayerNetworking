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
					Debug.WriteLine(message);
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
			MessagePacket message = new MessagePacket("Hello Sever, My name is Bob");
			//string msgToSend = message.Serialize();

			TCPSendMessage(message);
		}
	}
}
