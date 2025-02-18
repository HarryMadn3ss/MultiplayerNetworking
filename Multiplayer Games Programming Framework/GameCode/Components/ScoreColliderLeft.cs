﻿using nkast.Aether.Physics2D.Dynamics.Contacts;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Multiplayer_Games_Programming_Packet_Library;
using Multiplayer_Games_Programming_Framework.Core;

namespace Multiplayer_Games_Programming_Framework.GameCode.Components
{
    internal class ScoreColliderLeft : Component
    {


        public ScoreColliderLeft(GameObject gameObject) : base(gameObject)
        {

        }

        protected override void OnCollisionEnter(Fixture sender, Fixture other, Contact contact)
        {
            if(NetworkManager.m_Instance.m_playerNumber == 1)
            {
                ScorePacket _scorePacket = new ScorePacket(NetworkManager.m_Instance.m_lobbyNumber, NetworkManager.m_Instance.m_playerNumber);
                NetworkManager.m_Instance.TCPSendMessage(_scorePacket);
            }
        }
    }
}
