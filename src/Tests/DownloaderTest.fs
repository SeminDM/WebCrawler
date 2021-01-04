module CrawlerTest.Tests

open Crawler.Downloader
open Crawler.Parser
open Crawler.DownloadTypes
open Crawler.ParseTypes
open NUnit.Framework
open System

let documentUri() = new Uri("https://docs.microsoft.com/ru-ru/")
let imageUri() = new Uri("https://docs.microsoft.com/ru-ru/")

[<SetUp>]
let Setup () =
    ()

[<Test>]
let DownloadAndParse () =
    documentUri()
    |> downloadDocument
    |> function  
        | DownloadDocumentJobResult { DocumentUri = rootUri; HtmlContent = content } ->
            Assert.AreEqual(documentUri(), rootUri)
            Assert.Greater(content.Length, 0)
            let { Links = links; ImageLinks = _ } = parseDocument rootUri content 
            Assert.Greater(List.length links, 0)
            List.iter (fun uri -> Assert.IsTrue(absoluteUriIsInDomain rootUri uri)) links

        | DownloadImageJobResult _ -> Assert.Fail()
        | DownloadFailedJobResult { Uri = uri; Reason = reason } -> Assert.Fail($"{uri} {reason}")

    imageUri()
    |> downloadImage
    |> function
        | DownloadImageJobResult { ImageUri = rootUri; ImageContent = img } ->
            Assert.AreEqual(imageUri(), rootUri)
            Assert.Greater(img.Length, 0)
        | DownloadDocumentJobResult _ -> Assert.Fail()
        | DownloadFailedJobResult { Uri = uri; Reason = reason } -> Assert.Fail($"{uri} {reason}")