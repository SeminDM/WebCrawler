module Crawler.Parser

open Types
open Akka.FSharp
open HtmlAgilityPack
open System

    
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

let wrapToOption value = if value = null then None else Some value

let bind f = function Some args -> (f >> Some) args | None -> None

let searchLinks rootUri htmlContent =
    let doc = new HtmlDocument()
    doc.LoadHtml htmlContent
    htmlLinkNode
    |> doc.DocumentNode.SelectNodes
    |> wrapToOption
    |> bind (Seq.map (fun htmlNode -> htmlNode.Attributes.[htmlHrefAttr].Value))
    |> bind (Seq.filter IsWellFormedUriString)
    |> bind (Seq.map (toAsboluteUri rootUri))
    |> bind (Seq.filter (absoluteUriIsInDomain rootUri))
    |> bind Seq.toList

let searchImageLinks rootUri htmlContent =
    let doc = new HtmlDocument();
    doc.LoadHtml(htmlContent);
    htmlImageNode
    |> doc.DocumentNode.SelectNodes
    |> wrapToOption
    |> bind (Seq.map (fun htmlNode -> htmlNode.Attributes.[htmlSrcAttr].Value))
    |> bind (Seq.filter IsWellFormedUriString)
    |> bind (Seq.map (toAsboluteUri rootUri))
    |> bind Seq.toList

let parseDocument job =
    let { Initiator = initiator; RootUri = root; HtmlString = html } = job
    { Initiator = initiator; RootUri = root; Links = searchLinks root html; ImageLinks = searchImageLinks root html}

let parseActor (mailbox: Actor<_>) job = mailbox.Sender() <! parseDocument job

