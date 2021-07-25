module Crawler.Types

open Akka.Actor
open System

type CrawlJob = { Initiator: IActorRef; WebsiteUri: Uri }

type CrawlDocumentResult = { Initiator: IActorRef; DocumentUri: Uri; Size: int }
type CrawlImageResult = { Initiator: IActorRef; ImageUri: Uri; Size: int }
type CrawlFailedResult = { Initiator: IActorRef; RootUri: Uri; Reason: string }

type CrawlResult = 
    | Document of CrawlDocumentResult
    | Image of CrawlImageResult
    | Error of CrawlFailedResult

type DownloadDocumentJob = { Initiator: IActorRef; WebsiteUri: Uri; DocumentUri: Uri }
type DownloadImageJob = { Initiator: IActorRef; WebsiteUri: Uri; ImageUri: Uri }

type DownloadJob = 
    | DocumentJob of DownloadDocumentJob
    | ImageJob of DownloadImageJob

type DownloadDocumentResult = { Initiator: IActorRef; WebsiteUri: Uri; DownloadUri: Uri; HtmlContent: string }
type DownloadImageResult = { Initiator: IActorRef; WebsiteUri: Uri; DownloadUri: Uri; ImageContent: byte[] }
type DownloadFailedResult = { Initiator: IActorRef; WebsiteUri: Uri; DownloadUri: Uri; Reason: string }

type DownloadResult =
    | DocumentResult of DownloadDocumentResult
    | ImageResult of DownloadImageResult
    | FailedResult of DownloadFailedResult

type ParseDocumentJob = { Initiator: IActorRef; RootUri: Uri; HtmlString: string }

type ParseJobResult = { Initiator: IActorRef; RootUri: Uri; Links: Uri list option; ImageLinks: Uri list option }