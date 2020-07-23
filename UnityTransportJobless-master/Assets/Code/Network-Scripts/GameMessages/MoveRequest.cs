using UnityEngine;
using System;
using Unity.Networking.Transport;

namespace Assets.Code
{
    public class MoveRequest : MessageHeader
    {
        public Direction direction;

        public override MessageType Type => MessageType.MoveRequest;

        public override void SerializeObject(ref DataStreamWriter writer)
        {
            base.SerializeObject(ref writer);
            writer.WriteInt((int)direction);
        }

        public override void DeserializeObject(ref DataStreamReader reader)
        {
            base.DeserializeObject(ref reader);
            direction = (Direction)reader.ReadInt();
        }
    }
}
[Flags]
public enum Direction
{
    North,
    East,
    South,
    West
}

