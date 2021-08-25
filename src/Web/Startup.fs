namespace Web

open Crawler.Crawler
open Crawler.Types
open System
open System.Threading
open System.Threading.Tasks
open Akka.Actor
open Akka.FSharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe

type MyHub() =
    inherit Hub()
    
    override this.OnConnectedAsync() =
        base.OnConnectedAsync() |> ignore
        this.Clients.All.SendAsync ("receive", "connected")

    member this.Send (message : string) =
        if this.Clients <> null
        then
            this.Clients.All.SendAsync ("receive", message) |> Async.AwaitTask
        else
            Task.Delay(1) |> Async.AwaitTask
            
type Startup() =
    let webApp =
        choose [
            route "/ping"   >=> text "pong"
            route "/"  >=> htmlFile "index.html" ]
    
    member _.ConfigureServices(services: IServiceCollection) =
        services.AddSignalR() |> ignore
        services.AddSingleton<ActorSystem>(fun _ -> System.create "webSystem" <| Configuration.load()) |> ignore

    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment, lifetime: IApplicationLifetime) =
        if env.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore

        app
           .UseRouting()
           .UseEndpoints(fun endpoints ->
                //endpoints.MapGet("/", fun context -> context.Response.WriteAsync("Hello World!")) |> ignore
                endpoints.MapHub<MyHub>("/myhub") |> ignore
            )
           .UseGiraffe(webApp)
           
       
        lifetime.ApplicationStarted.Register(fun _ ->
            let hub = app.ApplicationServices.GetService<IHubContext<MyHub>>()
            
            let system = app.ApplicationServices.GetService<ActorSystem>()
            let crawlerRef = spawn system "crawlerActor" <| crawlerActor

            let consoleActorRef = spawn system "singleRActor" <| fun mailbox ->
                let sendMessage (msg: string) = hub.Clients.All.SendAsync ("receive", msg)
                let timer = new System.Diagnostics.Stopwatch()
                let runCrawler uri =
                    sendMessage "Crawling is started" |> Async.AwaitTask |> ignore
                    timer.Start()
                    crawlerRef <! { CrawlJob.Initiator = mailbox.Self; CrawlJob.WebsiteUri = new Uri(uri) }
        
                let rec loop() =
                    actor {
                        let! msg = mailbox.Receive()
                        match box msg with
                        | :? string as uri -> uri |> runCrawler 
                        | :? CrawlResult as crawlResult -> crawlResult.ToString() |> sendMessage |> Async.AwaitTask |> ignore
                        | :? CrawlFinishResult as finishResult -> 
                            timer.Stop()
                            sendMessage $"Crawling is finished:\r\n\tvisited links count: {finishResult.Visited}\r\n\ttime elapsed, sec: {timer.Elapsed.TotalSeconds}" |> Async.AwaitTask |> ignore 
                        //| obj -> obj |> printUnknownType
                        return! loop()
                    }
                loop()
            consoleActorRef <! "https://www.eurosport.ru/"//"https://docs.microsoft.com/ru-ru/"
            ) |> ignore  
        
        lifetime.ApplicationStopped.Register(fun _ -> app.ApplicationServices.GetService<ActorSystem>().Terminate() |> Async.AwaitTask |> ignore) |> ignore 
 
