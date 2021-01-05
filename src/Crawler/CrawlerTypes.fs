module Crawler.CrawlerTypes

open Akka.Actor
open System

type CrawlDocumentJob = { Initiator: IActorRef; DocumentUri: Uri }

type CrawlDocResult = { DocumentUri: Uri; Size: int }
type CrawlImageResult = { ImageUri: Uri; Size: int }
type CrawlFailedResult = { RootUri: Uri; Reason: string }

type CrawlResult = 
    | CrawlDocResult of CrawlDocResult
    | CrawlImageResult of CrawlImageResult
    | CrawlFailedResult of CrawlFailedResult