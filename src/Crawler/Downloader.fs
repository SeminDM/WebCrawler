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
    | DownloadDocumentResult 
    | DownloadImageResult
    | FailedDownloadResult

module Downloader =

    let DownloadDocument (uri: Uri) =
        let req = WebRequest.Create uri
        use resp = req.GetResponse()
        let reader = new StreamReader(resp.GetResponseStream())
        let content = reader.ReadToEnd()
        { Uri = uri; Content = content }

    let DownloadImage (uri: Uri) =
        let req = WebRequest.Create uri
        use resp = req.GetResponse()
        { Uri = uri; Size = resp.ContentLength }

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