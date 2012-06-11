using System;
using System.Linq;
using Newtonsoft.Json;

namespace Resque
{
    public class MultiQueue
    {
        public IRedis Client { get; private set; }
        public string[] Names { get; private set; }
        public string[] RedisNames { get; private set; }

        public MultiQueue(IRedis client, string[] names)
        {
            Client = client;
            Names = names;
            RedisNames = names.Select(x => string.Format("queue:{0}", x)).ToArray();
        }

        public Tuple<string, QueuedItem> Pop()
        {
            var queued = Client.BLPop(RedisNames);
            if (queued != null && !string.IsNullOrEmpty(queued.Item2))
                return new Tuple<string, QueuedItem>(queued.Item1, JsonConvert.DeserializeObject<QueuedItem>(queued.Item2));

            return null;
        }

    }
}