module Crawler.DownloadTypes

open Akka.Actor
open System

type DownloadDocumentJob = { Initiator: IActorRef; DocumentUri: Uri }
type DownloadImageJob = { Initiator: IActorRef; ImageUri: Uri }

type DownloadJob = 
    | DocumentJob of DownloadDocumentJob
    | ImageJob of DownloadImageJob

type DownloadDocumentJobResult = { Initiator: IActorRef; DocumentUri: Uri; HtmlContent: string }
type DownloadImageJobResult = { Initiator: IActorRef; ImageUri: Uri; ImageContent: byte[] }
type DownloadFailedJobResult = { Initiator: IActorRef; Uri: Uri; Reason: string }

type DownloadJobResult =
    | DocumentJobResult of DownloadDocumentJobResult
    | ImageJobResult of DownloadImageJobResult
    | FailedJobResult of DownloadFailedJobResult