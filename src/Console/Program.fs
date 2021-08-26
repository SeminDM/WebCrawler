open Console.ConsolePrinter
open Crawler.Client
open Crawler.Crawler
open Akka.FSharp
open System

let getSiteAddress args = if args = null || (Array.length args) = 0 then (*"https://www.mirf.ru/"*) (*"https://www.eurosport.ru/"*)"https://docs.microsoft.com/ru-ru/" else args.[0]

[<EntryPoint>]
let main argv =
    let system = create "consoleSystem" <| Configuration.load()
    let crawlerRef = spawn system "crawlerActor" <| crawlerActor
    let printer = ConsolePrinter()

    let consoleActorRef = spawn system "consoleActor" <| (clientActor crawlerRef printer)

    //"https://www.eurosport.ru/" "https://www.mirf.ru/" "https://docs.microsoft.com/ru-ru/"
    argv
    |> getSiteAddress
    |> (<!) consoleActorRef 

    Console.ReadKey() |> ignore
    system.Terminate() |> Async.AwaitTask |> ignore
    0