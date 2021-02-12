using ForkJoint.Contracts;
using MassTransit;
using MassTransit.Futures;

namespace ForkJoint.Api.Components.Futures
{
    public class ParseFileFuture :
        Future<ParseFile, ParseCompleted>
    {
        public ParseFileFuture()
        {
            ConfigureCommand(x => x.CorrelateById(context => context.Message.OrderLineId));

            SendRequest<HandleParse>()
                .OnResponseReceived<ParseReady>(x =>
                {
                    x.SetCompletedUsingInitializer(context => new {Description = $"Parsed {context.Message.Products} products"});
                });
        }
    }
}