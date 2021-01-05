// Learn more about F# at http://fsharp.org

open Crawler.Crawler
open Crawler.DownloadActor
open Crawler.ParseActor
open Crawler.CrawlerTypes
open Crawler.DownloadTypes
open Crawler.ParseTypes
open Akka.FSharp
open Akka.Actor
open System

[<EntryPoint>]
let main argv =

    let system = System.create "consoleSystem" <| Configuration.load()

    let downloaderRef = spawn system "downloadActor" <| actorOf2 downloadActor
    let parserRef = spawn system "parseActor" <| actorOf2 parseActor
    let crawlerRef = spawn system "crawlerActor" <| actorOf (crawlerActor downloaderRef parserRef)

    let coordinatorRef = spawn system "coordinatorActor" <| fun mailbox ->
        let rec loop() =
            actor {
                let! msg = mailbox.Receive()
                match box msg with
                | :? string as uri -> crawlerRef <! { Initiator = mailbox.Self; DocumentUri = new Uri(uri) }
                | :? CrawlResult as crawlResult ->
                    match crawlResult with
                    | CrawlDocResult { DocumentUri = uri; Size = size } ->
                        let message = String.Format("Document is downloaded. URI: %s Size %d", uri, size)
                        Console.WriteLine(message)
                    | CrawlImageResult { ImageUri = uri; Size = size } ->
                        let message = String.Format("Image is downloaded. URI: %s Size %d", uri, size)
                        Console.WriteLine(message)
                    | CrawlFailedResult { RootUri = uri; Reason = reason } ->
                        let message = String.Format("Download is failed. URI: %s Reason %d", uri, reason)
                        Console.WriteLine(message)
                | _ -> Console.WriteLine("Unknown messge type")
                return! loop()
            }
        loop()

    coordinatorRef <! "https://docs.microsoft.com/ru-ru/"
    downloaderRef <! { DownloadDocumentJob.Initiator = ActorRefs.Nobody; DownloadDocumentJob.DocumentUri = new Uri("https://docs.microsoft.com/ru-ru/") }
    parserRef <! { Initiator = ActorRefs.Nobody; RootUri = new Uri("http://ya.ru"); ParseDocumentJob.HtmlString = "<a href=\"URL\">http://ya.ru/a</a> <img src=\"URL\" alt=\"альтернативный текст\">" }

    System.Console.ReadKey() |> ignore
    0

    //let r = parserRef.Ask({ RootUri = new Uri("http://ya.ru"); HtmlString = "<a href=\"URL\">http://ya.ru/a</a> <img src=\"URL\" alt=\"альтернативный текст\">" }, TimeSpan.FromSeconds(5.0))