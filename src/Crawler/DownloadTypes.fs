module Crawler.DownloadTypes

open Akka.Actor
open System

type DownloadDocumentJob = { Initiator: IActorRef; OriginalUri: Uri; DocumentUri: Uri }
type DownloadImageJob = { Initiator: IActorRef; OriginalUri: Uri; ImageUri: Uri }

type DownloadJob = 
    | DocumentJob of DownloadDocumentJob
    | ImageJob of DownloadImageJob

type DownloadDocumentJobResult = { Initiator: IActorRef; OriginalUri: Uri; DocumentUri: Uri; HtmlContent: string }
type DownloadImageJobResult = { Initiator: IActorRef; OriginalUri: Uri; ImageUri: Uri; ImageContent: byte[] }
type DownloadFailedJobResult = { Initiator: IActorRef; OriginalUri: Uri; Uri: Uri; Reason: string }

type DownloadJobResult =
    | DocumentJobResult of DownloadDocumentJobResult
    | ImageJobResult of DownloadImageJobResult
    | FailedJobResult of DownloadFailedJobResult