using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ET
{
    public struct MessageInfo
    {
        public ActorId ActorId;
        public MessageObject MessageObject;
    }
    
    public class MessageQueue: Singleton<MessageQueue>, ISingletonAwake
    {
        public void Awake()
        {
        }

        public bool Send(ActorId actorId, MessageObject messageObject)
        {
            return this.Send(actorId.Address, actorId, messageObject);
        }
        
        public void Reply(ActorId actorId, MessageObject messageObject)
        {
            this.Send(actorId.Address, actorId, messageObject);
        }
        
        public bool Send(Address from, ActorId actorId, MessageObject messageObject)
        {
            Fiber fiber = FiberManager.Instance.Get(actorId.Fiber);
            if (fiber == null)
            {
                throw new Exception($"MessageQueue.Send error: from={from.Fiber} to={actorId.Fiber}");
            }
            return fiber.Send(from, actorId, messageObject);
        }
        
        public void Fetch(Fiber fiber, int count, List<MessageInfo> list)
        {
            for (int i = 0; i < count; ++i)
            {
                if (!fiber.msgs.TryDequeue(out MessageInfo message))
                {
                    break;
                }
                list.Add(message);
            }
        }
    }
}