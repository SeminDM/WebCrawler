module Crawler.CrawlerTypes

open Akka.Actor
open System

type CrawlDocumentJob = { Initiator: IActorRef; DocumentUri: Uri }

type CrawlDocumentResult = { Initiator: IActorRef; DocumentUri: Uri; Size: int }
type CrawlImageResult = { Initiator: IActorRef; ImageUri: Uri; Size: int }
type CrawlFailedResult = { Initiator: IActorRef; RootUri: Uri; Reason: string }

type CrawlResult = 
    | DocumentResult of CrawlDocumentResult
    | ImageResult of CrawlImageResult
    | FailedResult of CrawlFailedResult