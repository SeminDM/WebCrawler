module Crawler.Crawler

open Crawler.DownloadExecutor
open Crawler.Types
open Akka.FSharp

type State = { CoordinatorIndex: int; FinishedCount: int }

let processCrawlJob (mailbox: Actor<_>) (crawlJob: CrawlJob) (state: State) =
    let newState = { state with CoordinatorIndex = state.CoordinatorIndex + 1 }
    let coordinator = spawn mailbox.Context $"downloadCoordinatorActor_{newState.CoordinatorIndex}" (downloadCoordinatorActor mailbox.Self)
    coordinator <! DocumentJob { Initiator = crawlJob.Initiator; WebsiteUri = crawlJob.WebsiteUri; DocumentUri = crawlJob.WebsiteUri }
    newState

let processDownloadResult downloadResult =
    match downloadResult with
    | DocumentResult { Initiator = initiator; DownloadUri = uri; HtmlContent = html } ->
        initiator <! Document { Initiator = initiator; DocumentUri = uri; Size = String.length html }

    | ImageResult { Initiator = initiator; DownloadUri = uri; ImageContent = img } ->
        initiator <! Image { Initiator = initiator; ImageUri = uri; Size = Array.length img }

    | FailedResult { Initiator = initiator; DownloadUri = uri; Reason = reason } ->
        initiator <! Error { Initiator = initiator; RootUri = uri; Reason = reason }

let crawlerActor (mailbox: Actor<_>) =
    let rec loop (state) =
        actor {
            let! msg = mailbox.Receive()
            let newState = match box msg with
            | :? CrawlJob as crawlJob -> processCrawlJob mailbox crawlJob state
            | :? DownloadResult as downloadResult -> processDownloadResult downloadResult; state
            | :? CrawlFinishResult as finishResult -> 
                finishResult.Initiator <! finishResult
                { state with FinishedCount = state.FinishedCount + 1 }
            | _ -> failwith ""
            return! loop newState
        }
    loop { CoordinatorIndex = 0; FinishedCount = 0 }