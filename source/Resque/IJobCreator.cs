namespace Resque
{
    public interface IJobCreator
    {
        IJob CreateJob(IFailureService failureService, Worker worker, QueuedItem deserializedObject, string queue);
    }
}