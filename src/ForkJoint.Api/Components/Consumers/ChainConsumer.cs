using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ForkJoint.Api.Components.Futures;
using ForkJoint.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ForkJoint.Api.Components.Consumers
{
    public class ChainConsumer :
        IConsumer<TestSignal>,
        IConsumer<TriggerChain>,
        IConsumer<HandleDownload>,
        IConsumer<HandleParse>
    {
        private readonly ILogger<ChainConsumer> _logger;
        private readonly IRequestClient<SubmitDownload> _downloadClient;
        private readonly IRequestClient<SubmitParse> _parseClient;
        private static readonly List<Chain> Chains = new();

        public ChainConsumer(ILogger<ChainConsumer> logger, 
            IRequestClient<SubmitDownload> downloadClient, 
            IRequestClient<SubmitParse> parseClient
            )
        {
            _logger = logger;
            _downloadClient = downloadClient;
            _parseClient = parseClient;
        }

        private class SubmitDownloadRequest : SubmitDownload
        {
            public Guid ChainId { get; set; }
            public Guid OrderId { get; set; }
            public Download[] Files { get; set; }
        }

        private class SubmitParseRequest : SubmitParse
        {
            public Guid ChainId { get; set; }
            public Guid OrderId { get; set; }
            public Parse[] Parses { get; set; }
        }

        public async Task Consume(ConsumeContext<TriggerChain> context)
        {
            var chainId = context.Message.ChainId;

            var chain = Chains.FirstOrDefault(x => x.ChainId == chainId);
            if (chain == null)
            {
                chain = new Chain {ChainId = chainId};
                Chains.Add(chain);
            }

            if (!chain.Done)
            {
                if (!chain.DownloadStepId.HasValue)
                {
                    chain.DownloadStepId = await TriggerDownload(context);
                }
                else
                {
                    if (!chain.DownloadStepDone) chain.DownloadStepDone = await Check<SubmitDownload, SubmitDownloadCompleted, SubmitDownloadFaulted>(
                        _downloadClient, 
                        new SubmitDownloadRequest
                        {
                            ChainId = chain.ChainId,
                            OrderId = chain.DownloadStepId.Value
                        }, _logger);

                    if (!chain.ParseStepId.HasValue)
                    {
                        chain.ParseStepId = await TriggerParse(context);
                    }
                    else
                    {
                        if (!chain.ParseStepDone) chain.ParseStepDone = await Check<SubmitParse, SubmitParseCompleted, SubmitParseFaulted>(
                            _parseClient, new SubmitParseRequest
                            {
                                ChainId = chain.ChainId,
                                OrderId = chain.ParseStepId.Value
                            }, _logger);

                        if (chain.ParseStepDone && !chain.PhotoStepId.HasValue)
                        {
                            _logger.LogDebug("We are to trigger PHOTO - todo...");
                            // TODO: await TriggerPhoto(context, chain);
                        }
                        else
                        {
                           // if (!chain.PhotoStepDone) chain.PhotoStepDone = await Check<SubmitPhotoCompleted, SubmitPhotoFaulted>(context);
                           //
                           // if (chain.PhotoStepDone &&  !chain.ImportStepId.HasValue)
                           // {
                           //     await TriggerImport(context, chain);
                           // }
                           // else
                           // {
                           //     if (!chain.ImportStepDone) chain.ImportStepDone = await Check<SubmitImportCompleted, SubmitImportFaulted>(context);
                           // }
                        }
                    }
                }
            }

            await context.RespondAsync<ChainStatus>(new
            {
                Chain = chain
            });
        }

        private static async Task<bool> Check<TMessage, TCompleted, TFaulted>(IRequestClient<TMessage> client, TMessage message, ILogger logger)
            where TCompleted : class
            where TFaulted : class
            where TMessage : class
        {
            try
            {
                Response<TCompleted, TFaulted> response = await client.GetResponse<TCompleted, TFaulted>(message); // b460ee9f-fc62-42ed-b821-8b549e030f9e

                if (response.Is(out Response<TCompleted> _)) return true;
                if (response.Is(out Response<TFaulted> _)) return false;
                return false;
            }
            catch (RequestTimeoutException)
            {
                logger.LogDebug($"OH UH... request timeout. It seems it's not ready yet. Try again later!");
                return false;
            }
        }

        private static async Task<Guid> TriggerParse(ConsumeContext<TriggerChain> context)
        {
            var parseStepId = NewId.NextGuid();

            await context.Publish<SubmitParse>(new
            {
                ChainId = context.Message.ChainId,
                OrderId = parseStepId,
                Parses = new []
                {
                    new Parse
                    {
                        FileName = "DUMMY-HARDCODE",
                        ParseId = NewId.NextGuid(),
                        Parsed = false
                    }
                }
              //  Parses = context.Message.LinesCompleted.Select(line => new Parse
              //  {
              //      FileName = line.Value.Download.FileName,
              //      ParseId = NewId.NextGuid(),
              //      Parsed = false
              //  }).ToArray()
            });

            return parseStepId;
        }

        private static async Task<Guid> TriggerDownload(ConsumeContext<TriggerChain> context)
        {
            var downloadStepId = NewId.NextGuid();

            await context.Publish<SubmitDownload>(new
            {
                context.Message.ChainId,
                OrderId = downloadStepId,
                Files = new[] {"First.txt", "Second.txt"}.Select(name => new Download
                {
                    FileName = name,
                    DownloadId = NewId.NextGuid(),
                    Downloaded = false
                }).ToArray()
            });

            return downloadStepId;
        }

        public async Task Consume(ConsumeContext<SubmitParseCompleted> context)
        {
            var chainId = context.Message.ChainId;
            var parseStepId = context.Message.OrderId;

            var chain = Chains.FirstOrDefault(x => x.ChainId == chainId) ??
                        throw new Exception($"Chain {chainId} not found.");

            chain.ParseStepDone = true;

            // TODO: trigger photo future

            await context.RespondAsync<ChainStatus>(new
            {
                Chain = chain
            });
        }


        public async Task Consume(ConsumeContext<HandleDownload> context)
        {
            _logger.LogDebug($"Downloading {context.Message.File.FileName}...");

            await Task.Delay(1000);

            await context.RespondAsync<DownloadReady>(new
            {
                context.Message.OrderId,
                context.Message.OrderLineId,
                
                Size = context.Message.File.FileName.Length * 100
            });
        }

        public async Task Consume(ConsumeContext<HandleParse> context)
        {
            _logger.LogDebug($"Parsing {context.Message.Parse?.FileName}...");

            await Task.Delay(500*context.Message.Parse?.FileName.Length ?? 1);

            await context.RespondAsync<ParseReady>(new
            {
                context.Message.OrderId,
                context.Message.OrderLineId,
                Products = context.Message.Parse?.FileName.Length ?? 1 * 500
            });
        }

        public Task Consume(ConsumeContext<TestSignal> context)
        {
            _logger.LogDebug("OUHH YEAHHHHHHH!!! -------------------------------------------");
            return Task.CompletedTask;
        }
    }

}