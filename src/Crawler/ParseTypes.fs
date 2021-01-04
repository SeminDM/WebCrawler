module Crawler.ParseTypes

open System

type ParseDocumentJob = { RootUri: Uri; HtmlString: string }

type ParseJobResult = { Uri: Uri; Links: Uri list; ImageLinks: Uri list }