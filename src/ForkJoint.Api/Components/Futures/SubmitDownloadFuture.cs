using System.Security.Cryptography.X509Certificates;
using Automatonymous;

namespace ForkJoint.Api.Components.Futures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using MassTransit;
    using MassTransit.Futures;


    public class SubmitDownloadFuture :
        Future<SubmitDownload, SubmitDownloadCompleted, SubmitDownloadFaulted >
    {
        private Guid _chainId;

        public SubmitDownloadFuture()
        {
            ConfigureCommand(x => x.CorrelateById(context =>
            {
                _chainId = context.Message.ChainId;

                return context.Message.OrderId;
            }));

            SendRequests<Download, DownloadFile>(x => x.Files, x =>
                {
                    x.UsingRequestInitializer(context => MapDownloadFile(context));
                    x.TrackPendingRequest(message => message.OrderLineId);
                })
                .OnResponseReceived<DownloadCompleted>(x => x.CompletePendingRequest(message => message.OrderLineId));
                
            WhenAllCompleted(r =>
            {
                r.SetCompletedUsingInitializer(context => new
                {
                    LinesCompleted = context.Instance.Results.Select(x => x.Value.ToObject<DownloadCompleted>())
                        .ToDictionary(x => x.OrderLineId),
                });
            });

            WhenEnter(Completed, binder => binder.Publish(TestSignal));

            WhenAnyFaulted(f => f.SetFaultedUsingInitializer(context => MapDownloadFaulted(context)));
        }

        private TriggerChain TestSignal(ConsumeEventContext<FutureState> context)
        {
            var testSignal = new TestSignal
            {
                ChainId = _chainId // we have it in context - figure out how to get it from instance results
            };
            return testSignal;
        }

        static object MapDownloadFile(FutureConsumeContext<Download> context)
        {
            return new
            {
                OrderId = context.Instance.CorrelationId,
                OrderLineId = context.Message.DownloadId,
                File = context.Message,
                FileName = context.Message.FileName
            };
        }

        static object MapDownloadFaulted(FutureConsumeContext context)
        {
            Dictionary<Guid, Fault> faults = context.Instance.Faults.ToDictionary(x => x.Key, x => x.Value.ToObject<Fault>());

            return new
            {
                LinesCompleted = context.Instance.Results.ToDictionary(x => x.Key, x => x.Value.ToObject<DownloadCompleted>()),
                LinesFaulted = faults,
                Exceptions = faults.SelectMany(x => x.Value.Exceptions).ToArray()
            };
        }
    }
}
