using Unity.Networking.Transport;
namespace Assets.Code
{
	public class PlayerEnterRoomMessage : MessageHeader
	{
		public override MessageType Type => MessageType.PlayerEnterRoom;
		public int PlayerID;

		public override void SerializeObject(ref DataStreamWriter writer)
		{
			base.SerializeObject(ref writer);
			writer.WriteInt(PlayerID);
		}
		public override void DeserializeObject(ref DataStreamReader reader)
		{
			base.DeserializeObject(ref reader);
			PlayerID = reader.ReadInt();
		}
	}
}