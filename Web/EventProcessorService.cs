using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web
{
    using System.Threading;
    using Business.EventProcessor;
    using Kwetterprise.EventSourcing.Client.Interface;
    using Microsoft.Extensions.Hosting;

    public class EventProcessorService : IHostedService
    {
        private readonly ITweetEventProcessor eventProcessor;
        private readonly IEventListener eventListener;

        public EventProcessorService(ITweetEventProcessor eventProcessor, IEventListener eventListener)
        {
            this.eventProcessor = eventProcessor;
            this.eventListener = eventListener;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.eventListener.Subscribe(e => this.eventProcessor.Process(e));
            this.eventListener.StartListening();

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAny(this.eventListener.Stop(), Task.Delay(TimeSpan.FromSeconds(30), cancellationToken));
        }
    }
}
