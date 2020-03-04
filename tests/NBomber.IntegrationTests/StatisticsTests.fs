module Tests.StatisticsTests

open System

open Xunit
open FsCheck.Xunit
open Swensen.Unquote

open NBomber.Contracts
open NBomber.Domain
open NBomber.Domain.Statistics
open NBomber.Extensions

let private latencyCount = { Less800 = 1; More800Less1200 = 1; More1200 = 1 }

let private scenario = {
    ScenarioName = "Scenario1"
    TestInit = None
    TestClean = None
    Steps = Array.empty
    LoadSimulations = [| LoadSimulation.KeepConcurrentScenarios(copiesCount = 1, during = TimeSpan.FromSeconds(1.0)) |]
    WarmUpDuration = TimeSpan.FromSeconds(1.0)
}

[<Property>]
let ``calcRPS() should not fail and calculate correctly for any args values`` (latencies: Latency[], scnDuration: TimeSpan) =
    let result = Statistics.calcRPS(latencies, scnDuration)

    if latencies.Length = 0 then
        test <@ result = 0 @>

    elif latencies.Length <> 0 && scnDuration.TotalSeconds < 1.0 then
        test <@ result = latencies.Length @>

    else
        let expected = latencies.Length / int(scnDuration.TotalSeconds)
        test <@ result = expected @>

[<Property>]
let ``calcMin() should not fail and calculate correctly for any args values`` (latencies: Latency[]) =
    let result   = Statistics.calcMin(latencies)
    let expected = Array.minOrDefault 0 latencies
    test <@ result = expected @>

[<Property>]
let ``calcMean() should not fail and calculate correctly for any args values`` (latencies: Latency[]) =
    let result = latencies |> Statistics.calcMean
    let expected = latencies |> Array.averageByOrDefault 0.0 float |> int
    test <@ result = expected @>

[<Property>]
let ``calcMax() should not fail and calculate correctly for any args values`` (latencies: Latency[]) =
    let result = latencies |> Statistics.calcMax
    let expected = Array.maxOrDefault 0 latencies
    test <@ result = expected @>

[<Fact>]
let ``NodeStats.merge should correctly calculate all cluster stats`` () =

    let agentNodeInfo = { MachineName = "agent"; Sender = NodeType.Agent; CurrentOperation = NodeOperationType.Bombing }
    let coordinatorInfo = { agentNodeInfo with MachineName = "coordinator"; Sender = NodeType.Coordinator }

    let okLatencies = [| 1; 2; 3; 4|]
    let failCount = 5

    let stepStats = {
        StepName = "step1"; OkLatencies = okLatencies; RequestCount = 0; OkCount = 0; FailCount = failCount
        RPS = 0; Min = 0; Mean = 0; Max = 0; Percent50 = 0; Percent75 = 0; Percent95 = 0; StdDev = 0;
        DataTransfer = { MinKb = 1.0; MeanKb = 1.0; MaxKb = 1.0; AllMB = 1.0 }
    }

    let scenarioStats = {
        ScenarioName = "scenario1"; StepsStats = [| stepStats |]; RPS = 0;
        OkCount = 0; FailCount = 0; LatencyCount = latencyCount
        Duration = TimeSpan.FromSeconds(1.0)
    }

    let agentStats = {
        AllScenariosStats = [| scenarioStats |]; OkCount = 0; FailCount = 0
        LatencyCount = latencyCount; NodeStatsInfo = agentNodeInfo
    }

    let coordinatorStats = { agentStats with NodeStatsInfo = coordinatorInfo }

    let clusterInfo = { agentNodeInfo with MachineName = "cluster"; Sender = NodeType.Cluster }
    let mergedClusterStats = NodeStats.merge(clusterInfo, [|agentStats; coordinatorStats|], TimeSpan.Zero)

    test <@ mergedClusterStats.NodeStatsInfo.Sender = NodeType.Cluster @>
    test <@ mergedClusterStats.OkCount = 8 @>
    test <@ mergedClusterStats.FailCount = 10 @>
    test <@ mergedClusterStats.FailCount = 10 @>
    test <@ mergedClusterStats.LatencyCount.Less800 = 8 @>
    test <@ mergedClusterStats.AllScenariosStats.[0].RPS = 8 @>
    test <@ mergedClusterStats.AllScenariosStats.[0].StepsStats.[0].RequestCount = 18 @>
