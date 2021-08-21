module Crawler.Crawler

open Crawler.DownloadExecutor
open Crawler.Types
open Akka.FSharp

let processCrawlJob (mailbox: Actor<_>) downloadActor parseActor (crawlJob: CrawlJob) =
    let coordinator = spawn mailbox.Context "downloadCoordinatorActor" (downloadCoordinatorActor downloadActor parseActor)
    coordinator <! DocumentJob { Initiator = crawlJob.Initiator; WebsiteUri = crawlJob.WebsiteUri; DocumentUri = crawlJob.WebsiteUri }

let processDownloadResult downloadResult =
    match downloadResult with
    | DocumentResult { Initiator = initiator; DownloadUri = uri; HtmlContent = html } ->
        initiator <! Document { Initiator = initiator; DocumentUri = uri; Size = String.length html }

    | ImageResult { Initiator = initiator; DownloadUri = uri; ImageContent = img } ->
        initiator <! Image { Initiator = initiator; ImageUri = uri; Size = Array.length img }

    | FailedResult { Initiator = initiator; DownloadUri = uri; Reason = reason } ->
        initiator <! Error { Initiator = initiator; RootUri = uri; Reason = reason }

let crawlerActor2 downloadActor parseActor (mailbox: Actor<_>) msg =
    match box msg with
    | :? CrawlJob as crawlJob -> processCrawlJob mailbox downloadActor parseActor crawlJob
    | :? DownloadResult as downloadResult -> processDownloadResult downloadResult
    | _ -> failwith ""

let crawlerActor downloadActor parseActor (mailbox: Actor<_>) =
    let rec loop() =
        actor {
            let! msg = mailbox.Receive()
            match box msg with
            | :? CrawlJob as crawlJob -> processCrawlJob mailbox downloadActor parseActor crawlJob
            | :? DownloadResult as downloadResult -> processDownloadResult downloadResult
            | :? CrawlFinishResult as finishResult -> finishResult.Initiator <! finishResult
            | _ -> failwith ""
            return! loop()
        }
    loop()