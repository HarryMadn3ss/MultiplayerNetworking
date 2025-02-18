﻿using Microsoft.Xna.Framework;
using Multiplayer_Games_Programming_Framework;
using System;
using System.Collections.Generic;
using nkast.Aether.Physics2D.Dynamics;
using Multiplayer_Games_Programming_Framework.Core;
using System.Data;
using System.Diagnostics;
using Multiplayer_Games_Programming_Framework.GameCode.Components;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Encodings.Web;
using Multiplayer_Games_Programming_Packet_Library;

namespace Multiplayer_Games_Programming_Framework
{
	internal class GameScene : Scene
	{
		List<GameObject> m_GameObjects = new();

		SpriteBatch m_spriteBatch;
		SpriteFont m_font;
				

		BallGO m_Ball;
		PaddleGO m_PlayerPaddle;
		public PaddleGO m_RemotePaddle;

		BallControllerComponent m_BallController;

		Random m_Random = new Random();
		
		GameModeState m_GameModeState;

		float m_GameTimer;
		float m_GameRestartTimer;

		enum WinnerState
		{
			NONE,
			PLAYERONE,
			PLAYERTWO,
			DRAW,
		}

		WinnerState m_winningPlayer;

		public bool m_gameStateFlag;

		public void SetGameState(int state) { m_GameModeState = (GameModeState)state; }
		public void SetWinnerState(int state) { m_winningPlayer = (WinnerState)state; }


		public GameScene(SceneManager manager) : base(manager)
		{
			m_GameModeState = GameModeState.AWAKE;
			GameStatePacket gameStatePacket = new GameStatePacket(NetworkManager.m_Instance.m_lobbyNumber, NetworkManager.m_Instance.m_playerNumber, (int)m_GameModeState);
			NetworkManager.m_Instance.UdpSendMessage(gameStatePacket);

			
			
			//m_GraphicsDevice = new GraphicsDeviceManager(this);
        }

		public override void LoadContent()
		{
			base.LoadContent();

			//txt
            m_spriteBatch = GetSpriteBatch();
			
			m_font =  GetContentManager().Load<SpriteFont>("font");

			m_gameStateFlag = false;

            float screenWidth = Constants.m_ScreenWidth;
			float screenHeight = Constants.m_ScreenHeight;

			//m_Ball = GameObject.Instantiate<BallGO>(this, new Transform(new Vector2(screenWidth / 2, screenHeight / 2), new Vector2(1, 1), 0));
			//m_BallController = m_Ball.GetComponent<BallControllerComponent>();

			if (NetworkManager.m_Instance.m_playerNumber == 1)
			{
				m_PlayerPaddle = GameObject.Instantiate<PaddleGO>(this, new Transform(new Vector2(100, 500), new Vector2(5, 20), 0));
				m_PlayerPaddle.AddComponent(new PaddleController(m_PlayerPaddle , 0));

				m_RemotePaddle = GameObject.Instantiate<PaddleGO>(this, new Transform(new Vector2(screenWidth - 100, 500), new Vector2(5, 20), 0));
				m_RemotePaddle.AddComponent(new PaddleNetworkController(m_RemotePaddle, 1));
			}
			else
			{
				m_RemotePaddle = GameObject.Instantiate<PaddleGO>(this, new Transform(new Vector2(100, 500), new Vector2(5, 20), 0));
				m_RemotePaddle.AddComponent(new PaddleNetworkController(m_RemotePaddle, 0));

				m_PlayerPaddle = GameObject.Instantiate<PaddleGO>(this, new Transform(new Vector2(screenWidth - 100, 500), new Vector2(5, 20), 0));
				m_PlayerPaddle.AddComponent(new PaddleController(m_PlayerPaddle, 0));
			}

			//Border
			Vector2[] wallPos = new Vector2[]
			{

				new Vector2(screenWidth/2, 0), //top
				new Vector2(screenWidth, screenHeight/2), //right
				new Vector2(screenWidth/2, screenHeight), //bottom
				new Vector2(0, screenHeight/2) //left
			};

			Vector2[] wallScales = new Vector2[]
			{
				new Vector2(screenWidth / 10, 10), //top
				new Vector2(10, screenHeight/10), //right
				new Vector2(screenWidth/10, 10), //bottom
				new Vector2(10, screenHeight/10) //left
			};

			
			

			for (int i = 0; i < 4; i++)
			{
				GameObject go = GameObject.Instantiate<GameObject>(this, new Transform(wallPos[i], wallScales[i], 0));
				SpriteRenderer sr = go.AddComponent(new SpriteRenderer(go, "Square(10x10)"));
				Rigidbody rb = go.AddComponent(new Rigidbody(go, BodyType.Static, 10, sr.m_Size / 2));
				rb.CreateRectangle(sr.m_Size.X, sr.m_Size.Y, 0.0f, 1.0f, Vector2.Zero, Constants.GetCategoryByName("Wall"), Constants.GetCategoryByName("All"));
				go.AddComponent(new ChangeColourOnCollision(go, Color.Red));
				
				if(i == 1)
				{
					go.AddComponent(new ScoreColliderRight(go));
				}
				if(i == 3)
				{
					//left
					go.AddComponent(new ScoreColliderLeft(go));
				}

				m_GameObjects.Add(go);
			}
		}

