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
        DownloadDocumentJobResult { DocumentUri = uri; HtmlContent = html }
    with
    | e -> DownloadFailedJobResult { Uri = uri; Reason = e.Message }

let downloadImage (uri: Uri) =
    let req = WebRequest.Create uri
    try
        use resp = req.GetResponse()
        let stream = resp.GetResponseStream()
        let ms = new MemoryStream()
        stream.CopyTo(ms)
        DownloadImageJobResult { ImageUri = uri; ImageContent = ms.ToArray() }
    with
    | e -> DownloadFailedJobResult { Uri = uri; Reason = e.Message }