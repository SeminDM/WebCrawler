module CrawlerHub

open Crawler.Client
open Crawler.Types
open Microsoft.AspNetCore.SignalR

type CrawlHub() = inherit Hub()

type SignalRPrinter (hub: IHubContext<CrawlHub>) =
    let _hub = hub

    let send (msg: string) =
        _hub.Clients.All.SendAsync ("receive", msg) |> Async.AwaitTask |> ignore

    interface IMessagePrinter with
        member this.PrintStartMessage() = "Crawling is started" |> send
        member this.PrintFinishMessage result elapsed = $"Crawling is finished:\r\n\tvisited links count: {result.Visited}\r\n\ttime elapsed, sec: {elapsed}" |> send
        member this.PrintCrawlResult result = result.ToString() |> send
        member this.PrintUnknownType unknown = unknown.ToString() |> send