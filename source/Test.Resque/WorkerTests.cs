using System;
using Moq;
using NUnit.Framework;
using Resque;

namespace Test.Resque
{
    [TestFixture]
    public class WorkerTests
    {
        class TestPackage
        {
            public Mock<IJobCreator> JobCreatorMock = new Mock<IJobCreator>();
            public Mock<IFailureService> FailureServiceMock = new Mock<IFailureService>();
            public Mock<IRedis> RedisMock = new Mock<IRedis>();

            public QueuedItem QueuedItem = new QueuedItem();
            public Worker UnderTest;

            public TestPackage(string[] queues)
            {
                UnderTest = new Worker(JobCreatorMock.Object, FailureServiceMock.Object, RedisMock.Object, queues);

                JobCreatorMock
                    .Setup(x => x.CreateJob(It.IsAny<IFailureService>(), It.IsAny<Worker>(), It.IsAny<QueuedItem>(), It.IsAny<string>()))
                    .Returns(new TestPackage.TestJob());
                RedisMock.Setup(x => x.BLPop(It.IsAny<string[]>(), It.IsAny<int>())).Returns(new Tuple<string, string>("queue", QueuedItem.ToJson()));

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
        [Test]
        public void calling_Reserve_should_get_a_job()
        {
            var package = new TestPackage(new[] {""});

            var job = package.UnderTest.Reserve();

            Assert.That(job, Is.Not.Null);
        }
        [Test]
        public void calling_Reserve_with_2_queues_should_call_BLPop_with_2_queues()
        {
            var package = new TestPackage(new[] { "queue1", "queue2" });

            var job = package.UnderTest.Reserve();

            package.RedisMock.Verify(x => x.BLPop(new[] { "queue:queue1", "queue:queue2" }, It.IsAny<int>()));
        }
        [Test]
        public void calling_Reserve_with_splat_should_call_SMembers()
        {
            var package = new TestPackage(new[] { "*" });
            package.RedisMock.Setup(x => x.SMembers("queues")).Returns(new[] {"RandomQueue"})
                .Verifiable();

            var job = package.UnderTest.Reserve();

            package.RedisMock.Verify();
        }
    }

}
