using System;
using Newtonsoft.Json;

namespace Resque.FailureBackend
{
    public class RedisBackendFactory : IBackendFactory
    {
        public IRedis RedisClient { get; set; }

        public RedisBackendFactory(IRedis redisClient)
        {
            RedisClient = redisClient;
        }

        public RedisBackend Create(QueuedItem payload, Exception exception, IWorker worker, string queue)
        {
            return new RedisBackend(RedisClient, payload, exception, worker, queue);
        }
    }

    public class RedisBackend
    {
        public IRedis RedisClient { get; set; }
        public QueuedItem Payload { get; set; }
        public Exception Exception { get; set; }
        public IWorker Worker { get; set; }
        public string Queue { get; set; }

        public RedisBackend(IRedis redisClient, QueuedItem payload, Exception exception, IWorker worker, string queue)
        {
            RedisClient = redisClient;
            Payload = payload;
            Exception = exception;
            Worker = worker;
            Queue = queue;
        }

        public void Save()
        {
            var data = new
                           {
                               failed_at = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss zzz"),
                               payload = Payload,
                               exception = Exception.GetType().Name,
                               error = Exception.Message,
                               backtrace = new[]{Exception.StackTrace},
                               worker = Worker.RedisId,
                               queue = Queue
                           };

            RedisClient.RPush("failed", JsonConvert.SerializeObject(data));
        }
    }
 }