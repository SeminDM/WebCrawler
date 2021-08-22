module Crawler.DownloadExecutor

open Crawler.Types
open Akka.FSharp
open Microsoft.FSharp.Control.WebExtensions
open System.IO
open System.Net
open System.Net.Http

type state = {
    Processed: string Set;
    Processing: string Set;
    ParsingCount: int;
}

let createHttpClient = new HttpClient()

let downloadDocument (httpClient: HttpClient) { Initiator = initiator; WebsiteUri = original; DocumentUri = uri } =
    try
        async {
            let! html = Async.AwaitTask <| httpClient.GetStringAsync uri
            return html
        } |> Async.RunSynchronously |> fun html -> DocumentResult { Initiator = initiator; WebsiteUri = original; DownloadUri = uri; HtmlContent = html }
    with
    | e -> FailedResult { Initiator = initiator; WebsiteUri = original; DownloadUri = uri; Reason = e.Message }

let downloadImage (httpClient: HttpClient) { Initiator = initiator; WebsiteUri = original; ImageUri = uri } =
    try
        async {
            use! stream = Async.AwaitTask <| httpClient.GetStreamAsync uri
            let ms = new MemoryStream()
            stream.CopyTo(ms)
            return ms.ToArray()
        } |> Async.RunSynchronously |> fun img -> ImageResult { Initiator = initiator; WebsiteUri = original; DownloadUri = uri; ImageContent = img }
    with
    | e -> FailedResult { Initiator = initiator; WebsiteUri = original; DownloadUri = uri; Reason = e.Message }

let runDownload downloadActor downloadJob (state: state) =
    let getUri = function DocumentJob dj -> dj.DocumentUri.ToString() | ImageJob ij -> ij.ImageUri.ToString()
    let uri = getUri downloadJob
    if (Set.contains uri state.Processed || Set.contains uri state.Processing)
    then 
        state
    else 
        match downloadJob with
        | DocumentJob dj -> downloadActor <! downloadJob; { state with Processing = (Set.add uri state.Processing) }
        | ImageJob ij -> downloadActor <! downloadJob; { state with Processing = (Set.add uri state.Processing) }
        
let processParseResult crawler downloadActor parseResult state =
    let { Initiator = initiator; RootUri = rootUri; Links = links; ImageLinks = imgLinks } = parseResult

    let rec runDoc links state =
        match links with
        | Some (h::t) ->  
            state 
            |> runDownload downloadActor (DocumentJob { Initiator = initiator; WebsiteUri = rootUri; DocumentUri = h })
            |> runDoc (Some t)
        | _ -> state

    let rec runImg links state =
        match links with
        | Some (h::t) ->  
            state
            |> runDownload downloadActor (ImageJob { Initiator = initiator; WebsiteUri = rootUri; ImageUri = h })
            |> runImg (Some t)
        | _ -> state

    let state' = { state with ParsingCount = state.ParsingCount - 1 } |> runDoc links |> runImg imgLinks

    if state'.ParsingCount = 0 && Set.isEmpty state'.Processing
    then
        crawler <! { Initiator = initiator; Visited = Set.count state'.Processed }
        state'
    else
        state'

let processDownloadResult crawler parseActor downloadResult state =
    
    let i = match downloadResult with
    | DocumentResult { Initiator = initiator } -> initiator
    | ImageResult { Initiator = initiator } -> initiator
    | FailedResult { Initiator = initiator } -> initiator


    let state' = match downloadResult with
    | DocumentResult { Initiator = initiator; DownloadUri = downloadUri; WebsiteUri = root; HtmlContent = html } ->
        crawler <! downloadResult
        parseActor <! { Initiator = initiator; RootUri = root; HtmlString = html }
        { state with Processing = (Set.remove (downloadUri.ToString()) state.Processing); Processed = (Set.add (downloadUri.ToString()) state.Processed); ParsingCount = state.ParsingCount + 1 }
    
    | ImageResult { Initiator = initiator; DownloadUri = downloadUri } -> 
        crawler <! downloadResult
        { state with Processing = (Set.remove (downloadUri.ToString()) state.Processing); Processed = (Set.add (downloadUri.ToString()) state.Processed) }
          
    | FailedResult { Initiator = initiator; DownloadUri = downloadUri } ->
        crawler <! downloadResult
        { state with Processing = (Set.remove (downloadUri.ToString()) state.Processing); Processed = (Set.add (downloadUri.ToString()) state.Processed) }

    if state'.ParsingCount = 0 && Set.isEmpty state'.Processing
    then
        crawler <! { Initiator = i; Visited = Set.count state'.Processed }
        state'
    else
        state'
  
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
    let rec loop state =
        actor {
            let! msg = mailbox.Receive()
            let state' = match box msg with
            | :? DownloadJob as downloadJob -> runDownload downloadActor downloadJob state
            | :? DownloadResult as downloadResult -> processDownloadResult crawler parseActor downloadResult state
            | :? ParseJobResult as parseResult -> processParseResult crawler downloadActor parseResult state
            | _ -> failwith ""
            return! loop state'
        }
    loop { Processed = Set.empty; Processing = Set.empty; ParsingCount = 0}