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
	}

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


	public class Message : Packet
	{
		[JsonPropertyName("Message")]
		public string m_message;

		public Message() 
		{ 
			Type = PacketType.MESSAGEPACKET;
		}

		public Message(string message)
		{
			Type = PacketType.MESSAGEPACKET;
			m_message = message;
		}
	}

	public class Position : Packet
	{

		[JsonPropertyName("PositionX")]
		public float X { get; set; }

		[JsonPropertyName("PositionY")]
		public float Y { get; set; }

		public Position()
		{
			Type = PacketType.POSITIONPACKET;
		}

		public Position(float x, float y)
		{
			Type = PacketType.POSITIONPACKET;	
			X = x;
			Y = y;
		}

	}

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
						return JsonSerializer.Deserialize<Message>(root.GetRawText(), options);
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