﻿open Crawler.Crawler
open Crawler.Types
open Akka.FSharp
open System

let getSiteAddress args = if args = null || (Array.length args) = 0 then (*"https://www.mirf.ru/"*) (*"https://www.eurosport.ru/"*)"https://docs.microsoft.com/ru-ru/" else args.[0]

let printColorMessage (msg: string) color =
    Console.ForegroundColor <- color
    Console.WriteLine msg
    Console.ResetColor()

let printDocResult result =
    let { DocumentUri = uri; Size = size } = result
    let msg = String.Format("Document is downloaded. URI: {0} Size {1}", uri, size)
    printColorMessage msg ConsoleColor.Blue

let printImgResult result =
    let { ImageUri = uri; Size = size } = result
    let msg = String.Format("Image is downloaded. URI: {0} Size {1}", uri, size)
    printColorMessage msg ConsoleColor.Green

let printErrorResult result =
    let { RootUri = uri; Reason = reason } = result
    let msg = String.Format("Download is failed. URI: {0} Reason {1}", uri, reason)
    printColorMessage msg ConsoleColor.Red

let printCrawlResult = function
    | Document d -> printDocResult d
    | Image i -> printImgResult i
    | Error e -> printErrorResult e

let printUnknownType obj =
    let msg = String.Format("Message type {0} is not supported", obj.GetType())
    printColorMessage msg ConsoleColor.Yellow
    

[<EntryPoint>]
let main argv =
    let system = create "consoleSystem" <| Configuration.load()
    let crawlerRef = spawn system "crawlerActor" <| crawlerActor

    let consoleActorRef = spawn system "consoleActor" <| fun mailbox ->
        
        let timer = new System.Diagnostics.Stopwatch()
        let runCrawler uri =
            Console.WriteLine "Crawling is started"
            timer.Start()
            crawlerRef <! { CrawlJob.Initiator = mailbox.Self; CrawlJob.WebsiteUri = new Uri(uri) }

        let rec loop() =
            actor {
                let! msg = mailbox.Receive()
                match box msg with
                | :? string as uri -> uri |> runCrawler 
                | :? CrawlResult as crawlResult -> crawlResult |> printCrawlResult 
                | :? CrawlFinishResult as finishResult -> 
                    timer.Stop()
                    Console.WriteLine $"Crawling is finished:\r\n\tvisited links count: {finishResult.Visited}\r\n\ttime elapsed, sec: {timer.Elapsed.TotalSeconds}"; 
                | obj -> obj |> printUnknownType
                return! loop()
            }
        loop()

    //"https://www.eurosport.ru/" "https://www.mirf.ru/" "https://docs.microsoft.com/ru-ru/"
    argv
    |> getSiteAddress
    |> (<!) consoleActorRef 

    Console.ReadKey() |> ignore
    system.Terminate() |> Async.AwaitTask |> ignore
    0
