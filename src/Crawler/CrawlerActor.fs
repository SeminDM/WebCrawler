module Crawler.Crawler

open Crawler.CrawlerTypes
open Crawler.DownloadTypes
open Crawler.ParseTypes
open Akka.FSharp

let processCrawlJob dowloadActor (crawlJob:CrawlDocumentJob) = dowloadActor <! DownloadDocumentJob { Uri = crawlJob.DocumentUri }

let processDownloadResult sender parseActor downloadResult =
    match downloadResult with
    | DownloadDocumentJobResult { DocumentUri = uri; HtmlContent = html } ->
        sender <! { DocumentUri = uri; Size = String.length html }
        parseActor <! { RootUri = uri; HtmlString = html }
    
    | DownloadImageJobResult { ImageUri = uri; ImageContent = img } ->
        sender <! { ImageUri = uri; Size = Array.length img }

    | DownloadFailedJobResult { Uri = uri; Reason = reason } ->
        sender <! { RootUri = uri; Reason = reason }

let processParseResult dowloadActor parseResult =
    let { Uri = _; Links = links; ImageLinks = imgLinks } = parseResult
    List.iter (fun link -> dowloadActor <! DownloadDocumentJob { Uri = link }) links
    List.iter (fun imgLink -> dowloadActor <! DownloadImageJob { Uri = imgLink }) imgLinks

let crawlerActor (mailbox: Actor<_>) dowloadActor parseActor =
    let processCrawlJob' = processCrawlJob dowloadActor
    let processDownloadResult' = processDownloadResult (mailbox.Sender()) parseActor
    let processParseResult' = processParseResult dowloadActor

    let rec loop() = actor {
        let! msg = mailbox.Receive()
        match box msg with
        | :? CrawlDocumentJob as crawlJob -> processCrawlJob' crawlJob
        | :? DownloadJobResult as downloadResult -> processDownloadResult' downloadResult
        | :? ParseJobResult as parseResult -> processParseResult' parseResult
        | _ -> failwith ""
        return! loop()
    }
    loop()