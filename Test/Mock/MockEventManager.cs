using System;
using System.Collections.Generic;

namespace Test.Mock
{
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;
    using Kwetterprise.EventSourcing.Client.Interface;
    using Kwetterprise.EventSourcing.Client.Local;
    using Kwetterprise.EventSourcing.Client.Models.Event;

    public class MockEventManager : IEventPublisher, IEventListener
    {
        private readonly LocalEventManager eventManager = new LocalEventManager();

        public Queue<EventBase> Events { get; } = new Queue<EventBase>();

        public MockEventManager()
        {
            // var allTopics = Enum.GetValues(typeof(Topic)).Cast<Topic>().ToList();
            this.eventManager.StartListening();
        }

        public Task<IList<EventBase>> WaitForEvents(int count, TimeSpan? timeout)
        {
            return this.eventManager.Buffer(count).Take(1).Timeout(timeout ?? TimeSpan.MaxValue).ToTask();
        }

        public Task<T> WaitOne<T>(TimeSpan? timeout)
            where T : EventBase
        {
            return this.eventManager.Take(1).Cast<T>().Timeout(timeout ?? TimeSpan.MaxValue).ToTask();
        }

        public Task Publish<T>(T message, Topic topic)
            where T : EventBase
        {
            this.Events.Enqueue(message);
            return this.eventManager.Publish(message, topic);
        }

        public IDisposable Subscribe(IObserver<EventBase> observer)
        {
            return this.eventManager.Subscribe(observer);
        }

        public void StartListening()
        {
            this.eventManager.StartListening();
        }

        public Task Stop()
        {
            return this.eventManager.Stop();
        }

        public void Dispose()
        {
            this.eventManager.Dispose();
        }
    }
}