		protected override string SceneName()
		{
			return "GameScene";
		}

		protected override World CreateWorld()
		{
			return new World(Constants.m_Gravity);
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);			

			if(m_GameModeState == GameModeState.PLAYING)
			{				
				m_GameTimer += deltaTime;
				if(NetworkManager.m_Instance.m_playerNumber == 1)
				{
					TimerPacket timerPacket = new TimerPacket(NetworkManager.m_Instance.m_lobbyNumber, m_GameTimer, m_GameRestartTimer);
					NetworkManager.m_Instance.TCPSendMessage(timerPacket); 
				}
				else
				{
					m_GameTimer = NetworkManager.m_Instance.m_gameTimer;
				}               
            }
            if (m_GameModeState == GameModeState.ENDING)
			{
				m_GameRestartTimer -= deltaTime;
				if (NetworkManager.m_Instance.m_playerNumber == 1)
				{					
					TimerPacket timerPacket = new TimerPacket(NetworkManager.m_Instance.m_lobbyNumber, m_GameTimer, m_GameRestartTimer);
					NetworkManager.m_Instance.TCPSendMessage(timerPacket);					
				}
				else
				{
					m_GameRestartTimer = NetworkManager.m_Instance.m_gameRestartTimer;
				}
			}

			switch (m_GameModeState)
			{
				case GameModeState.AWAKE:
                    NetworkManager.m_Instance.m_playerOneScore = 0;
                    NetworkManager.m_Instance.m_playerTwoScore = 0;

                    m_Ball = GameObject.Instantiate<BallGO>(this, new Transform(new Vector2(Constants.m_ScreenWidth / 2, Constants.m_ScreenHeight / 2), new Vector2(1, 1), 0));
					m_BallController = m_Ball.GetComponent<BallControllerComponent>();

                    if (NetworkManager.m_Instance.m_playerNumber == 1)
					{						
						m_GameTimer = 0;
                        m_GameRestartTimer = 30;
						m_GameModeState = GameModeState.STARTING;
                        GameStatePacket gameAwakePacket = new GameStatePacket(NetworkManager.m_Instance.m_lobbyNumber, NetworkManager.m_Instance.m_playerNumber, (int)m_GameModeState);
						NetworkManager.m_Instance.UdpSendMessage(gameAwakePacket);						
					}
					else
					{
						m_GameModeState = (GameModeState)NetworkManager.m_Instance.m_gameState;
						m_winningPlayer = (WinnerState)NetworkManager.m_Instance.m_gameWinner;
                        //m_GameTimer = NetworkManager.m_Instance.m_gameTimer;                        
                    }
                    break;

				case GameModeState.STARTING:

                    if (NetworkManager.m_Instance.m_playerNumber == 1)
					{
						m_BallController.Init(10, new Vector2((float)m_Random.NextDouble(), (float)m_Random.NextDouble()));
						m_BallController.StartBall();					
						m_GameModeState = GameModeState.PLAYING;
                        GameStatePacket gamePlayingPacket = new GameStatePacket(NetworkManager.m_Instance.m_lobbyNumber, NetworkManager.m_Instance.m_playerNumber, (int)m_GameModeState);
						NetworkManager.m_Instance.UdpSendMessage(gamePlayingPacket);
					}
                    else
                    {
                        m_GameModeState = (GameModeState)NetworkManager.m_Instance.m_gameState;
                        m_winningPlayer = (WinnerState)NetworkManager.m_Instance.m_gameWinner;
                    }


                    break;

				case GameModeState.PLAYING:

					if(NetworkManager.m_Instance.m_playerOneScore > 7)
					{						
						if(NetworkManager.m_Instance.m_playerNumber == 1)
						{
							m_Ball.Destroy();
                            //player one wins
                            m_winningPlayer = WinnerState.PLAYERONE;
                            m_GameModeState = GameModeState.ENDING;
                            GameStatePacket gameEndingPacket = new GameStatePacket(NetworkManager.m_Instance.m_lobbyNumber, NetworkManager.m_Instance.m_playerNumber, (int)m_GameModeState, (int)m_winningPlayer);
							NetworkManager.m_Instance.UdpSendMessage(gameEndingPacket);
                            BallPacket packet = new BallPacket(NetworkManager.m_Instance.m_lobbyNumber, NetworkManager.m_Instance.m_playerNumber, -100, -100);
                            NetworkManager.m_Instance.TCPSendMessage(packet);
                        }
                        else
                        {
                            m_GameModeState = (GameModeState)NetworkManager.m_Instance.m_gameState;
                            m_winningPlayer = (WinnerState)NetworkManager.m_Instance.m_gameWinner;
                        }

                    }
					else if(NetworkManager.m_Instance.m_playerTwoScore >  7)
					{
                        
                        if (NetworkManager.m_Instance.m_playerNumber == 1)
						{
							m_Ball.Destroy();
							//player two wins
							m_winningPlayer = WinnerState.PLAYERTWO;
							m_GameModeState = GameModeState.ENDING;
                            GameStatePacket gameEndingPacket = new GameStatePacket(NetworkManager.m_Instance.m_lobbyNumber, NetworkManager.m_Instance.m_playerNumber, (int)m_GameModeState, (int)m_winningPlayer);
							NetworkManager.m_Instance.UdpSendMessage(gameEndingPacket);
                            BallPacket packet = new BallPacket(NetworkManager.m_Instance.m_lobbyNumber, NetworkManager.m_Instance.m_playerNumber, -100, -100);
                            NetworkManager.m_Instance.TCPSendMessage(packet);
                        }
                        else
                        {
                            m_GameModeState = (GameModeState)NetworkManager.m_Instance.m_gameState;
                            m_winningPlayer = (WinnerState)NetworkManager.m_Instance.m_gameWinner;
                        }
                    }
					else if (m_GameTimer > 90)
					{
                       
                        if (NetworkManager.m_Instance.m_playerNumber == 1)
						{
                            //draw
							m_Ball.Destroy();
                            m_winningPlayer = WinnerState.DRAW;
                            m_GameModeState = GameModeState.ENDING;
                            GameStatePacket gameEndingPacket = new GameStatePacket(NetworkManager.m_Instance.m_lobbyNumber, NetworkManager.m_Instance.m_playerNumber, (int)m_GameModeState, (int)m_winningPlayer);
							NetworkManager.m_Instance.UdpSendMessage(gameEndingPacket);
                            BallPacket packet = new BallPacket(NetworkManager.m_Instance.m_lobbyNumber, NetworkManager.m_Instance.m_playerNumber, -100, -100);
                            NetworkManager.m_Instance.TCPSendMessage(packet);
                        }
                        else
                        {							
							m_GameModeState = (GameModeState)NetworkManager.m_Instance.m_gameState;
							m_winningPlayer = (WinnerState)NetworkManager.m_Instance.m_gameWinner;						
                        }
                    }
					break;
				case GameModeState.ENDING:
					Debug.WriteLine("Game Over");

					//if (NetworkManager.m_Instance.m_index == 0)
					//{
					//	TimerPacket timerPacket = new TimerPacket(0, 30);
					//	NetworkManager.m_Instance.TCPSendMessage(timerPacket, false);                        
     //               }
					//else
					//{
     //                   m_GameRestartTimer = NetworkManager.m_Instance.m_gameRestartTimer;
     //               }

                    if (m_GameRestartTimer < 0)
					{						
                        if (NetworkManager.m_Instance.m_playerNumber == 1)
						{
                            m_GameModeState = GameModeState.AWAKE;
                            GameStatePacket gameRestartPacket = new GameStatePacket(NetworkManager.m_Instance.m_lobbyNumber, NetworkManager.m_Instance.m_playerNumber, (int)m_GameModeState);
							NetworkManager.m_Instance.UdpSendMessage(gameRestartPacket);
						}
                        else
                        {							
							m_GameModeState = (GameModeState)NetworkManager.m_Instance.m_gameState;
                            m_winningPlayer = (WinnerState)NetworkManager.m_Instance.m_gameWinner;                                    
                        }
                    }
					break;
				default:
					break;
			}
		}

