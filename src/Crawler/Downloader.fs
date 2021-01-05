module Crawler.Downloader

open DownloadTypes
open System.IO
open System.Net

let downloadDocument job =
    let { DownloadDocumentJob.Initiator = initiator; DownloadDocumentJob.DocumentUri = uri } = job
    let req = WebRequest.Create uri
    try
        use resp = req.GetResponse()
        let reader = new StreamReader(resp.GetResponseStream())
        let html = reader.ReadToEnd()
        DownloadDocumentJobResult { Initiator = initiator; DocumentUri = uri; HtmlContent = html }
    with
    | e -> DownloadFailedJobResult { Initiator = initiator; Uri = uri; Reason = e.Message }

let downloadImage job =
    let { DownloadImageJob.Initiator = initiator; DownloadImageJob.ImageUri = uri } = job
    let req = WebRequest.Create uri
    try
        use resp = req.GetResponse()
        let stream = resp.GetResponseStream()
        let ms = new MemoryStream()
        stream.CopyTo(ms)
        DownloadImageJobResult { Initiator = initiator; ImageUri = uri; ImageContent = ms.ToArray() }
    with
    | e -> DownloadFailedJobResult { Initiator = initiator; Uri = uri; Reason = e.Message }