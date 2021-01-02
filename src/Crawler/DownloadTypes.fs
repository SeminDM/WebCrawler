module Crawler.DownloadTypes

open System

type DownloadDocumentJob = { Uri: Uri }
type DownloadDocumentWithImagesJob = { Uri: Uri }

type DownloadJob = 
    | DownloadDocumentJob of DownloadDocumentJob
    | DownloadDocumentWithImagesJob of DownloadDocumentWithImagesJob

type DownloadDocumentResult = { Uri: Uri; HtmlContent: string }
type DownloadImageResult = { Uri: Uri; ImageBytes: int[] }
type FailedDownloadResult = { Uri: Uri; Reason: string }

type DownloadResult =
    | DownloadDocumentResult of DownloadDocumentResult
    | DownloadImageResult of DownloadImageResult
    | DownloadFailedResult of FailedDownloadResult