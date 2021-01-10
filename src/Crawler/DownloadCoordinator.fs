module Crawler.DownloadCoordinator

open Crawler.DownloadTypes
open Crawler.ParseTypes
open Akka.FSharp

let runDownload downloadActor downloadJob visitedLinks=
    let getUri = function DocumentJob dj -> dj.DocumentUri | ImageJob ij -> ij.ImageUri
    let uri = getUri downloadJob
    if not (Set.contains (uri.ToString()) visitedLinks)
    then 
        downloadActor <! downloadJob
        Set.add (uri.ToString()) visitedLinks
    else 
        visitedLinks

let processParseResult downloadActor visitedLinks parseResult =
    let { Initiator = initiator; RootUri = rootUri; Links = links; ImageLinks = imgLinks } = parseResult

    let rec runDoc links visited =
        match links with
        | Some (h::t) ->  
            visited
            |> runDownload downloadActor (DocumentJob { Initiator = initiator; OriginalUri = rootUri; DocumentUri = h })
            |> runDoc (Some t)
        | _ -> visited

    let rec runImg links visited =
        match links with
        | Some (h::t) ->  
            visited
            |> runDownload downloadActor (ImageJob { Initiator = initiator; OriginalUri = rootUri; ImageUri = h })
            |> runImg (Some t)
        | _ -> visited

    visitedLinks
    |> runDoc links
    |> runImg imgLinks

let processDownloadResult crawler parseActor downloadResult =
    match downloadResult with
    | DocumentJobResult { Initiator = initiator; OriginalUri = root; HtmlContent = html } ->
        crawler <! downloadResult
        parseActor <! { Initiator = initiator; RootUri = root; HtmlString = html }
    | ImageJobResult _ -> crawler <! downloadResult
    | FailedJobResult _ -> crawler <! downloadResult
  
let downloadCoordinatorActor downloadActor parseActor (mailbox: Actor<_>) =
    let crawler = mailbox.Context.Parent
    let rec loop visitedLinks =
        actor {
            let! msg = mailbox.Receive()
            let visited' = match box msg with
            | :? DownloadJob as downloadJob -> runDownload downloadActor  downloadJob visitedLinks
            | :? DownloadJobResult as downloadResult -> processDownloadResult crawler parseActor downloadResult; visitedLinks
            | :? ParseJobResult as parseResult -> processParseResult downloadActor visitedLinks parseResult
            | _ -> failwith ""
            return! loop visited'
        }
    loop Set.empty