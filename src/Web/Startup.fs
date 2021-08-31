module Start

open CrawlerHub
open ActorTypes
open Akka.Actor
open Akka.FSharp
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
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
        services.AddControllers() |> ignore
        services
            .AddSingleton<ActorSystem>(fun _ -> create "webSystem" <| Configuration.load())
            .AddSingleton<CrawlerActorRef>(createCrawler)
            .AddSingleton<SignalRActorRef>(createSignalR)
            |> ignore

    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment, lifetime: IHostApplicationLifetime) =
        if env.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore

        app
           .UseRouting()
           .UseEndpoints(fun endpoints ->
               endpoints.MapControllers() |> ignore
               endpoints.MapHub<CrawlHub>("/crawler") |> ignore)
           .UseGiraffe(webApp)
           
        lifetime.ApplicationStarted.Register(fun _ ->
            app.ApplicationServices.GetService<SignalRActorRef>() |> ignore
            ) |> ignore
        
        lifetime.ApplicationStopped.Register(fun _ -> app.ApplicationServices.GetService<ActorSystem>().Terminate() |> Async.AwaitTask |> ignore) |> ignore