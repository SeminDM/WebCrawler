open Crawler.Crawler
open Crawler.DownloadActor
open Crawler.ParseActor
open Crawler.CrawlerTypes
open Akka.FSharp
open System

let getSiteAddress args = if args = null || (Array.length args) = 0 then "https://docs.microsoft.com/ru-ru/" else args.[0]

let printDocResult result =
    let { DocumentUri = uri; Size = size } = result
    let color = Console.BackgroundColor
    Console.ForegroundColor <- ConsoleColor.Blue
    let message = String.Format("Document is downloaded. URI: {0} Size {1}", uri, size)
    Console.WriteLine(message)
    Console.ForegroundColor <- color

let printImgResult result =
    let { ImageUri = uri; Size = size } = result
    let color = Console.BackgroundColor
    Console.ForegroundColor <- ConsoleColor.Green
    let message = String.Format("Image is downloaded. URI: {0} Size {1}", uri, size)
    Console.WriteLine(message)
    Console.ForegroundColor <- color

let printErrorResult result =
    let { RootUri = uri; Reason = reason } = result
    let color = Console.BackgroundColor
    Console.ForegroundColor <- ConsoleColor.Red
    let message = String.Format("Download is failed. URI: {0} Reason {1}", uri, reason)
    Console.WriteLine(message)
    Console.ForegroundColor <- color

let printCrawlResult = function
    | DocumentResult d -> printDocResult d
    | ImageResult i -> printImgResult i
    | FailedResult e -> printErrorResult e

let printUnknownType obj =
    let color = Console.BackgroundColor
    Console.ForegroundColor <- ConsoleColor.Yellow
    let message = String.Format("Message type {0} is not supported", obj.GetType())
    Console.WriteLine(message)
    Console.ForegroundColor <- color
    

[<EntryPoint>]
let main argv =
    let system = System.create "consoleSystem" <| Configuration.load()

    let downloaderRef = spawn system "downloadActor" <| actorOf2 downloadActor
    let parserRef = spawn system "parseActor" <| actorOf2 parseActor
    let crawlerRef = spawn system "crawlerActor" <| actorOf (crawlerActor downloaderRef parserRef)

    let coordinatorRef = spawn system "coordinatorActor" <| fun mailbox ->
        let runCrawler uri = crawlerRef <! { CrawlDocumentJob.Initiator = mailbox.Self; CrawlDocumentJob.DocumentUri = new Uri(uri) }
        let rec loop() =
            actor {
                let! msg = mailbox.Receive()
                match box msg with
                | :? string as uri -> uri |> runCrawler |> ignore
                | :? CrawlResult as crawlResult -> crawlResult |> printCrawlResult |> ignore
                | obj -> obj |> printUnknownType 
                return! loop()
            }
        loop()

    //"https://www.eurosport.ru/" "https://www.mirf.ru/" "https://docs.microsoft.com/ru-ru/"
    argv
    |> getSiteAddress
    |> (<!) coordinatorRef 

    System.Console.ReadKey() |> ignore
    0
