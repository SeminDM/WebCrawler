module CrawlerTest.Tests

open Crawler
open Crawler.Downloader
open NUnit.Framework
open System

let documentUri() = new Uri("https://yandex.ru/")
let imageUri() = new Uri("https://yandex.ru/images/search?from=tabbar&text=messi&pos=9&img_url=https%3A%2F%2Fpbs.twimg.com%2Fmedia%2FDb-XwSLWAAogBYn.jpg&rpt=simage")

[<SetUp>]
let Setup () =
    ()

[<Test>]
let Download () =
    documentUri()
    |> DownloadDocument
    |> function  
        | DownloadDocument { Uri = uri; Content = content } ->
            Assert.AreEqual(documentUri(), uri)
            Assert.Greater(content.Length, 0)
        | DownloadImage _ -> Assert.Fail()
        | FailedDownload _ -> Assert.Fail()

    imageUri()
    |> DownloadImage
    |> function
        | DownloadImage { Uri = uri; Size = size } ->
            Assert.AreEqual(imageUri(), uri)
            Assert.Greater(size, 0)
        | DownloadDocument _ -> Assert.Fail()
        | FailedDownload _ -> Assert.Fail()