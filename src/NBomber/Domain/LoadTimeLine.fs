module internal NBomber.Domain.Concurrency.LoadTimeLine

open System
open FsToolkit.ErrorHandling

open NBomber
open NBomber.Extensions
open NBomber.Contracts
open NBomber.Domain
open NBomber.Errors

let validateSimulation (simulation: LoadSimulation) =
    result {
        let checkCopies (copies) =
            if copies <= 0 then AppError.createResult(CopiesCountIsZeroOrNegative(simulation.ToString()))
            else Ok copies

        let checkDuration (duration) =
            if duration < TimeSpan.FromSeconds(1.0) then AppError.createResult(DurationIsLessThan1Sec(simulation.ToString()))
            else Ok duration

        match simulation with
        | KeepConcurrentScenarios (copies, duration)
        | RampConcurrentScenarios (copies, duration)
        | InjectScenariosPerSec (copies, duration)
        | RampScenariosPerSec (copies, duration) ->
            let! _ = checkCopies copies
            let! _ = checkDuration duration
            return simulation
    }

let rec createTimeLine (startTime: TimeSpan, simulations: LoadSimulation list) =
    result {
        match simulations with
        | [] -> return List.empty
        | simulation :: tail ->
            match! validateSimulation simulation with
            | KeepConcurrentScenarios (_, duration)
            | RampConcurrentScenarios (_, duration)
            | InjectScenariosPerSec (_, duration)
            | RampScenariosPerSec (_, duration) ->
                let endTime = startTime + duration
                let timeLineItem = { EndTime = endTime; LoadSimulation = simulation }
                let! timeLine = createTimeLine(endTime, tail)
                return timeLineItem :: timeLine
    }

let unsafeCreateWithDuration (loadSimulations: Contracts.LoadSimulation list) =
    let timeLine = createTimeLine(TimeSpan.Zero, loadSimulations) |> Result.getOk
    let timeItem = timeLine |> List.last
    {| LoadTimeLine = timeLine; ScenarioDuration = timeItem.EndTime |}

let createSimulationFromSettings (settings: Configuration.LoadSimulationSettings) =
    match settings with
    | Configuration.KeepConcurrentScenarios (c,d) -> KeepConcurrentScenarios(c, d.TimeOfDay)
    | Configuration.RampConcurrentScenarios (c,d) -> LoadSimulation.RampConcurrentScenarios(c, d.TimeOfDay)
    | Configuration.InjectScenariosPerSec (c,d)   -> LoadSimulation.InjectScenariosPerSec(c, d.TimeOfDay)
    | Configuration.RampScenariosPerSec (c,d)     -> LoadSimulation.RampScenariosPerSec(c, d.TimeOfDay)

let getRunningSimulation (timeLine: LoadTimeLine, currentTime: TimeSpan) =
    timeLine
    |> List.tryFind(fun x -> currentTime <= x.EndTime)
    |> Option.map(fun x -> x.LoadSimulation)
