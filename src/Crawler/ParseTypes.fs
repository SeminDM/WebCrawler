module Crawler.ParseTypes

open Akka.Actor
open System

type ParseDocumentJob = { Initiator: IActorRef; RootUri: Uri; HtmlString: string }

type ParseJobResult = { Initiator: IActorRef; RootUri: Uri; Links: Uri list option; ImageLinks: Uri list option }