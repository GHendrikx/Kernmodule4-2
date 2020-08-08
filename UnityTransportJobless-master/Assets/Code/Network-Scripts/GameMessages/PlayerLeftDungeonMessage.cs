using Unity.Networking.Transport;

namespace Assets.Code
{
	public class PlayerLeftDungeonMessage : MessageHeader
	{
		public override MessageType Type => MessageType.PlayerLeftDungeon;
		public int playerID;
		public override void SerializeObject(ref DataStreamWriter writer)
		{
			base.SerializeObject(ref writer);
		}
		public override void DeserializeObject(ref DataStreamReader reader)
		{
			base.DeserializeObject(ref reader);
		}
	}
}