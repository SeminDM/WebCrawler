module Web.ActorProviders

open Crawler.Client
open Crawler.Crawler
open CrawlerHub
open Akka.Actor
open Akka.FSharp
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.DependencyInjection
open System

type CrawlerActorProvider(serviceProvider: IServiceProvider) =
    let _getter =
        lazy
            let system = serviceProvider.GetService<ActorSystem>()
            spawn system "crawlerActor" <| crawlerActor

    member this.GetActor = _getter.Value

type SignalRActorProvider(serviceProvider: IServiceProvider) =
    let _getter =
        lazy
            let system = serviceProvider.GetService<ActorSystem>()
            let crawlerProvider = serviceProvider.GetService<CrawlerActorProvider>()
            let crawler = crawlerProvider.GetActor
            let printer = serviceProvider.GetService<IHubContext<CrawlHub>>() |> SignalRPrinter
            spawn system "signalrActor" <| (clientActor crawler printer)

    member this.GetActor = _getter.Value