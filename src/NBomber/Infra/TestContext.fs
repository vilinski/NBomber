﻿module internal NBomber.Infra.TestContext

open System

open NBomber
open NBomber.Extensions
open NBomber.Configuration
open NBomber.Contracts

let empty =
    { TestSuite = Constants.DefaultTestSuite
      TestName = Constants.DefaultTestName
      RegisteredScenarios = Array.empty
      TestConfig = None
      InfraConfig = None
      ReportFileName = None
      ReportFormats = Constants.AllReportFormats
      ReportingSinks = Array.empty
      SendStatsInterval = TimeSpan.FromSeconds Constants.MinSendStatsIntervalSec }

let getTestSuite (context: TestContext) =
    context.TestConfig
    |> Option.bind(fun x -> x.TestSuite)
    |> Option.defaultValue context.TestSuite

let getTestName (context: TestContext) =
    context.TestConfig
    |> Option.bind(fun x -> x.TestName)
    |> Option.defaultValue context.TestName

let getScenariosSettings (context: TestContext) =
    context.TestConfig
    |> Option.bind(fun x -> x.GlobalSettings)
    |> Option.bind(fun x -> x.ScenariosSettings)
    |> Option.defaultValue List.empty
    |> List.toArray

let tryGetClusterSettings (context: TestContext) =
    context.TestConfig
    |> Option.bind(fun x -> x.ClusterSettings)

let getNodeType (context: TestContext) =
    context.TestConfig
    |> Option.bind(fun x -> x.ClusterSettings)
    |> Option.map(function
        | ClusterSettings.Coordinator _ -> NodeType.Coordinator
        | ClusterSettings.Agent _       -> NodeType.Agent)
    |> Option.defaultValue NodeType.SingleNode

let getTargetScenarios (context: TestContext) =
    let targetScn =
        context.TestConfig
        |> Option.bind(fun x -> x.GlobalSettings)
        |> Option.bind(fun x -> x.TargetScenarios)

    let allScns = context.RegisteredScenarios
                  |> Array.map(fun x -> x.ScenarioName)
                  |> Array.toList

    defaultArg targetScn allScns
    |> List.toArray

let getReportFileName (sessionId: string, context: TestContext) =
    let tryGetFromConfig (ctx) = maybe {
        let! config = ctx.TestConfig
        let! settings = config.GlobalSettings
        return! settings.ReportFileName
    }
    context
    |> tryGetFromConfig
    |> Option.orElse(context.ReportFileName)
    |> Option.defaultValue("report_" + sessionId)

let getReportFormats (context: TestContext) =
    let tryGetFromConfig (ctx) = maybe {
        let! config = ctx.TestConfig
        let! settings = config.GlobalSettings
        let! formats = settings.ReportFormats
        return formats |> List.toArray
    }
    context
    |> tryGetFromConfig
    |> Option.orElse(if Array.isEmpty context.ReportFormats then None
                     else Some context.ReportFormats)
    |> Option.defaultValue Constants.AllReportFormats

let getCustomSettings (context: TestContext) =
    maybe {
        let! config = context.TestConfig
        return! config.CustomSettings
    }
    |> function Some v -> v | None -> ""

let getSendStatsInterval (context: TestContext) =
    let tryGetFromConfig (ctx) = maybe {
        let! config = ctx.TestConfig
        let! settings = config.GlobalSettings
        let! intervalInDataTime = settings.SendStatsInterval
        return intervalInDataTime.TimeOfDay
    }
    context
    |> tryGetFromConfig
    |> Option.defaultValue context.SendStatsInterval
