namespace Crawler

open System
open System.Net
open System.IO
open System.Text.RegularExpressions

type DownloadDocumentJob = {
    Uri: Uri 
}

type DownloadImageJob = {
    Uri: Uri 
}

type DownloadDocumentResult = {
    Uri: Uri
    Content: string
}

type DownloadImageResult = {
    Uri: Uri
    Size: int64
}

type ParseDocumentResult = {
    Links: Uri list
    ImageLinks: Uri list
}

type FailedDownloadResult = {
    Uri: Uri
    Reason: string
}

type DownloadJob = DownloadDocumentJob | DownloadImageJob

type DownloadResult =
    | DownloadDocument of DownloadDocumentResult
    | DownloadImage of DownloadImageResult
    | FailedDownload of FailedDownloadResult

module Downloader =

    let DownloadDocument (uri: Uri) =
        let req = WebRequest.Create uri
        try
            use resp = req.GetResponse()
            let reader = new StreamReader(resp.GetResponseStream())
            let content = reader.ReadToEnd()
            DownloadDocument { Uri = uri; Content = content }
        with
        | e -> FailedDownload { Uri = uri; Reason = e.Message }

    let DownloadImage (uri: Uri) : DownloadResult =
        let req = WebRequest.Create uri
        try
            use resp = req.GetResponse()
            let stream = resp.GetResponseStream()
            let ms = new MemoryStream()
            stream.CopyTo(ms)
            DownloadImage { Uri = uri; Size = ms.Length }
        with
        | e -> FailedDownload { Uri = uri; Reason = e.Message }

module Parser =
    
    let searchLinks content =
        let linkMatch = Regex.Matches(content,"href=\s*\"[^\"h]*(http://[^&\"]*)\"")
        [ for x in linkMatch -> x.Groups.[1].Value ]
        |> List.filter (fun str -> Uri.IsWellFormedUriString(str, UriKind.Absolute))
        |> List.map (fun str -> new Uri(str))

    let searchImages content =
        let imgMatch = Regex.Matches(content,"img=")
        [ for x in imgMatch -> x.Groups.[1].Value ]
        |> List.filter (fun str -> Uri.IsWellFormedUriString(str, UriKind.Absolute))
        |> List.map (fun str -> new Uri(str))

    let ParseDocument content =
        { Links = searchLinks content; ImageLinks = searchImages content }