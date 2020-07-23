using Unity.Networking.Transport;

namespace Assets.Code
{
    public class RequestDeniedMessage : MessageHeader
    {
        public override MessageType Type => MessageType.RequestDenied;
        public uint requestDenied;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteUInt(requestDenied);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            requestDenied = reader.ReadUInt();
        }
    }
}