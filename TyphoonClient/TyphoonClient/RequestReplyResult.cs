
using Waters.Control.Client;

public class RequestReplyResult
{
    public string ReplyId { get; set; }
    public byte[] MessageData { get; set; }

    public T Message<T>() where T : new()
    {
        return MessageSerializer.Deserialize<T>(MessageData);
    }
}
