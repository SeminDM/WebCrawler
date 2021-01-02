module Crawler.DownloadActor

open Crawler.Downloader
open Crawler.DownloadTypes
open Crawler.ParseTypes
open Akka.FSharp

let processJob sender parseActor job =
    match job with
    | DownloadDocumentJob { Uri = uri } -> 
        let r = downloadDocument uri
        match r with
        | DownloadDocumentResult { Uri = uri; HtmlContent = html } -> parseActor <! { RootUri = uri; HtmlString = html }
        | DownloadFailedResult { Uri = uri; Reason = reason } -> sender <! reason
    | DownloadDocumentWithImagesJob { Uri = uri } -> sender <! downloadImage uri

let processParseResult downloadActor parseActor parseResult =
    let { Links = links; ImageLinks = imgLinks } = parseResult



let downloadActor parseActor (mailbox: Actor<_>) =
    let rec loop() = actor {
        let! msg = mailbox.Receive()
        match msg with
        | DownloadJob as job -> processJob (mailbox.Sender()) parseActor job
        | ParseDocumentResult as parseResult -> processParseResult parseResult 

        return! loop()
    }
    loop()