		public override void Draw(float deltaTime)
		{
			base.Draw(deltaTime);			
			m_spriteBatch.DrawString(m_font, "Player One: " + NetworkManager.m_Instance.m_playerOneScore, new Vector2(100, 10), Color.CornflowerBlue, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1);
			m_spriteBatch.DrawString(m_font, "Player Two: " + NetworkManager.m_Instance.m_playerTwoScore, new Vector2(450, 10), Color.CornflowerBlue, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1);
			m_spriteBatch.DrawString(m_font, Math.Round(90 - m_GameTimer).ToString(), new Vector2(Constants.m_ScreenWidth / 2, 0), Color.Red, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1);

			if(m_GameModeState == GameModeState.ENDING)
			{
				switch(m_winningPlayer)
				{
					case WinnerState.PLAYERONE:
				        m_spriteBatch.DrawString(m_font, "Player One Wins!!!", new Vector2(Constants.m_ScreenWidth / 2, Constants.m_ScreenHeight / 2), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1);

				        break;
					case WinnerState.PLAYERTWO:
				        m_spriteBatch.DrawString(m_font, "Player Two Wins!!!", new Vector2(Constants.m_ScreenWidth / 2, Constants.m_ScreenHeight / 2), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1);

				        break;
					case WinnerState.DRAW:
				        m_spriteBatch.DrawString(m_font, "Draw", new Vector2(Constants.m_ScreenWidth / 2, Constants.m_ScreenHeight / 2), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1);
				        break;

					default:
						break;
				}
                m_spriteBatch.DrawString(m_font, "Game Restarting in: " + Math.Round(m_GameRestartTimer) , new Vector2(Constants.m_ScreenWidth / 2, Constants.m_ScreenHeight / 2 + 20), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1);

            }

        }
	}
}