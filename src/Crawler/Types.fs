module Crawler.Types

open System

type CrawlDocJob = { Uri: Uri }
type CrawlJobWithImages = { Uri: Uri }
type CrawlJob = CrawlDocJob | CrawlJobWithImages


type CrawlDocResult = { RootUri: Uri; Links: Uri list; ImageLinks: Uri list; Size: int }
type CrawlImageResult = { ImageUri: Uri; Size: int }
type CrawlFailedResult = { RootUri: Uri; Reason: string }
type CrawlResult = CrawlDocResult | CrawlImageResult

type DownloadDocumentResult = { Uri: Uri; HtmlContent: string }
type DownloadImageResult = { Uri: Uri; Size: int64 }
type FailedDownloadResult = { Uri: Uri; Reason: string }


type DownloadResult =
    | DownloadDocumentResult of DownloadDocumentResult
    | DownloadImageResult of DownloadImageResult
    | FailedDownloadResult of FailedDownloadResult


type ParseDocumentResult = { Links: Uri list; ImageLinks: Uri list }