module ActorTypes

open CrawlerHub
open Crawler.Client
open Crawler.Crawler
open Akka.Actor
open Akka.FSharp
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.SignalR
open System

type CrawlerActorRef = CrawlerActorRef of IActorRef
type SignalRActorRef = SignalRActorRef of IActorRef

let createCrawler = fun (serviceProvider: IServiceProvider) -> 
    let system = serviceProvider.GetService<ActorSystem>()
    crawlerActor |> spawn system "crawlerActor" |> CrawlerActorRef

let createSignalR = fun (serviceProvider: IServiceProvider) ->
    let system = serviceProvider.GetService<ActorSystem>()
    let (CrawlerActorRef crawler) = serviceProvider.GetService<CrawlerActorRef>()
    let printer = serviceProvider.GetService<IHubContext<CrawlHub>>() |> SignalRPrinter
    (clientActor crawler printer) |> spawn system "signalrActor" |> SignalRActorRef

