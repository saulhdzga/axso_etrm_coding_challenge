# PowerPositionReporter

Console/worker-style .NET application for the AXSO ETRM coding challenge.

The application:

- Retrieves day-ahead power trades from the provided `PowerService.dll`.
- Aggregates trade volumes into 24 local wall-clock hourly rows from `23:00` through `22:00`.
- Writes `PowerPosition_YYYYMMDD_HHMM.csv` files with `Local Time,Volume` columns.
- Runs once at startup and then at the configured interval without skipping overdue extracts.
- Uses Europe/London local time for extract dates, output filenames, and report scheduling.
- Logs scheduler, extract, retry, and failure events through Serilog.

Configuration is read from `appsettings.json`, with the output path and interval also overridable by command-line arguments:

```json
{
  "Report": {
    "OutputPath": "reports",
    "IntervalMinutes": 15
  },
  "PowerService": {
    "Retry": {
      "MaxRetryAttempts": 2,
      "DelayMilliseconds": 250
    }
  }
}
```

Run from the solution root:

```powershell
dotnet run --project PowerPositionReporter
```

Run with command-line overrides:

```powershell
dotnet run --project PowerPositionReporter -- reports 15
```
