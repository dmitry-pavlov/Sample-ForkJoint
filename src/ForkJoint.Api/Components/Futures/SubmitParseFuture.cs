using System;
using System.Collections.Generic;
using System.Linq;
using Automatonymous;
using ForkJoint.Contracts;
using MassTransit;
using MassTransit.Futures;

namespace ForkJoint.Api.Components.Futures
{
    public class SubmitParseFuture :
        Future<SubmitParse, SubmitParseCompleted, SubmitParseFaulted >
    {
        private Guid _chainId;

        public SubmitParseFuture ()
        {
            ConfigureCommand(x => x.CorrelateById(context =>
            {
                _chainId = context.Message.ChainId;

                return context.Message.OrderId;
            }));

            SendRequests<Parse, ParseFile>(x => x.Parses, x =>
                {
                    x.UsingRequestInitializer(context => MapParseFile(context));
                    x.TrackPendingRequest(message => message.OrderLineId);
                })
                .OnResponseReceived<ParseCompleted>(x => x.CompletePendingRequest(message => message.OrderLineId));

            WhenAllCompleted(r => r.SetCompletedUsingInitializer(context => new
            {
                LinesCompleted = context.Instance.Results.Select(x => x.Value.ToObject<ParseCompleted>()).ToDictionary(x => x.OrderLineId),
            }));

            WhenEnter(Completed, binder => binder.Publish(TestSignal));

            WhenAnyFaulted(f => f.SetFaultedUsingInitializer(context => MapParseFaulted(context)));
        }

        private TriggerChain TestSignal(ConsumeEventContext<FutureState> context)
        {
            var testSignal = new TestSignal
            {
                ChainId = _chainId // we have it in context - figure out how to get it from instance results
            };
            return testSignal;
        }



        static object MapParseFile(FutureConsumeContext<Parse> context)
        {
            return new
            {
                OrderId = context.Instance.CorrelationId,
                OrderLineId = context.Message.ParseId,
                File = context.Message
            };
        }

        static object MapParseFaulted(FutureConsumeContext context)
        {
            Dictionary<Guid, Fault> faults = context.Instance.Faults.ToDictionary(x => x.Key, x => x.Value.ToObject<Fault>());

            return new
            {
                LinesCompleted = context.Instance.Results.ToDictionary(x => x.Key, x => x.Value.ToObject<ParseCompleted>()),
                LinesFaulted = faults,
                Exceptions = faults.SelectMany(x => x.Value.Exceptions).ToArray()
            };
        }
    }
}