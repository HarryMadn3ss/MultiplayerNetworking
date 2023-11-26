using Myra;
using Myra.Graphics2D.UI;
using nkast.Aether.Physics2D.Dynamics;
using Multiplayer_Games_Programming_Framework.Core;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using System;
using Multiplayer_Games_Programming_Packet_Library;

namespace Multiplayer_Games_Programming_Framework
{
	internal class LobbyScene : Scene
	{
		private Desktop m_Desktop;

		bool m_safeGuard = false;

		float m_timer = 3;

        public LobbyScene(SceneManager manager) : base(manager)
		{
			manager.m_Game.IsMouseVisible = true;
		}

		protected override World CreateWorld()
		{
			return null;
		}

		protected override string SceneName()
		{
			return "Lobby Menu";
		}

		public override void LoadContent()
		{
			MyraEnvironment.Game = m_Manager.m_Game;            

            var grid = new Grid
			{
				ShowGridLines = false,
				RowSpacing = 8,
				ColumnSpacing = 8
			};

			int cols = 4;
			for(int i = 0; i < cols; ++i)
			{
				grid.ColumnsProportions.Add(new Proportion(ProportionType.Part));
			}

			int rows = 5;
			for (int i = 0; i < rows; ++i)
			{
				grid.RowsProportions.Add(new Proportion(ProportionType.Part));
			}

			m_Desktop = new Desktop();
			m_Desktop.Root = grid;

            var waitTxt = new TextBox();
            waitTxt.Text = "Waiting for Player...";
            waitTxt.GridRow = 2;
            waitTxt.GridColumn = 1;
            waitTxt.GridColumnSpan = 2;
            waitTxt.HorizontalAlignment = HorizontalAlignment.Center;
            waitTxt.VerticalAlignment = VerticalAlignment.Center;
            waitTxt.Width = (Constants.m_ScreenWidth / cols) * waitTxt.GridColumnSpan;
            waitTxt.Height = (Constants.m_ScreenHeight / rows) * waitTxt.GridRowSpan;
			waitTxt.Background = null;
            waitTxt.AcceptsKeyboardFocus = false;
            grid.Widgets.Add(waitTxt);

			if (NetworkManager.m_Instance.m_playerNumber == 1)
			{
				var PlayButton = new TextButton();

				PlayButton.Text = "Play";
				PlayButton.GridRow = 3;
				PlayButton.GridColumn = 1;
				PlayButton.GridColumnSpan = 2;
				PlayButton.HorizontalAlignment = HorizontalAlignment.Center;
				PlayButton.VerticalAlignment = VerticalAlignment.Center;
				PlayButton.Width = (Constants.m_ScreenWidth / cols);
				PlayButton.Height = (Constants.m_ScreenHeight / rows);
				PlayButton.Enabled = false;
				grid.Widgets.Add(PlayButton);
				
				PlayButton.Click += (s, a) =>
				{
					//m_Manager.LoadScene(new GameScene(m_Manager));
					GameStartPacket gameStartPacket = new GameStartPacket(true, NetworkManager.m_Instance.m_index);
					NetworkManager.m_Instance.TCPSendMessage(gameStartPacket, false);
				};
				
				
				//lock behind the player count
				if(NetworkManager.m_Instance.m_lobbyReady)
				{
					PlayButton.Enabled = true;
					
				}				
			}
			else if(NetworkManager.m_Instance.m_playerNumber != 1 && !NetworkManager.m_Instance.m_gameStart)
			{
                var waitForHostTxt = new TextBox();
                waitForHostTxt.Text = "Waiting for Host!";
                waitForHostTxt.GridRow = 2;
                waitForHostTxt.GridColumn = 1;
                waitForHostTxt.GridColumnSpan = 2;
                waitForHostTxt.HorizontalAlignment = HorizontalAlignment.Center;
                waitForHostTxt.VerticalAlignment = VerticalAlignment.Center;
                waitForHostTxt.Width = (Constants.m_ScreenWidth / cols) * waitTxt.GridColumnSpan;
                waitForHostTxt.Height = (Constants.m_ScreenHeight / rows) * waitTxt.GridRowSpan;
                waitForHostTxt.Background = null;
				waitForHostTxt.AcceptsKeyboardFocus = false;
                grid.Widgets.Add(waitForHostTxt);
            }		

            
            if (NetworkManager.m_Instance.m_lobbyReady)
            {                
                waitTxt.Visible = false;
            }
            else
            {
                waitTxt.Visible = true;
            }



            var childPanel = new Panel();
			childPanel.GridColumn = 0;
			childPanel.GridRow = 0;
			
			grid.Widgets.Add(childPanel);

			
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (NetworkManager.m_Instance.m_lobbyReady && m_safeGuard == false)
			{
				LoadContent();
				m_safeGuard = true;
			}
			if (NetworkManager.m_Instance.m_gameStart)
			{
				m_Manager.LoadScene(new GameScene(m_Manager));
			}						
        }


        public override void Draw(float deltaTime)
		{			
			base.Draw(deltaTime);
			m_Desktop.Render();
			
        }
	}
}
