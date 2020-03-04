﻿namespace rec NBomber.Configuration

open System
open FSharp.Json
open Microsoft.Extensions.Configuration

type ReportFormat =
    | Txt = 0
    | Html = 1
    | Csv = 2
    | Md = 3

type LoadSimulationSettings =
    | KeepConcurrentScenarios of copiesCount:int * during:DateTime
    | RampConcurrentScenarios of copiesCount:int * during:DateTime
    | InjectScenariosPerSec   of copiesCount:int * during:DateTime
    | RampScenariosPerSec     of copiesCount:int * during:DateTime

type ScenarioSetting = {
    ScenarioName: string
    WarmUpDuration: DateTime
    LoadSimulationsSettings: LoadSimulationSettings list
}

type GlobalSettings = {
    ScenariosSettings: ScenarioSetting list option
    TargetScenarios: string list option
    ReportFileName: string option
    ReportFormats: ReportFormat list option
    SendStatsInterval: DateTime option
}

type AgentSettings = {
    ClusterId: string
    TargetGroup: string
    MqttServer: string
    MqttPort: int option
}

type TargetGroupSettings = {
    TargetGroup: string
    TargetScenarios: string list
}

type CoordinatorSettings = {
    ClusterId: string
    TargetScenarios: string list
    MqttServer: string
    MqttPort: int option
    Agents: TargetGroupSettings list
}

type ClusterSettings =
    | Coordinator of CoordinatorSettings
    | Agent       of AgentSettings

type TestConfig = {
    TestSuite: string
    TestName: string
    GlobalSettings: GlobalSettings option
    ClusterSettings: ClusterSettings option
    [<JsonField(Transform=typeof<TestConfig.JsonStringTransform>)>]
    CustomSettings: string option
}

module internal TestConfig =

    type JsonStringTransform() =
        interface ITypeTransform with
            member x.targetType () = typeof<obj>
            member x.toTargetType(value) = value
            member x.fromTargetType(value) =
                let config = JsonConfig.create(allowUntyped = true)
                let str = Json.serializeEx config value
                str :> obj

    let parse (json) =
        let config = JsonConfig.create(allowUntyped = true)
        Json.deserializeEx<TestConfig> config json
