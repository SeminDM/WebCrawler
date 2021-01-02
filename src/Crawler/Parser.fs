module Crawler.Parser

open ParseTypes
open System
open HtmlAgilityPack

    
let htmlLinkNode = "//a[@href]"
let htmlHrefAttr = "href"
let htmlImageNode = "//img[@src]"
let htmlSrcAttr = "src"


let toAsboluteUri rootUri rawUriStr = 
    if Uri.IsWellFormedUriString(rawUriStr, UriKind.Absolute)
    then new Uri(rawUriStr, UriKind.Absolute)
    else new Uri(rootUri, rawUriStr);

let absoluteUriIsInDomain (rootUri: Uri) (rawUri: Uri) = rootUri.Host = rawUri.Host;

let IsWellFormedUriString rawUriStr = Uri.IsWellFormedUriString(rawUriStr, UriKind.Absolute)

let searchLinks rootUri htmlContent =
    let doc = new HtmlDocument()
    doc.LoadHtml htmlContent
    htmlLinkNode
    |> doc.DocumentNode.SelectNodes 
    |> Seq.map (fun htmlNode -> htmlNode.Attributes.[htmlHrefAttr].Value)
    |> Seq.filter IsWellFormedUriString
    |> Seq.map (toAsboluteUri rootUri)
    |> Seq.filter (absoluteUriIsInDomain rootUri)
    |> Seq.toList

let searchImageLinks rootUri htmlContent =
    let doc = new HtmlDocument();
    doc.LoadHtml(htmlContent);
    htmlImageNode
    |> doc.DocumentNode.SelectNodes
    |> Seq.map (fun htmlNode -> htmlNode.Attributes.[htmlSrcAttr].Value)
    |> Seq.filter IsWellFormedUriString
    |> Seq.map (toAsboluteUri rootUri)
    |> Seq.toList

let parseDocument rootUri htmlContent =
    { Links = searchLinks rootUri htmlContent; ImageLinks = searchImageLinks rootUri htmlContent }

