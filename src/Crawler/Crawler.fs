module Crawler.Crawler

open Crawler.Downloader
open Types

let download uri =

    let result = uri |> downloadDocument
    match result with
    | DownloadDocumentResult { Uri = uri; HtmlContent = html} -> 2
    | FailedDownloadResult { Uri = uri; Reason = reason} -> 1

let crawl job =
    match job with
    | CrawlDocJob _ -> 2
    | CrawlJobWithImages _ -> 1