module Crawler.Downloader

open DownloadTypes
open System.IO
open System.Net

let downloadDocument { Initiator = initiator; OriginalUri = original; DocumentUri = uri } =
    let req = WebRequest.Create uri
    req.Timeout <- ((int)(System.TimeSpan.FromMinutes(1.0)).TotalMilliseconds)
    try
        use resp = req.GetResponse()
        let reader = new StreamReader(resp.GetResponseStream())
        let html = reader.ReadToEnd()
        DocumentJobResult { Initiator = initiator; OriginalUri = original;DocumentUri = uri; HtmlContent = html }
    with
    | e -> FailedJobResult { Initiator = initiator; OriginalUri = original; Uri = uri; Reason = e.Message }

let downloadImage { Initiator = initiator; OriginalUri = original; ImageUri = uri } =
    let req = WebRequest.Create uri
    try
        use resp = req.GetResponse()
        let stream = resp.GetResponseStream()
        let ms = new MemoryStream()
        stream.CopyTo(ms)
        ImageJobResult { Initiator = initiator; OriginalUri = original; ImageUri = uri; ImageContent = ms.ToArray() }
    with
    | e -> FailedJobResult { Initiator = initiator; OriginalUri = original; Uri = uri; Reason = e.Message }