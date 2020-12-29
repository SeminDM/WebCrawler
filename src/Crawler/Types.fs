module Crawler.Types

open System

type DownloadDocumentJob = { Uri: Uri }

type DownloadImageJob = { Uri: Uri }


type DownloadDocumentResult = { Uri: Uri; HtmlContent: string }

type DownloadImageResult = { Uri: Uri; Size: int64 }

type FailedDownloadResult = { Uri: Uri; Reason: string }

type DownloadJob = DownloadDocumentJob | DownloadImageJob

type DownloadResult =
    | DownloadDocumentResult of DownloadDocumentResult
    | DownloadImageResult of DownloadImageResult
    | FailedDownloadResult of FailedDownloadResult


type ParseDocumentResult = { Links: Uri list; ImageLinks: Uri list }