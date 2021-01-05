module Crawler.ParseActor

open Crawler.Parser
open Akka.FSharp

let parseActor (mailbox: Actor<_>) job = mailbox.Sender() <! parseDocument job
