using System;

namespace Resque
{
    public interface IJob
    {
        IFailureService FailureService { get; }
        QueuedItem Payload { get; }
        string Queue { get; }
        IWorker Worker { get; }
        
        void Perform();

        void Failed(Exception exception);
    }
}