using Google.Apis.Gmail.v1.Data;

namespace core;

public class MessageBatch
{

    #region Constructors

    public MessageBatch(IEnumerable<Message> messages)
    {
        MessageIds = messages.Select(m => m.Id).ToArray();
    }

    #endregion
    
    #region Properties

    public string[] MessageIds { get; }

    #endregion
    
}
