namespace NBomber.Domain

open System
open System.Threading
open System.Threading.Tasks

open Serilog
open NBomber.Contracts
open NBomber.Extensions

//todo: use opaque types
type internal StepName = string
type internal ScenarioName = string
type internal Latency = int

module internal Constants =
    open NBomber.Configuration

    [<Literal>]
    let DefaultScenarioDurationInSec = 60.0

    [<Literal>]
    let DefaultConcurrentCopies = 50

    [<Literal>]
    let DefaultWarmUpDurationInSec = 10.0

    [<Literal>]
    let DefaultRepeatCount = 0

    [<Literal>]
    let DefaultDoNotTrack = false

    let AllReportFormats = [|ReportFormat.Txt; ReportFormat.Html; ReportFormat.Csv; ReportFormat.Md|]

    [<Literal>]
    let StepResponseKey = "nbomber_step_response"

    [<Literal>]
    let EmptyPoolName = "nbomber_empty_pool"

    [<Literal>]
    let EmptyFeedName = "nbomber_empty_feed"

    [<Literal>]
    let DefaultTestSuite = "nbomber_test_suite"

    [<Literal>]
    let DefaultTestName = "nbomber_load_test"

    [<Literal>]
    let MinSendStatsIntervalSec = 5.0

type internal ConnectionPool<'TConnection> = {
    PoolName: string
    OpenConnection: unit -> 'TConnection
    CloseConnection: ('TConnection -> unit) option
    ConnectionsCount: int
    AliveConnections: 'TConnection[]
} with
    interface IConnectionPool<'TConnection> with
        member x.PoolName = x.PoolName

[<CustomEquality; NoComparison>]
type internal UntypedConnectionPool = {
    PoolName: string
    OpenConnection: unit -> obj
    CloseConnection: (obj -> unit) option
    ConnectionsCount: int
    AliveConnections: obj[]
} with
    override x.GetHashCode() = x.PoolName.GetHashCode()
    override x.Equals(b) =
        match b with
        | :? UntypedConnectionPool as pool -> x.PoolName = pool.PoolName
        | _ -> false

type internal UntypedStepContext = {
    CorrelationId: CorrelationId
    CancellationToken: CancellationToken
    Connection: obj
    mutable Data: Dict<string,obj>
    FeedItem: obj
    Logger: ILogger
}

type internal UntypedFeed = {
    Name: string
    GetNextItem: CorrelationId * Dict<string,obj> -> obj
}

type internal Step = {
    StepName: StepName
    ConnectionPool: UntypedConnectionPool
    Execute: UntypedStepContext -> Task<Response>
    Context: UntypedStepContext option
    Feed: UntypedFeed
    RepeatCount: int
    DoNotTrack: bool
} with
    interface IStep with
        member x.StepName = x.StepName

type internal StepResponse = {
    Response: Response
    StartTimeMs: float
    LatencyMs: int
}

type internal Scenario = {
    ScenarioName: ScenarioName
    TestInit: (ScenarioContext -> Task) option
    TestClean: (ScenarioContext -> Task) option
    Steps: Step[]
    ConcurrentCopies: int
    CorrelationIds: CorrelationId[]
    WarmUpDuration: TimeSpan
    Duration: TimeSpan
}
