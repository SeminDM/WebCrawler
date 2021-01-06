module Crawler.DownloadActor

open Crawler.Downloader
open Crawler.DownloadTypes
open Akka.FSharp

let processJob sender job =
    match job with
    | DocumentJob dj -> sender <! downloadDocument dj
    | ImageJob ij -> sender <! downloadImage ij

let downloadActor (mailbox: Actor<_>) job =
    let sender = mailbox.Sender()
    processJob sender job
