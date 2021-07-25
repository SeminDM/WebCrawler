module CrawlerTest.Tests

open Crawler.DownloadExecutor
open Crawler.Parser
open Crawler.Types
open Akka.Actor
open NUnit.Framework
open System

let documentUri() = { DownloadDocumentJob.Initiator = ActorRefs.Nobody; DownloadDocumentJob.WebsiteUri = new Uri("https://docs.microsoft.com/ru-ru/"); DownloadDocumentJob.DocumentUri = new Uri("https://docs.microsoft.com/ru-ru/") }
let imageUri() = { DownloadImageJob.Initiator = ActorRefs.Nobody; DownloadImageJob.WebsiteUri = new Uri("https://docs.microsoft.com/ru-ru/"); DownloadImageJob.ImageUri = new Uri("https://docs.microsoft.com/ru-ru/") }

[<SetUp>]
let Setup () =
    ()

[<Test>]
let DownloadAndParse () =
    documentUri()
    |> downloadDocument createHttpClient
    |> function  
        | DocumentResult { Initiator = initiator; DownloadUri = rootUri; HtmlContent = content } ->
            Assert.AreEqual(documentUri(), rootUri)
            Assert.Greater(content.Length, 0)
            let { Links = links; ImageLinks = _ } = parseDocument { Initiator = initiator; RootUri = rootUri; HtmlString = content }  
            Assert.Greater((Option.get >> List.length) links, 0)
            List.iter (fun uri -> Assert.IsTrue(absoluteUriIsInDomain rootUri uri)) (Option.get links)

        | ImageResult _ -> Assert.Fail()
        | FailedResult { DownloadUri = uri; Reason = reason } -> Assert.Fail($"{uri} {reason}")

    imageUri()
    |> downloadImage createHttpClient
    |> function
        | ImageResult { DownloadUri = rootUri; ImageContent = img } ->
            Assert.AreEqual(imageUri(), rootUri)
            Assert.Greater(img.Length, 0)
        | DocumentResult _ -> Assert.Fail()
        | FailedResult { DownloadUri = uri; Reason = reason } -> Assert.Fail($"{uri} {reason}")