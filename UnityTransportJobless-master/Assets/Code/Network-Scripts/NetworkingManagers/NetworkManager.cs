using System.Collections.Generic;
using Unity.Networking.Transport;
using Assets.Code;

public static class NetworkManager
{
    public static void SendMessage(NetworkDriver networkDriver, MessageHeader message, NetworkConnection id)
    {
        var writer = networkDriver.BeginSend(id);
        message.SerializeObject(ref writer);
        networkDriver.EndSend(writer);
    }

    public static MessageHeader ReadMessage<T>(DataStreamReader reader, Queue<MessageHeader> messageQueue) where T : MessageHeader, new()
    {
        var msg = new T();
        msg.DeserializeObject(ref reader);
        messageQueue.Enqueue(msg);
        return msg;
    }
}

