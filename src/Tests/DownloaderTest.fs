module CrawlerTest.Tests

open Crawler.Downloader
open Crawler.Parser
open Crawler.DownloadTypes
open Crawler.ParseTypes
open Akka.Actor
open NUnit.Framework
open System

let documentUri() = { DownloadDocumentJob.Initiator = ActorRefs.Nobody; DownloadDocumentJob.DocumentUri = new Uri("https://docs.microsoft.com/ru-ru/") }
let imageUri() = { DownloadImageJob.Initiator = ActorRefs.Nobody; DownloadImageJob.ImageUri = new Uri("https://docs.microsoft.com/ru-ru/") }

[<SetUp>]
let Setup () =
    ()

[<Test>]
let DownloadAndParse () =
    documentUri()
    |> downloadDocument
    |> function  
        | DocumentJobResult { Initiator = initiator; DocumentUri = rootUri; HtmlContent = content } ->
            Assert.AreEqual(documentUri(), rootUri)
            Assert.Greater(content.Length, 0)
            let { Links = links; ImageLinks = _ } = parseDocument { Initiator = initiator; RootUri = rootUri; HtmlString = content }  
            Assert.Greater((Option.get >> List.length) links, 0)
            List.iter (fun uri -> Assert.IsTrue(absoluteUriIsInDomain rootUri uri)) (Option.get links)

        | ImageJobResult _ -> Assert.Fail()
        | FailedJobResult { Uri = uri; Reason = reason } -> Assert.Fail($"{uri} {reason}")

    imageUri()
    |> downloadImage
    |> function
        | ImageJobResult { ImageUri = rootUri; ImageContent = img } ->
            Assert.AreEqual(imageUri(), rootUri)
            Assert.Greater(img.Length, 0)
        | DocumentJobResult _ -> Assert.Fail()
        | FailedJobResult { Uri = uri; Reason = reason } -> Assert.Fail($"{uri} {reason}")