namespace Startup.Controllers.CrawlController

open ActorTypes
open Akka.FSharp
open Microsoft.AspNetCore.Mvc
open System

[<Route("api/crawl")>]
[<ApiController>]
type CrawlController (serviceProvider:IServiceProvider, signalRProvider: SignalRActorRef) =
    inherit Controller()

    let _signalRProvider = signalRProvider
    let _serviceProvider = serviceProvider

    [<HttpPost>]
    [<Route("run")>]
    member this.Run([<FromForm>]website: string) = 
        let (SignalRActorRef a) =  _signalRProvider
        a <! website