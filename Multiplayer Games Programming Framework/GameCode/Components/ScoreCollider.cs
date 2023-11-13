using nkast.Aether.Physics2D.Dynamics.Contacts;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Multiplayer_Games_Programming_Packet_Library;

namespace Multiplayer_Games_Programming_Framework.GameCode.Components
{
    internal class ScoreCollider : Component
    {


        public ScoreCollider(GameObject gameObject) : base(gameObject)
        {

        }

        protected override void OnCollisionEnter(Fixture sender, Fixture other, Contact contact)
        {
            ScorePacket _scorePacket = new ScorePacket();
        }
    }
}
