using Microsoft.Xna.Framework;
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
			PLAYERONE,
			PLAYERTWO,
			DRAW,
		}

		WinnerState m_winningPlayer;

		public GameScene(SceneManager manager) : base(manager)
		{
			m_GameModeState = GameModeState.AWAKE;

			
			//m_GraphicsDevice = new GraphicsDeviceManager(this);
        }

		public override void LoadContent()
		{
			base.LoadContent();

			//txt
            m_spriteBatch = GetSpriteBatch();
			
			m_font =  GetContentManager().Load<SpriteFont>("font");



            float screenWidth = Constants.m_ScreenWidth;
			float screenHeight = Constants.m_ScreenHeight;

			//m_Ball = GameObject.Instantiate<BallGO>(this, new Transform(new Vector2(screenWidth / 2, screenHeight / 2), new Vector2(1, 1), 0));
			//m_BallController = m_Ball.GetComponent<BallControllerComponent>();

			if (NetworkManager.m_Instance.m_index == 0)
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
			}
			if(m_GameModeState == GameModeState.ENDING)
			{
				m_GameRestartTimer -= deltaTime;
			}

			switch (m_GameModeState)
			{
				case GameModeState.AWAKE:
                    NetworkManager.m_Instance.m_playerOneScore = 0;
                    NetworkManager.m_Instance.m_playerTwoScore = 0;
					m_GameTimer = 0;
					m_GameRestartTimer = 5;
                    m_Ball = GameObject.Instantiate<BallGO>(this, new Transform(new Vector2(Constants.m_ScreenWidth / 2, Constants.m_ScreenHeight / 2), new Vector2(1, 1), 0));
                    m_BallController = m_Ball.GetComponent<BallControllerComponent>();
                    m_GameModeState = GameModeState.STARTING;
                    break;

				case GameModeState.STARTING:
					m_BallController.Init(10, new Vector2((float)m_Random.NextDouble(), (float)m_Random.NextDouble()));
					m_BallController.StartBall();					
                    m_GameModeState = GameModeState.PLAYING;

					break;

				case GameModeState.PLAYING:					
					if(NetworkManager.m_Instance.m_playerOneScore > 7)
					{
						//player one wins
						m_winningPlayer = WinnerState.PLAYERONE;
                        m_GameModeState = GameModeState.ENDING;
                        m_Ball.Destroy();
                    }
					else if(NetworkManager.m_Instance.m_playerTwoScore >  7)
					{
                        //player two wins
                        m_winningPlayer = WinnerState.PLAYERTWO;
                        m_GameModeState = GameModeState.ENDING;
                        m_Ball.Destroy();
                    }
					else if (m_GameTimer < 0)
					{
                        //draw
                        m_winningPlayer = WinnerState.DRAW;
                        m_GameModeState = GameModeState.ENDING;
                        m_Ball.Destroy();
                    }
					break;
				case GameModeState.ENDING:
					Debug.WriteLine("Game Over");
					if(m_GameRestartTimer < 0)
					{
						m_GameModeState = GameModeState.AWAKE;
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
			m_spriteBatch.DrawString(m_font, "Player Two: " + NetworkManager.m_Instance.m_playerTwoScore, new Vector2(400, 10), Color.CornflowerBlue, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1);
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