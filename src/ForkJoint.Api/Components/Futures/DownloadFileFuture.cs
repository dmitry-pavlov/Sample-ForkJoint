using ForkJoint.Contracts;
using MassTransit;
using MassTransit.Futures;

namespace ForkJoint.Api.Components.Futures
{
    public class DownloadFileFuture :
        Future<DownloadFile, DownloadCompleted>
    {
        public DownloadFileFuture()
        {
            ConfigureCommand(x => x.CorrelateById(context => context.Message.OrderLineId));

            SendRequest<HandleDownload>()
                .OnResponseReceived<DownloadReady>(x =>
                {
                    x.SetCompletedUsingInitializer(context => new {Description = $"Downloaded {context.Message.Size} bytes"});
                });
        }
    }
}