using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;

namespace Resque
{
    public interface IWorker
    {
        string RedisId { get; }
        bool Pause { get; set; }
        bool Shutdown { get; set; }
        void Work(int interval = 5);
    }

    public class Worker : IWorker
    {
        private readonly string _dnsName = Dns.GetHostName();
        private readonly int _threadId = Thread.CurrentThread.ManagedThreadId;

        public IJobCreator JobCreator { get; private set; }
        public IFailureService FailureService { get; set; }
        public IRedis Client { get; set; }
        protected string[] Queues { get; set; }
        public bool Pause { get; set; }
        public bool Shutdown { get; set; }

        public string RedisId
        {
            get
            {
                return String.Format("{0}:{1}:{2}", _dnsName, _threadId, String.Join(",", Queues));
            }
        }

        public Worker(IJobCreator locator, IFailureService failureService, IRedis client, params string[] queues)
        {
            JobCreator = locator;
            FailureService = failureService;
            Client = client;

            if(queues == null || queues.Length == 0)
                throw new ArgumentException("Invalid Queues");

            if (queues.Length == 1 && queues[0] == "*")
                Queues = new string[] {"high"};//Resque.Queues();
            else
                Queues = queues;
        }


        public void Work(int interval = 5)
        {
            try
            {
                Startup();
                while (true)
                {
                    if (Shutdown)
                        break;
                    if (Pause)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                        continue;
                    }

                    var job = Reserve();
                    if (job != null)
                    {
                        SetWorkingOn(job);

                        System.Threading.Tasks.Task.Factory.StartNew(() => Process(job)).Wait();

                        DoneWorking();
                    }
                    else
                    {
                        if (interval == 0)
                            break;
                        Thread.Sleep(TimeSpan.FromSeconds(interval));
                    }
                }
            }
            finally
            {
                UnregisterWorker();
            }
        }

        private void Startup()
        {
            RegisterWorker();
        }

        private void RegisterWorker()
        {
            Client.SAdd("resque:workers", RedisId);
        }

        private void UnregisterWorker()
        {
            Client.SRemove("resque:workers", RedisId);
            Client.RemoveKeys(String.Format("resque:worker:{0}", RedisId));
            Client.RemoveKeys(String.Format("resque:worker:{0}:started", RedisId));
        }

        private void Process(IJob job)
        {
            try
            {
                job.Perform();
            }
            catch (Exception ex)
            {
                try
                {
                    job.Failed(ex);
                }
                catch (Exception)
                {
                }
                SetFailed();
            }
        }

        private void DoneWorking()
        {
            Client.RemoveKeys(string.Format("resque:worker:{0}", RedisId));
        }

        private void SetFailed()
        {

        }

        private void SetWorkingOn(IJob job)
        {
            var data = new
                           {
                               queue = job.Queue,
                               run_at = DateTime.Now.ToString("ddd MMM dd hh:mm:ss zzzz yyyy"),
                               payload = job.Payload
                           };
            Client.Set(string.Format("resque:worker:{0}", RedisId), JsonConvert.SerializeObject(data));
        }

        public IJob Reserve()
        {
            foreach (var queue in Queues)
            {
                var queued = Client.LPop(queue);
                if (!string.IsNullOrEmpty(queued))
                    return JobCreator.CreateJob(FailureService,
                                            this,
                                            JsonConvert.DeserializeObject<QueuedItem>(queued), queue);
            }
            return null;
        }
    }
}