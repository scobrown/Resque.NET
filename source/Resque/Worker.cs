using System;
using System.Collections.Generic;
using System.Linq;
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
        protected string[] Queues { get; private set; }
        protected string[] RedisQueues
        {
            get
            {
                var queues = new List<string>();
                if(Queues.Contains("*"))
                    queues.AddRange(Client.SMembers("queues"));

                return Queues.Distinct().OrderBy(x => x).ToArray();
            }
        }
        public bool Pause { get; set; }
        public bool Shutdown { get; set; }

        public WorkerState State { get { return Client.Exists("worker:" + RedisId) ? WorkerState.Working : WorkerState.Idle; }}
        public bool Idle { get { return State == WorkerState.Idle; } }
        public bool Working { get { return State == WorkerState.Working; } }

        public DateTime Started { get { return DateTime.Parse(Client.Get(string.Format("worker:{0}:started", RedisId))); } }

        public string RedisId
        {
            get
            {
                return String.Format("{0}:{1}:{2}", _dnsName, _threadId, string.Join(",", Queues));
            }
        }

        public Worker(IJobCreator locator, IFailureService failureService, IRedis client, params string[] queues)
        {
            JobCreator = locator;
            FailureService = failureService;
            Client = client;

            if(queues == null || queues.Length == 0)
                throw new ArgumentException("Invalid Queues");

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
            Client.SAdd("workers", RedisId);
            Client.Set(string.Format("worker:{0}:started", RedisId), DateTime.Now.ToString("yyyy MMM dd HH:mm:ss zzzz"));
        }

        private void UnregisterWorker()
        {
            Client.SRemove("workers", RedisId);
            Client.RemoveKeys(String.Format("worker:{0}", RedisId));
            Client.RemoveKeys(String.Format("worker:{0}:started", RedisId));
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
            Client.RemoveKeys(string.Format("worker:{0}", RedisId));
        }

        private void SetFailed()
        {

        }

        private void SetWorkingOn(IJob job)
        {
            var data = new
                           {
                               queue = job.Queue,
                               run_at = DateTime.Now.ToString("yyyy MMM dd HH:mm:ss zzzz"),
                               payload = job.Payload
                           };
            Client.Set(string.Format("worker:{0}", RedisId), JsonConvert.SerializeObject(data));
        }

        public IJob Reserve()
        {
            var multi = new MultiQueue(Client, RedisQueues);
            var item = multi.Pop();
            if (item != null)
            {
                var queueName = GetQueueName(item.Item1);
                return JobCreator.CreateJob(FailureService, this, item.Item2, queueName);
            }
            return null;
        }

        private string GetQueueName(string item)
        {
            const string queueSubString = "queue:";
            var index = item.IndexOf(queueSubString);
            if(index == -1 ) return item;
            return item.Substring(index + queueSubString.Length);
        }
    }

    public enum WorkerState
    {
        Working,
        Idle
    }
}