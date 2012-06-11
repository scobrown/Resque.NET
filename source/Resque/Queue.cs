using System;
using Newtonsoft.Json;

namespace Resque
{
    public class Queue
    {
        public IJobCreator JobCreator { get; set; }
        public IFailureService FailureService { get; set; }
        public IRedis Client { get; set; }
        public string Name { get; set; }
        private string RedisName { get; set; }

        public Queue(IRedis client, string name)
        {
            Client = client;
            Name = name;
            RedisName = string.Format("queue:{0}", name);

            Client.SAdd("queues", RedisName);
        }
        public Tuple<string, QueuedItem> Pop()
        {
            var queued = Client.BLPop(new[] {RedisName});
            if (queued != null && !string.IsNullOrEmpty(queued.Item2))
                return new Tuple<string, QueuedItem>(queued.Item1, JsonConvert.DeserializeObject<QueuedItem>(queued.Item2));

            return null;
        }

        public void Push(QueuedItem item)
        {
            Client.RPush(RedisName, item.ToJson());
        }
    }
}