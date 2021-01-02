module Crawler.ParseTypes

open System

type ParseDocumentJob = { RootUri: Uri; HtmlString: string }

type ParseDocumentResult = { Links: Uri list; ImageLinks: Uri list }