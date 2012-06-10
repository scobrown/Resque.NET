using System;
using Moq;
using NUnit.Framework;
using Resque;

namespace Test.Resque
{
    [TestFixture]
    public class WorkerTests
    {
        [Test]
        public void calling_Reserve_should_get_a_job()
        {
            var queuedItem = new QueuedItem();

            var jobCreatorMock = new Mock<IJobCreator>();
            var failureServiceMock = new Mock<IFailureService>();
            var redisMock = new Mock<IRedis>();
            jobCreatorMock
                .Setup(x => x.CreateJob(It.IsAny<IFailureService>(), It.IsAny<Worker>(), It.IsAny<QueuedItem>(), It.IsAny<string>()))
                .Returns(new TestJob());
            redisMock.Setup(x => x.LPop(It.IsAny<string>())).Returns(queuedItem.ToJson());

            var underTest = new Worker(jobCreatorMock.Object, failureServiceMock.Object, redisMock.Object, "");
            var job = underTest.Reserve();

            Assert.That(job, Is.Not.Null);
        }
    }

    public class TestJob : IJob
    {
        public IFailureService FailureService { get; set; }

        public QueuedItem Payload { get; set; }

        public string Queue { get; set; }

        public IWorker Worker { get; set; }

        public void Perform()
        {
            throw new NotImplementedException();
        }

        public void Failed(Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}
