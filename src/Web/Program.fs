open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open System.Timers
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Start

module Program =
    let createHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.UseStartup<Startup>() |> ignore
            )

    [<EntryPoint>]
    let main args =
        let host = createHostBuilder(args).Build().Run()
        
        0 // Exit code