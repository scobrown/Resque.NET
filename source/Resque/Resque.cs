using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Resque
{
    public class Resque
    {
        private readonly Dictionary<string, Queue> _queues = new Dictionary<string, Queue>();
        public IJobCreator JobCreator { get; set; }
        public IFailureService FailureService { get; set; }
        public IRedis Client { get; set; }
        protected List<Worker> Workers { get; private set; }

        public IEnumerable<string> Queues {get { return Client.SMembers("queue:*"); }} 

        public Resque(IJobCreator jobCreator, IFailureService failureService, IRedis client)
        {
            JobCreator = jobCreator;
            FailureService = failureService;
            Client = client;
            Workers = new List<Worker>();
        }
        public void StopAll()
        {
            foreach (var worker in Workers)
            {
                worker.Shutdown = true;
            }
        }
        public void Push(string queue, string job, params string[] args)
        {
            GetQueue(queue).Push(new QueuedItem()
                                     {
                                         @class = job,
                                         args = args
                                     });
        }
        public void Work(params string[] queues)
        {
            var worker = new Worker(JobCreator, FailureService, Client, queues);
            Workers.Add(worker);
            worker.Work();
        }
        public System.Threading.Tasks.Task WorkAsync(params string[] queues)
        {
            var worker = new Worker(JobCreator, FailureService, Client, queues);
            Workers.Add(worker);
            return System.Threading.Tasks.Task.Factory.StartNew(() => worker.Work());
        }
        private Queue GetQueue(string name)
        {
            if (_queues.ContainsKey(name))
                return _queues[name];
            var queue = new Queue(Client, name);
            _queues.Add(name, queue);
            return queue;
        }
    }
}
