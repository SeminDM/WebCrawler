namespace Startup.Controllers.CrawlController

open System
open Akka.FSharp
open Microsoft.AspNetCore.Mvc
open Start

[<Route("api/crawl")>]
[<ApiController>]
type CrawlController (serviceProvider:IServiceProvider, signalRProvider: SignalRActorProvider) =
    inherit Controller()

    let _signalRProvider = signalRProvider
    let _serviceProvider = serviceProvider

    [<HttpPost>]
    [<Route("run")>]
    member this.Run([<FromForm>]website: string) = _signalRProvider.GetActor <! website