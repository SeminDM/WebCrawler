open Microsoft.AspNetCore.Hosting
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