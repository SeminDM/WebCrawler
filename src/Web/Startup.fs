module Start

open Crawler.Client
open Crawler.Crawler
open CrawlerHub
open Akka.Actor
open Akka.FSharp
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System

type CrawlerProvider = delegate of IServiceProvider -> IActorRef
type SignalRProvider = delegate of IServiceProvider -> IActorRef

type Startup() =
    let webApp =
        choose [
            route "/ping" >=> text "pong"
            route "/" >=> htmlFile "wwwroot\index.html" ]
    
    member _.ConfigureServices(services: IServiceCollection) =
        services.AddSignalR() |> ignore
        services.AddControllers() |> ignore
        services
            .AddSingleton<ActorSystem>(fun _ -> create "webSystem" <| Configuration.load())
            .AddSingleton<CrawlerProvider>(fun serviceProvider ->
                let system = serviceProvider.GetService<ActorSystem>()
                spawn system "crawlerActor" <| crawlerActor)
            .AddSingleton<SignalRProvider>(fun serviceProvider ->
                let system = serviceProvider.GetService<ActorSystem>()
                let crawler = serviceProvider.GetService<CrawlerProvider>()
                let printer = serviceProvider.GetService<IHubContext<CrawlHub>>() |> SignalRPrinter
                spawn system "signalrActor" <| (clientActor (crawler.Invoke(serviceProvider)) printer)) |> ignore

    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment, lifetime: IHostApplicationLifetime) =
        if env.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore

        app
           .UseRouting()
           .UseEndpoints(fun endpoints ->
               endpoints.MapControllers() |> ignore
               endpoints.MapHub<CrawlHub>("/crawler") |> ignore)
           .UseGiraffe(webApp)
           
//        lifetime.ApplicationStarted.Register(fun _ ->
//            let signalrActor = app.ApplicationServices.GetService<SignalRProvider>()
//            app.ApplicationServices |> signalrActor.Invoke <! "https://docs.microsoft.com/ru-ru/" // "https://www.eurosport.ru/
//            ) |> ignore
        
        lifetime.ApplicationStopped.Register(fun _ -> app.ApplicationServices.GetService<ActorSystem>().Terminate() |> Async.AwaitTask |> ignore) |> ignore