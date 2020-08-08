using System.Collections.Generic;
using Unity.Networking.Transport;

namespace Assets.Code
{
	public class EndGameMessage : MessageHeader
	{
		public override MessageType Type => MessageType.EndGame;
		public byte numberOfScores;
		public List<int> playerID = new List<int>();
		public List<ushort> highScorePairs = new List<ushort>();
		public override void SerializeObject(ref DataStreamWriter writer)
		{
			base.SerializeObject(ref writer);

			writer.WriteByte(numberOfScores);

            for (int i = 0; i == numberOfScores; i++)
            {
				writer.WriteInt(playerID[i]);
				writer.WriteUShort(highScorePairs[i]);
            }
		}
		public override void DeserializeObject(ref DataStreamReader reader)
		{
			base.DeserializeObject(ref reader);

			numberOfScores = reader.ReadByte();

            for (int i = 0; i < numberOfScores; i++)
            {
				playerID.Add(reader.ReadInt());
				highScorePairs.Add(reader.ReadUShort());
            }
		}
	}
}