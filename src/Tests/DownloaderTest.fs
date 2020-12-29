module CrawlerTest.Tests

open Crawler.Downloader
open Crawler.Parser
open Crawler.Types
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
        | DownloadDocumentResult { Uri = rootUri; HtmlContent = content } ->
            Assert.AreEqual(documentUri(), rootUri)
            Assert.Greater(content.Length, 0)
            let { Links = links; ImageLinks = _ } = parseDocument rootUri content 
            Assert.Greater(List.length links, 0)
            List.iter (fun uri -> Assert.IsTrue(absoluteUriIsInDomain rootUri uri)) links

        | DownloadImageResult _ -> Assert.Fail()
        | FailedDownloadResult { Uri = uri; Reason = reason } -> Assert.Fail($"{uri} {reason}")

    imageUri()
    |> downloadImage
    |> function
        | DownloadImageResult { Uri = rootUri; Size = size } ->
            Assert.AreEqual(imageUri(), rootUri)
            Assert.Greater(size, 0)
        | DownloadDocumentResult _ -> Assert.Fail()
        | FailedDownloadResult { Uri = uri; Reason = reason } -> Assert.Fail($"{uri} {reason}")