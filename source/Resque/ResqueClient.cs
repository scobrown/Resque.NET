using System.Collections.Concurrent;

namespace Resque
{
    public class ResqueClient
    {
        private readonly ConcurrentDictionary<string, Queue> _queues = new ConcurrentDictionary<string, Queue>();
        public IRedis Client { get; set; }

        public ResqueClient(IRedis client)
        {
            Client = client;
        }
        public void Push(string queue, string job, params string[] args)
        {
            GetQueue(queue).Push(new QueuedItem()
                                     {
                                         @class = job,
                                         args = args
                                     });
        }        
        private Queue GetQueue(string name)
        {
            return _queues.GetOrAdd(name, n => new Queue(Client, n));
        }
    }
}
