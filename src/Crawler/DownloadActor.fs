module Crawler.DownloadActor

open Crawler.Downloader
open Crawler.DownloadTypes
open Akka.FSharp

let processJob sender job =
    match job with
    | DownloadDocumentJob { Uri = uri } -> sender <! downloadDocument uri
    | DownloadImageJob { Uri = uri } -> sender <! downloadImage uri

let downloadActor (mailbox: Actor<_>) =
    let processJob' = processJob (mailbox.Sender())

    let rec loop() = actor {
        let! job = mailbox.Receive()
        processJob' job
        return! loop()
    }
    loop()