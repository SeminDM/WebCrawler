module Crawler.Downloader

open DownloadTypes
open System.IO
open System.Net.Http

let createHttpClient = new HttpClient()

let downloadDocument (httpClient: HttpClient) { Initiator = initiator; OriginalUri = original; DocumentUri = uri } =
    try
        let html = httpClient.GetStringAsync uri
        DocumentJobResult { Initiator = initiator; OriginalUri = original;DocumentUri = uri; HtmlContent = html.Result }
    with
    | e -> FailedJobResult { Initiator = initiator; OriginalUri = original; Uri = uri; Reason = e.Message }

let downloadImage (httpClient: HttpClient) { Initiator = initiator; OriginalUri = original; ImageUri = uri } =
    try
        use resp = (httpClient.GetStreamAsync uri).Result
        let ms = new MemoryStream()
        resp.CopyTo(ms)
        ImageJobResult { Initiator = initiator; OriginalUri = original; ImageUri = uri; ImageContent = ms.ToArray() }
    with
    | e -> FailedJobResult { Initiator = initiator; OriginalUri = original; Uri = uri; Reason = e.Message }