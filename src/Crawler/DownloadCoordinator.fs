module Crawler.DownloadCoordinator

open Crawler.DownloadTypes
open Crawler.ParseTypes
open Crawler.Parser
open Akka.FSharp

let processParseResult downloadActor visitedLinks parseResult =
    let { Initiator = initiator; Uri = _; Links = links; ImageLinks = imgLinks } = parseResult
    links |> bind (List.iter (fun link -> downloadActor <! DocumentJob { Initiator = initiator; DocumentUri = link })) |> ignore 
    imgLinks |> bind (List.iter (fun imgLink -> downloadActor <! ImageJob { Initiator = initiator; ImageUri = imgLink })) |> ignore

let processDownloadResult crawler parseActor downloadResult =
    match downloadResult with
    | DocumentJobResult { Initiator = initiator; DocumentUri = uri; HtmlContent = html } ->
        crawler <! downloadResult
        parseActor <! { Initiator = initiator; RootUri = uri; HtmlString = html }
    | ImageJobResult _ -> crawler <! downloadResult
    | FailedJobResult _ -> crawler <! downloadResult

let runDownload downloadActor visitedLinks downloadJob =
    let getUri = function DocumentJob dj -> dj.DocumentUri | ImageJob ij -> ij.ImageUri
    let uri = getUri downloadJob
    if (Set.contains (uri.ToString()) visitedLinks)
    then 
        downloadActor <! downloadJob
        Set.add (uri.ToString()) visitedLinks
    else visitedLinks
  
let downloadCoordinatorActor downloadActor parseActor (mailbox: Actor<_>) =
    let crawler = mailbox.Context.Parent
    let rec loop visitedLinks =
        actor {
            let! msg = mailbox.Receive()
            match box msg with
            | :? DownloadJob as downloadJob -> return! loop (runDownload downloadActor visitedLinks downloadJob)
            | :? DownloadJobResult as downloadResult -> processDownloadResult crawler parseActor downloadResult
            | :? ParseJobResult as parseResult -> processParseResult downloadActor visitedLinks parseResult
            | _ -> failwith ""
            return! loop visitedLinks
        }
    loop Set.empty