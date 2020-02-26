module HelloWorldScenario

open System
open System.Threading.Tasks

open FSharp.Control.Tasks.V2.ContextInsensitive

open NBomber.Contracts
open NBomber.FSharp

let run () =

    let step1 = Step.create("step_1", fun context -> task {
        // you can do any logic here: go to http, websocket etc

        do! Task.Delay(TimeSpan.FromSeconds(0.1))
        return Response.Ok(42) // this value will be passed as response for the next step
    })

    let step2 = Step.create("step_2", fun context -> task {
        // you can do any logic here: go to http, websocket etc

        let value = context.GetPreviousStepResponse<int>() // 42
        return Response.Ok()
    })

    let scenario = Scenario.create "Hello World!" [step1; step2]

    NBomberRunner.registerScenarios [scenario]
    |> NBomberRunner.runInConsole
