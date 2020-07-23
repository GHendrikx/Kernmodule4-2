using Unity.Networking.Transport;

namespace Assets.Code
{
	public class HitMonsterMessage : MessageHeader
	{
		public override MessageType Type => MessageType.HitMonster;
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