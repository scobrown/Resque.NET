using System;
using Resque.FailureBackend;

namespace Resque
{
    public interface IFailureService
    {
        void Create(QueuedItem payload, Exception exception, IWorker worker, string queue);
    }

    public class FailureService : IFailureService
    {
        public IBackendFactory Backend { get; set; }

        public FailureService(IBackendFactory backend)
        {
            Backend = backend;
        }
        public void Create(QueuedItem payload, Exception exception, IWorker worker, string queue)
        {
            Backend.Create(payload, exception, worker, queue).Save();
        }
    }
}