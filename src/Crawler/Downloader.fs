module Crawler.Downloader

open DownloadTypes
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
    | e -> DownloadFailedResult { Uri = uri; Reason = e.Message }

let downloadImage (uri: Uri) =
    let req = WebRequest.Create uri
    try
        use resp = req.GetResponse()
        let stream = resp.GetResponseStream()
        let ms = new MemoryStream()
        stream.CopyTo(ms)
        DownloadImageResult { Uri = uri; Size = ms.Length }
    with
    | e -> DownloadFailedResult { Uri = uri; Reason = e.Message }