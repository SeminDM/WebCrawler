module Crawler.DownloadTypes

open System

type DownloadDocumentJob = { Uri: Uri }
type DownloadImageJob = { Uri: Uri }

type DownloadJob = 
    | DownloadDocumentJob of DownloadDocumentJob
    | DownloadImageJob of DownloadImageJob

type DownloadDocumentJobResult = { DocumentUri: Uri; HtmlContent: string }
type DownloadImageJobResult = { ImageUri: Uri; ImageContent: byte[] }
type DownloadFailedJobResult = { Uri: Uri; Reason: string }

type DownloadJobResult =
    | DownloadDocumentJobResult of DownloadDocumentJobResult
    | DownloadImageJobResult of DownloadImageJobResult
    | DownloadFailedJobResult of DownloadFailedJobResult