module Crawler.DownloadActor

open Crawler.Downloader
open Crawler.DownloadTypes
open Akka.FSharp

let processJob (mailbox: Actor<DownloadJob>) job =
    let sender = mailbox.Sender()
    match job with
    | DownloadDocumentJob dj -> sender <! downloadDocument dj
    | DownloadImageJob ij -> sender <! downloadImage ij

let downloadActor (mailbox: Actor<DownloadJob>) job = ()
    //processJob mailbox job
