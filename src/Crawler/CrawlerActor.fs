module Crawler.Crawler

open Crawler.CrawlerTypes
open Crawler.DownloadTypes
open Crawler.ParseTypes
open Crawler.Parser
open Akka.FSharp

let processCrawlJob downloadActor (crawlJob: CrawlDocumentJob) =
    downloadActor <! DocumentJob { Initiator = crawlJob.Initiator; DocumentUri = crawlJob.DocumentUri }

let processDownloadResult parseActor downloadResult =
    match downloadResult with
    | DocumentJobResult { Initiator = initiator; DocumentUri = uri; HtmlContent = html } ->
        initiator <! DocumentResult { Initiator = initiator; DocumentUri = uri; Size = String.length html }
        parseActor <! { Initiator = initiator; RootUri = uri; HtmlString = html }
    
    | ImageJobResult { Initiator = initiator; ImageUri = uri; ImageContent = img } ->
        initiator <! ImageResult { Initiator = initiator;ImageUri = uri; Size = Array.length img }

    | FailedJobResult { Initiator = initiator; Uri = uri; Reason = reason } ->
        initiator <! FailedResult { Initiator = initiator; RootUri = uri; Reason = reason }

let processParseResult downloadActor parseResult =
    let { Initiator = initiator; Uri = _; Links = links; ImageLinks = imgLinks } = parseResult
    links |> bind (List.iter (fun link -> downloadActor <! DocumentJob { Initiator = initiator; DocumentUri = link })) |> ignore 
    imgLinks |> bind (List.iter (fun imgLink -> downloadActor <! ImageJob { Initiator = initiator; ImageUri = imgLink })) |> ignore

let crawlerActor downloadActor parseActor msg =
    match box msg with
    | :? CrawlDocumentJob as crawlJob -> processCrawlJob downloadActor crawlJob
    | :? ParseJobResult as parseResult -> processParseResult downloadActor parseResult
    | :? DownloadJobResult as downloadResult -> processDownloadResult parseActor downloadResult
    | _ -> failwith ""
