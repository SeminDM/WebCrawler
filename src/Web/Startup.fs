namespace Web

open Crawler.Client
open Crawler.Crawler
open Web.CrawlerHub
open Akka.Actor
open Akka.FSharp
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System
           
type Startup() =
    let webApp =
        choose [
            route "/ping" >=> text "pong"
            route "/" >=> htmlFile "wwwroot\index.html" ]
    
    member _.ConfigureServices(services: IServiceCollection) =
        services.AddSignalR() |> ignore
        services.AddSingleton<ActorSystem>(fun _ -> create "webSystem" <| Configuration.load()) |> ignore

    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment, lifetime: IHostApplicationLifetime) =
        if env.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore

        app
           .UseRouting()
           .UseEndpoints(fun endpoints -> endpoints.MapHub<CrawlHub>("/crawler") |> ignore)
           .UseGiraffe(webApp)
           
        lifetime.ApplicationStarted.Register(fun _ ->
            let system = app.ApplicationServices.GetService<ActorSystem>()
            let crawler = spawn system "crawlerActor" <| crawlerActor

            let printer = app.ApplicationServices.GetService<IHubContext<CrawlHub>>() |> SignalRPrinter
            let signalrActor = spawn system "signalrActor" <| (clientActor crawler printer)

            signalrActor <! "https://docs.microsoft.com/ru-ru/" // "https://www.eurosport.ru/
            ) |> ignore
        
        lifetime.ApplicationStopped.Register(fun _ -> app.ApplicationServices.GetService<ActorSystem>().Terminate() |> Async.AwaitTask |> ignore) |> ignore