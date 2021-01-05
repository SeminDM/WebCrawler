module Crawler.DownloadTypes

open Akka.Actor
open System

type DownloadDocumentJob = { Initiator: IActorRef; DocumentUri: Uri }
type DownloadImageJob = { Initiator: IActorRef; ImageUri: Uri }

type DownloadJob = 
    | DownloadDocumentJob of DownloadDocumentJob
    | DownloadImageJob of DownloadImageJob

type DownloadDocumentJobResult = { Initiator: IActorRef; DocumentUri: Uri; HtmlContent: string }
type DownloadImageJobResult = { Initiator: IActorRef; ImageUri: Uri; ImageContent: byte[] }
type DownloadFailedJobResult = { Initiator: IActorRef; Uri: Uri; Reason: string }

type DownloadJobResult =
    | DownloadDocumentJobResult of DownloadDocumentJobResult
    | DownloadImageJobResult of DownloadImageJobResult
    | DownloadFailedJobResult of DownloadFailedJobResult