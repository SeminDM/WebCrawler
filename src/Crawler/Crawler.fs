module Crawler.Crawler

open Crawler.DownloadExecutor
open Crawler.Types
open Akka.FSharp
open Akka.Routing

let processCrawlJob (mailbox: Actor<_>) (crawlJob: CrawlJob) =
    // increase when multiple sites crawling will be implemented
    let pool = RoundRobinPool 1
    let coordinator = spawnOpt mailbox.Context "downloadCoordinatorActor" (downloadCoordinatorActor mailbox.Self) [ SpawnOption.Router pool ]
    coordinator <! DocumentJob { Initiator = crawlJob.Initiator; WebsiteUri = crawlJob.WebsiteUri; DocumentUri = crawlJob.WebsiteUri }

let processDownloadResult downloadResult =
    match downloadResult with
    | DocumentResult { Initiator = initiator; DownloadUri = uri; HtmlContent = html } ->
        initiator <! Document { Initiator = initiator; DocumentUri = uri; Size = String.length html }

    | ImageResult { Initiator = initiator; DownloadUri = uri; ImageContent = img } ->
        initiator <! Image { Initiator = initiator; ImageUri = uri; Size = Array.length img }

    | FailedResult { Initiator = initiator; DownloadUri = uri; Reason = reason } ->
        initiator <! Error { Initiator = initiator; RootUri = uri; Reason = reason }

let crawlerActor (mailbox: Actor<_>) =
    let rec loop() =
        actor {
            let! msg = mailbox.Receive()
            match box msg with
            | :? CrawlJob as crawlJob -> processCrawlJob mailbox crawlJob
            | :? DownloadResult as downloadResult -> processDownloadResult downloadResult
            | :? CrawlFinishResult as finishResult -> finishResult.Initiator <! finishResult
            | _ -> failwith ""
            return! loop()
        }
    loop()