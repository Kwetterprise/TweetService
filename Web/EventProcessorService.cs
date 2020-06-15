using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Web
{
    using System.Threading;
    using Business.EventProcessor;
    using Kwetterprise.EventSourcing.Client.Interface;
    using Microsoft.Extensions.Hosting;

    public class EventProcessorService : IHostedService
    {
        private readonly ILogger<EventProcessorService> logger;
        private readonly ITweetEventProcessor eventProcessor;
        private readonly IEventListener eventListener;

        public EventProcessorService(ILogger<EventProcessorService> logger, ITweetEventProcessor eventProcessor, IEventListener eventListener)
        {
            this.logger = logger;
            this.eventProcessor = eventProcessor;
            this.eventListener = eventListener;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting event processor.");

            this.eventListener.Subscribe(e => this.eventProcessor.Process(e));
            this.eventListener.StartListening();

            this.logger.LogInformation("Started event processor.");

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Stopping event processor.");
            await Task.WhenAny(this.eventListener.Stop(), Task.Delay(TimeSpan.FromSeconds(30), cancellationToken));
            this.logger.LogInformation("Stopped event processor.");
        }
    }
}
