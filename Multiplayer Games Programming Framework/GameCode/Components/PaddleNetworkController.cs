using Microsoft.Xna.Framework;
using Multiplayer_Games_Programming_Framework.Core;
using Multiplayer_Games_Programming_Packet_Library;
using System.Diagnostics;

namespace Multiplayer_Games_Programming_Framework.GameCode.Components
{
	internal class PaddleNetworkController : Component
	{
		int m_Index;
		Rigidbody m_Rigidbody;

		public PaddleNetworkController(GameObject gameObject, int index) : base(gameObject)
		{
			m_Index = index;
		}

		protected override void Start(float deltaTime)
		{
			m_Rigidbody = m_GameObject.GetComponent<Rigidbody>();
		}

		public void UpdatePosition(Vector2 pos)
		{
			m_Rigidbody.UpdatePosition(pos);

			//packet
			PositionPacket packet = new PositionPacket(m_Index, pos.X, pos.Y);
			NetworkManager.m_Instance.TCPSendMessage(packet);
			
			Debug.WriteLine(packet.X.ToString() + packet.Y.ToString());			
			
		}

        protected override void Update(float deltaTime)
        {
			//monogame checks if there is a update that is overriding then calls thme
			//need to update postion with the new position
			UpdatePosition();

            base.Update(deltaTime);
        }
    }
}
