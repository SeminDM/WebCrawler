module Crawler.Crawler

open Crawler.CrawlerTypes
open Crawler.DownloadTypes
open Crawler.ParseTypes
open Akka.FSharp

let processCrawlJob downloadActor (crawlJob: CrawlDocumentJob) =
    downloadActor <! { Initiator = crawlJob.Initiator; DocumentUri = crawlJob.DocumentUri }

let processDownloadResult parseActor downloadResult =
    match downloadResult with
    | DownloadDocumentJobResult { Initiator = initiator; DocumentUri = uri; HtmlContent = html } ->
        initiator <! { DocumentUri = uri; Size = String.length html }
        parseActor <! { Initiator = initiator; RootUri = uri; HtmlString = html }
    
    | DownloadImageJobResult { Initiator = initiator; ImageUri = uri; ImageContent = img } ->
        initiator <! { ImageUri = uri; Size = Array.length img }

    | DownloadFailedJobResult { Initiator = initiator; Uri = uri; Reason = reason } ->
        initiator <! { RootUri = uri; Reason = reason }

let processParseResult downloadActor parseResult =
    let { Initiator = initiator; Uri = _; Links = links; ImageLinks = imgLinks } = parseResult
    List.iter (fun link -> downloadActor <! DownloadDocumentJob { Initiator = initiator; DocumentUri = link }) links
    List.iter (fun imgLink -> downloadActor <! DownloadImageJob { Initiator = initiator; ImageUri = imgLink }) imgLinks

let crawlerActor downloadActor parseActor msg =
    match box msg with
    | :? CrawlDocumentJob as crawlJob -> processCrawlJob downloadActor crawlJob
    | :? ParseJobResult as parseResult -> processParseResult downloadActor parseResult
    | :? DownloadJobResult as downloadResult -> processDownloadResult parseActor downloadResult
    | _ -> failwith ""
