module Crawler.DownloadActor

open Crawler.Downloader
open Crawler.DownloadTypes
open Akka.FSharp

let downloadActor (mailbox: Actor<_>) =
    let httpClient = createHttpClient
    let rec loop() =
        actor {
            let! job = mailbox.Receive()
            match job with
            | DocumentJob dj ->  mailbox.Sender() <! downloadDocument httpClient dj
            | ImageJob ij ->  mailbox.Sender() <! downloadImage httpClient ij
            return! loop()
        }
    loop()
