module Crawler.ParseTypes

open Akka.Actor
open System

type ParseDocumentJob = { Initiator: IActorRef; RootUri: Uri; HtmlString: string }

type ParseJobResult = { Initiator: IActorRef; Uri: Uri; Links: Uri list; ImageLinks: Uri list }