module Crawler.DownloadExecutor

open Crawler.Types
open Akka.FSharp
open System.IO
open System.Net.Http

let createHttpClient = new HttpClient()

let downloadDocument (httpClient: HttpClient) { Initiator = initiator; WebsiteUri = original; DocumentUri = uri } =
    try
        let html = httpClient.GetStringAsync uri
        DocumentResult { Initiator = initiator; WebsiteUri = original; DownloadUri = uri; HtmlContent = html.Result }
    with
    | e -> FailedResult { Initiator = initiator; WebsiteUri = original; DownloadUri = uri; Reason = e.Message }

let downloadImage (httpClient: HttpClient) { Initiator = initiator; WebsiteUri = original; ImageUri = uri } =
    try
        use resp = (httpClient.GetStreamAsync uri).Result
        let ms = new MemoryStream()
        resp.CopyTo(ms)
        ImageResult { Initiator = initiator; WebsiteUri = original; DownloadUri = uri; ImageContent = ms.ToArray() }
    with
    | e -> FailedResult { Initiator = initiator; WebsiteUri = original; DownloadUri = uri; Reason = e.Message }

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
            |> runDownload downloadActor (DocumentJob { Initiator = initiator; WebsiteUri = rootUri; DocumentUri = h })
            |> runDoc (Some t)
        | _ -> visited

    let rec runImg links visited =
        match links with
        | Some (h::t) ->  
            visited
            |> runDownload downloadActor (ImageJob { Initiator = initiator; WebsiteUri = rootUri; ImageUri = h })
            |> runImg (Some t)
        | _ -> visited

    visitedLinks
    |> runDoc links
    |> runImg imgLinks

let processDownloadResult crawler parseActor downloadResult =
    match downloadResult with
    | DocumentResult { Initiator = initiator; WebsiteUri = root; HtmlContent = html } ->
        crawler <! downloadResult
        parseActor <! { Initiator = initiator; RootUri = root; HtmlString = html }
    | ImageResult _ -> crawler <! downloadResult
    | FailedResult _ -> crawler <! downloadResult
  
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

let downloadCoordinatorActor downloadActor parseActor (mailbox: Actor<_>) =
    let crawler = mailbox.Context.Parent
    let rec loop visitedLinks =
        actor {
            let! msg = mailbox.Receive()
            let visited' = match box msg with
            | :? DownloadJob as downloadJob -> runDownload downloadActor  downloadJob visitedLinks
            | :? DownloadResult as downloadResult -> processDownloadResult crawler parseActor downloadResult; visitedLinks
            | :? ParseJobResult as parseResult -> processParseResult downloadActor visitedLinks parseResult
            | _ -> failwith ""
            return! loop visited'
        }
    loop Set.empty