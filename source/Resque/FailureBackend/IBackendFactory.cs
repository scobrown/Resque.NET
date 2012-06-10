using System;

namespace Resque.FailureBackend
{
    public interface IBackendFactory
    {
        RedisBackend Create(QueuedItem payload, Exception exception, IWorker worker, string queue);
    }
}