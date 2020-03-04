module internal NBomber.Constants

open NBomber.Configuration

[<Literal>]
let DefaultScenarioDurationInSec = 60.0

[<Literal>]
let DefaultConcurrentCopiesCount = 50

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
let MinSendStatsIntervalSec = 10.0

//todo: opaque types instead of ms

[<Literal>]
let SchedulerTickIntervalMs = 250

[<Literal>]
let NotificationTickIntervalMs = 3_000

[<Literal>]
let ReTryCount = 10

[<Literal>]
let OperationTimeOut = 3_000
