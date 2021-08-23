module Crawler.DownloadExecutor

open Crawler.Types
open Akka.FSharp
open System.IO
open System.Net.Http

type State = {
    Processed: string Set;
    Processing: string Set;
    ParsingCount: int;
}
with
    member this.IsUriProcessed uri = Set.contains uri this.Processed
    member this.IsUriProcessing uri = Set.contains uri this.Processing
    member this.IsFinishedState = this.ParsingCount = 0 && Set.isEmpty this.Processing
    
    static member AddProcessingUri uri state = { state with Processing = (Set.add (uri.ToString()) state.Processing) }
    static member ReplaceProcessingUri uri state = { state with Processing = (Set.remove (uri.ToString()) state.Processing); Processed = (Set.add (uri.ToString()) state.Processed) }
    static member IncrementParsingCount state = { state with ParsingCount = (state.ParsingCount + 1) }
    static member DecrementParsingCount state = { state with ParsingCount = (state.ParsingCount - 1) }

    static member Empty = { Processed = Set.empty; Processing = Set.empty; ParsingCount = 0}
end

let createHttpClient = new HttpClient()

let downloadDocument (httpClient: HttpClient) job  =
    let { Initiator = initiator; WebsiteUri = original; DocumentUri = uri } = job
    try
        async {
            let! html = Async.AwaitTask <| httpClient.GetStringAsync uri
            return html
        } |> Async.RunSynchronously |> fun html -> DocumentResult { Initiator = initiator; WebsiteUri = original; DownloadUri = uri; HtmlContent = html }
    with
    | e -> FailedResult { Initiator = initiator; WebsiteUri = original; DownloadUri = uri; Reason = e.Message }

let downloadImage (httpClient: HttpClient) job =
    let { Initiator = initiator; WebsiteUri = original; ImageUri = uri } = job
    try
        async {
            use! stream = Async.AwaitTask <| httpClient.GetStreamAsync uri
            let ms = new MemoryStream()
            stream.CopyTo(ms)
            return ms.ToArray()
        } |> Async.RunSynchronously |> fun img -> ImageResult { Initiator = initiator; WebsiteUri = original; DownloadUri = uri; ImageContent = img }
    with
    | e -> FailedResult { Initiator = initiator; WebsiteUri = original; DownloadUri = uri; Reason = e.Message }

let runDownload downloadActor downloadJob (state: State) =
    let getUri = function DocumentJob dj -> dj.DocumentUri.ToString() | ImageJob ij -> ij.ImageUri.ToString()
    let uri = getUri downloadJob
    if (state.IsUriProcessed uri || state.IsUriProcessing uri)
    then 
        state
    else 
        downloadActor <! downloadJob; 
        State.AddProcessingUri uri state
        
let processParseResult crawler downloadActor parseResult state =
    let { Initiator = initiator; RootUri = rootUri; Links = links; ImageLinks = imgLinks } = parseResult

    let uriToDocJob uri = DocumentJob { Initiator = initiator; WebsiteUri = rootUri; DocumentUri = uri }
    let uriToImgJob uri = ImageJob { Initiator = initiator; WebsiteUri = rootUri; ImageUri = uri }

    let jobs =
        match links, imgLinks with
        | Some list, None -> List.map uriToDocJob list
        | None, Some list -> List.map uriToImgJob list
        | Some list1, Some list2 -> List.map uriToDocJob list1 @ List.map uriToImgJob list2
        | _, _ -> List.Empty

    let state' = jobs |> List.fold (fun st job -> runDownload downloadActor job st) (State.DecrementParsingCount state)
    if state'.IsFinishedState
    then
        crawler <! { Initiator = initiator; Visited = Set.count state'.Processed }
        state'
    else
        state'

let processDownloadResult crawler parseActor downloadResult state =
    
    crawler <! downloadResult

    let (state', initiator ) = match downloadResult with
    | DocumentResult { Initiator = initiator; DownloadUri = uri; WebsiteUri = root; HtmlContent = html } ->
        parseActor <! { Initiator = initiator; RootUri = root; HtmlString = html }
        let state' = state |> State.ReplaceProcessingUri uri |> State.IncrementParsingCount
        (state', initiator)
    
    | ImageResult { Initiator = initiator; DownloadUri = uri } -> 
        let state' = state |> State.ReplaceProcessingUri uri
        (state', initiator)
          
    | FailedResult { Initiator = initiator; DownloadUri = uri } ->
        let state' = state |> State.ReplaceProcessingUri uri
        (state', initiator)

    if state'.IsFinishedState
    then
        crawler <! { Initiator = initiator; Visited = Set.count state'.Processed }
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
            | _ -> failwith "Unsupported message type"
            return! loop state'
        }
    loop State.Empty