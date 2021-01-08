module Crawler.Crawler

open Crawler.CrawlerTypes
open Crawler.DownloadTypes
open Crawler.ParseTypes
open Crawler.DownloadCoordinator
open Crawler.Parser
open Akka.FSharp

let processCrawlJob (mailbox: Actor<_>) downloadActor parseActor (crawlJob: CrawlDocumentJob) =
    let coordinator = spawn mailbox.Context "downloadCoordinatorActor" (downloadCoordinatorActor downloadActor parseActor)
    coordinator <! DownloadJob.DocumentJob { Initiator = crawlJob.Initiator; DocumentUri = crawlJob.DocumentUri }

let processDownloadResult downloadResult =
    match downloadResult with
    | DocumentJobResult { Initiator = initiator; DocumentUri = uri; HtmlContent = html } ->
        initiator <! DocumentResult { Initiator = initiator; DocumentUri = uri; Size = String.length html }
    
    | ImageJobResult { Initiator = initiator; ImageUri = uri; ImageContent = img } ->
        initiator <! ImageResult { Initiator = initiator; ImageUri = uri; Size = Array.length img }

    | FailedJobResult { Initiator = initiator; Uri = uri; Reason = reason } ->
        initiator <! FailedResult { Initiator = initiator; RootUri = uri; Reason = reason }

let crawlerActor downloadActor parseActor (mailbox: Actor<_>) msg =
    match box msg with
    | :? CrawlDocumentJob as crawlJob -> processCrawlJob mailbox downloadActor parseActor crawlJob
    | :? DownloadJobResult as downloadResult -> processDownloadResult downloadResult
    | _ -> failwith ""