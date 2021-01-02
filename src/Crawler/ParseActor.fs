module Crawler.ParseActor

open Crawler.Parser
open Crawler.ParseTypes
open Akka.FSharp

let parseActor (mailbox: Actor<ParseDocumentJob>) =
    let rec loop() = actor {
        let! { RootUri = root; HtmlString = html } = mailbox.Receive()
        mailbox.Sender() <! parseDocument root html
        return! loop()
    }
    loop()
