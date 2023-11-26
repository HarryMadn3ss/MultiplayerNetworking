using System.Net;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Multiplayer_Games_Programming_Packet_Library
{
	public enum PacketType
	{
		MESSAGEPACKET,
		POSITIONPACKET,
		LOGINPACKET,
		BALLPACKET,
		SCOREPACKET,
        ENCRYPTEDPACKET,
		GAMESTATEPACKET,
        TIMERPACKET,
        LOBBYPACKET,
        SERVERSTATUSPACKET,
        GAMESTARTPACKET,
        GAMECOUNTDOWN
    }

	[Serializable]
	public class Packet
	{
		[JsonPropertyName("Type")]
		public PacketType Type { get; set; }

		public string Serialize()
		{
			var options = new JsonSerializerOptions
			{ 
				Converters = { new PacketConverter() },
				IncludeFields = true,
			};

			return JsonSerializer.Serialize(this, options);
		}

		public static Packet? Deserialize(string json)
		{
			var options = new JsonSerializerOptions
			{
				Converters = { new PacketConverter() },
				IncludeFields = true,
			};

			return JsonSerializer.Deserialize<Packet>(json, options);
		}		
	}

	[Serializable]
	public class MessagePacket : Packet
	{
		[JsonPropertyName("Message")]
		public string? m_message;

		public MessagePacket() 
		{ 
			Type = PacketType.MESSAGEPACKET;
			m_message = "Not Initialized";
		}

		public MessagePacket(string message)
		{
			Type = PacketType.MESSAGEPACKET;
			m_message = message;
		}
    }

	[Serializable]
	public class PositionPacket : Packet
	{
		[JsonPropertyName("Index")]
		public int Index { get; set; }

		[JsonPropertyName("PositionX")]
		public float X { get; set; }

		[JsonPropertyName("PositionY")]
		public float Y { get; set; }

		public PositionPacket()
		{
			Type = PacketType.POSITIONPACKET;
			Index = int.MaxValue;
			X = float.MaxValue;
			Y = float.MaxValue;
		}

		public PositionPacket(int index, float x, float y)
		{
			Type = PacketType.POSITIONPACKET;
			Index = index;
			X = x;
			Y = y;
		}        

    }
	[Serializable]
    public class LoginPacket : Packet
    {
		[JsonPropertyName("Index")]
		public int m_index { get; set; }

        [JsonPropertyName("Key")]
        public RSAParameters m_key { get; set; }

        [JsonPropertyName("LobbyNumber")]
        public int m_lobbyNumber { get; set; }

        public LoginPacket()
        {
            Type = PacketType.LOGINPACKET;
			m_index = int.MaxValue;
			m_key = new RSAParameters();
            m_lobbyNumber = int.MaxValue;
        }
        public LoginPacket(int index)
        {
            Type = PacketType.LOGINPACKET;
            m_index = index;
        }

        public LoginPacket(int index, RSAParameters key, int lobbyNumber)
        {
            Type = PacketType.LOGINPACKET;
            m_index = index;
			m_key = key;
            m_lobbyNumber = lobbyNumber;
        }
    }

    [Serializable]
    public class BallPacket : Packet
    {    
        [JsonPropertyName("PositionX")]
        public float X { get; set; }

        [JsonPropertyName("PositionY")]
        public float Y { get; set; }

        public BallPacket()
        {
            Type = PacketType.BALLPACKET;
            X = float.MaxValue;
            Y = float.MaxValue;
        }

        public BallPacket(float x, float y)
        {
            Type = PacketType.BALLPACKET;            
            X = x;
            Y = y;
        }

    }
    [Serializable]
    public class ScorePacket : Packet
    {
        [JsonPropertyName("Index")]
        public int? m_index{ get; set; }

        [JsonPropertyName("PlayerOneScore")]
        public int m_playerOneScore{ get; set; }

        [JsonPropertyName("PlayerTwoScore")]
        public int m_playerTwoScore{ get; set; }

        public ScorePacket()
        {
            Type = PacketType.SCOREPACKET;
            m_index = int.MaxValue;
        }

        public ScorePacket(int index)
        {
            Type = PacketType.SCOREPACKET;
            m_index = index;
        }

		public ScorePacket(int playerOne, int playerTwo)
		{
			Type = PacketType.SCOREPACKET;
			m_playerOneScore = playerOne;
			m_playerTwoScore = playerTwo;
		}
    }

	[Serializable]
	public class EncryptedPacket : Packet
	{
		[JsonPropertyName("Index")]
		public int? m_index { get; set; }

        [JsonPropertyName("Data")]
		public byte[] m_encryptedData { get; set; }

        public EncryptedPacket()
		{
			Type = PacketType.ENCRYPTEDPACKET;
			m_index = int.MaxValue;
			m_encryptedData = new byte[0];
		}

		public EncryptedPacket(int index, byte[] encryptedData)
		{
			Type = PacketType.ENCRYPTEDPACKET;
			m_encryptedData = encryptedData;
			m_index = index;
		}
	}

    [Serializable]
    public class GameStatePacket : Packet
    {
        [JsonPropertyName("Index")]
        public int m_index{ get; set; }

		[JsonPropertyName("Game State")]
        public int m_gameState { get; set; }      

		[JsonPropertyName("Winner")]
		public int m_winnerState { get; set; }


        public GameStatePacket()
        {
            Type = PacketType.GAMESTATEPACKET;
            m_index = int.MaxValue;
            m_gameState = int.MaxValue;
        }

        public GameStatePacket(int index, int gameState)
        {
            Type = PacketType.GAMESTATEPACKET;
            m_gameState = gameState;
            m_index = index;
        }
        public GameStatePacket(int index, int gameState, int winnerState)
        {
            Type = PacketType.GAMESTATEPACKET;
            m_index = index;
            m_gameState = gameState;
			m_winnerState = winnerState;
        }
    }

    [Serializable]
    public class TimerPacket : Packet
    {
        [JsonPropertyName("GameTimer")]
        public float m_gameTimer;

        [JsonPropertyName("RestartTimer")]
        public float m_restartTimer;        

        public TimerPacket()
        {
            Type = PacketType.TIMERPACKET;
            m_gameTimer = float.MaxValue;
            m_restartTimer = float.MaxValue;           
        }

        public TimerPacket(float gameTimer, float restarttimer)
        {
            Type = PacketType.TIMERPACKET;
            m_gameTimer = gameTimer;
            m_restartTimer = restarttimer; 
        }       
    }

    [Serializable]
    public class LobbyPacket : Packet
    {
        [JsonPropertyName("LobbyNumber")]
        public int m_lobbyNumber { get; set; }

        [JsonPropertyName("LobbyReady")]
        public bool m_lobbyReady { get; set;}

        [JsonPropertyName("PlayerNumber")]
        public int m_playerNumber;

        public LobbyPacket()
        {
            Type = PacketType.LOBBYPACKET;
            m_lobbyNumber = int.MaxValue;
            m_lobbyReady = false;
            m_playerNumber = int.MaxValue;
        }
        public LobbyPacket(bool ready)
        {
            Type = PacketType.LOBBYPACKET;
            m_lobbyReady = ready;           
        }

        public LobbyPacket(int lobbynumber, int playerNumber)
        {
            Type = PacketType.LOBBYPACKET;
            m_lobbyNumber = lobbynumber;           
            m_playerNumber = playerNumber;
        }

    }

    [Serializable]
    public class ServerStatusPacket : Packet
    {
        [JsonPropertyName("ServerNumber")]
        public int m_serverNumber;

        [JsonPropertyName("Status")]
        public bool m_online { get; set; }

        public ServerStatusPacket()
        {
            Type = PacketType.SERVERSTATUSPACKET;
            m_serverNumber = int.MaxValue;
            m_online = false;
        }

        public ServerStatusPacket(int serverNumber)
        {
            Type = PacketType.SERVERSTATUSPACKET;
            m_serverNumber = serverNumber;
        }

        public ServerStatusPacket(int serverNumber, bool online)
        {
            Type = PacketType.SERVERSTATUSPACKET;
            m_serverNumber = serverNumber;
            m_online = online;
        }
    }

    [Serializable]
    public class GameStartPacket : Packet
    {
        [JsonPropertyName("GameStart")]
        public bool m_startGame { get; set; }

        [JsonPropertyName("LobbyNumber")]
        public int m_lobbyNumber { get; set; }

        public GameStartPacket()
        {
            Type = PacketType.GAMESTARTPACKET;
            m_startGame = false;
            m_lobbyNumber = int.MaxValue;
        }
        public GameStartPacket(bool startGame, int lobbyNumber)
        {
            Type = PacketType.GAMESTARTPACKET;
            m_startGame = true;
            m_lobbyNumber = m_lobbyNumber;
        }
    }

    [Serializable]
    public class GameCountdownPacket : Packet
    {
        [JsonPropertyName("GameCountdown")]
        public float m_countdown { get; set; }

        [JsonPropertyName("LobbyNumber")]
        public int m_lobbyNumber { get; set; }

        public GameCountdownPacket()
        {
            Type = PacketType.TIMERPACKET;
            m_countdown = float.MaxValue;
        }

        public GameCountdownPacket(int lobbyNumber, float countdown)
        {
            Type = PacketType.TIMERPACKET;
            m_lobbyNumber = lobbyNumber;
            m_countdown = countdown;
        }
    }



    [Serializable]
	public class PacketConverter : JsonConverter<Packet> 
	{
        public override Packet? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
			using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
			{
				var root = doc.RootElement;
				if(root.TryGetProperty("Type", out var typeProperty))
				{
					if(typeProperty.GetByte() == (byte)PacketType.MESSAGEPACKET)
					{
						return JsonSerializer.Deserialize<MessagePacket>(root.GetRawText(), options);
					}
                    if (typeProperty.GetByte() == (byte)PacketType.POSITIONPACKET)
                    {
                        return JsonSerializer.Deserialize<PositionPacket>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.LOGINPACKET)
                    {
                        return JsonSerializer.Deserialize<LoginPacket>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.BALLPACKET)
                    {
                        return JsonSerializer.Deserialize<BallPacket>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.SCOREPACKET)
                    {
                        return JsonSerializer.Deserialize<ScorePacket>(root.GetRawText(), options);
                    }
					if (typeProperty.GetByte() == (byte)PacketType.ENCRYPTEDPACKET)
					{
						return JsonSerializer.Deserialize<EncryptedPacket>(root.GetRawText(), options);
					}
                    if (typeProperty.GetByte() == (byte)PacketType.GAMESTATEPACKET)
                    {
                        return JsonSerializer.Deserialize<GameStatePacket>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.TIMERPACKET)
                    {
                        return JsonSerializer.Deserialize<TimerPacket>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.LOBBYPACKET)
                    {
                        return JsonSerializer.Deserialize<LobbyPacket>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.SERVERSTATUSPACKET)
                    {
                        return JsonSerializer.Deserialize<ServerStatusPacket>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.GAMESTARTPACKET)
                    {
                        return JsonSerializer.Deserialize<GameStartPacket>(root.GetRawText(), options);
                    }
                    if (typeProperty.GetByte() == (byte)PacketType.GAMECOUNTDOWN)
                    {
                        return JsonSerializer.Deserialize<GameCountdownPacket>(root.GetRawText(), options);
                    }
                }
			}

				throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Packet value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}