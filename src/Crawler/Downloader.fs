module Crawler.Downloader

open Types
open System
open System.IO
open System.Net


let downloadDocument (uri: Uri) =
    let req = WebRequest.Create uri
    try
        use resp = req.GetResponse()
        let reader = new StreamReader(resp.GetResponseStream())
        let html = reader.ReadToEnd()
        DownloadDocumentResult { Uri = uri; HtmlContent = html }
    with
    | e -> FailedDownloadResult { Uri = uri; Reason = e.Message }

let downloadImage (uri: Uri) : DownloadResult =
    let req = WebRequest.Create uri
    try
        use resp = req.GetResponse()
        let stream = resp.GetResponseStream()
        let ms = new MemoryStream()
        stream.CopyTo(ms)
        DownloadImageResult { Uri = uri; Size = ms.Length }
    with
    | e -> FailedDownloadResult { Uri = uri; Reason = e.Message }