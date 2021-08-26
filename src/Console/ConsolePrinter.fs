module Console.ConsolePrinter

open System
open Crawler.Client
open Crawler.Types

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

type ConsolePrinter =
    interface IMessagePrinter with
        member this.PrintStartMessage() = Console.WriteLine "Crawling is started"
        member this.PrintFinishMessage result elapsed =
            Console.WriteLine $"Crawling is finished:\r\n\tvisited links count: {result.Visited}\r\n\ttime elapsed, sec: {elapsed}"
        member this.PrintCrawlResult result = printCrawlResult result
        member this.PrintUnknownType unknown = printUnknownType unknown

    new() = {}