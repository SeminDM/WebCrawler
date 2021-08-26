module Crawler.Client

open Crawler.Types
open Akka.Actor
open Akka.FSharp
open System

type IMessagePrinter = interface
    abstract PrintStartMessage: unit -> unit
    abstract PrintFinishMessage: CrawlFinishResult -> float -> unit
    abstract PrintCrawlResult: CrawlResult -> unit
    abstract PrintUnknownType: obj -> unit
end

let clientActor (crawler: IActorRef) (printer: IMessagePrinter) (mailbox: Actor<_>) =
    let timer = System.Diagnostics.Stopwatch()
    let runCrawler uri =
        printer.PrintStartMessage()
        timer.Start()
        crawler <! { CrawlJob.Initiator = mailbox.Self; CrawlJob.WebsiteUri = Uri(uri) }

    let rec loop() =
        actor {
            let! msg = mailbox.Receive()
            match box msg with
            | :? string as uri -> runCrawler uri
            | :? CrawlResult as crawlResult -> printer.PrintCrawlResult crawlResult
            | :? CrawlFinishResult as finishResult ->
                timer.Stop()
                printer.PrintFinishMessage finishResult timer.Elapsed.TotalSeconds
            | obj -> printer.PrintUnknownType obj
            return! loop()
        }
    loop()