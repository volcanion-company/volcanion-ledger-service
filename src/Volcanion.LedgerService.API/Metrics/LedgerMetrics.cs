using Prometheus;

namespace Volcanion.LedgerService.API.Metrics;

public static class LedgerMetrics
{
    // Transaction counters
    public static readonly Counter TransactionsTotal = Prometheus.Metrics.CreateCounter(
        "ledger_transactions_total",
        "Total number of transactions processed",
        new CounterConfiguration
        {
            LabelNames = ["type", "status"]
        });

    public static readonly Counter TransactionsFailed = Prometheus.Metrics.CreateCounter(
        "ledger_transactions_failed_total",
        "Total number of failed transactions",
        new CounterConfiguration
        {
            LabelNames = ["type", "reason"]
        });

    // Balance metrics
    public static readonly Gauge CurrentBalance = Prometheus.Metrics.CreateGauge(
        "ledger_account_balance",
        "Current account balance",
        new GaugeConfiguration
        {
            LabelNames = ["currency"]
        });

    // API latency
    public static readonly Histogram ApiRequestDuration = Prometheus.Metrics.CreateHistogram(
        "ledger_api_request_duration_seconds",
        "API request duration in seconds",
        new HistogramConfiguration
        {
            LabelNames = ["method", "endpoint", "status_code"],
            Buckets = [0.01, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10]
        });

    // Database operation metrics
    public static readonly Histogram DatabaseOperationDuration = Prometheus.Metrics.CreateHistogram(
        "ledger_database_operation_duration_seconds",
        "Database operation duration in seconds",
        new HistogramConfiguration
        {
            LabelNames = ["operation", "table"],
            Buckets = [0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1]
        });

    // Concurrent operations
    public static readonly Gauge ConcurrentOperations = Prometheus.Metrics.CreateGauge(
        "ledger_concurrent_operations",
        "Number of concurrent operations",
        new GaugeConfiguration
        {
            LabelNames = ["operation_type"]
        });

    // Insufficient balance attempts
    public static readonly Counter InsufficientBalanceAttempts = Prometheus.Metrics.CreateCounter(
        "ledger_insufficient_balance_total",
        "Total number of insufficient balance attempts");

    // Helper methods
    public static void RecordTransaction(string type, string status)
    {
        TransactionsTotal.WithLabels(type, status).Inc();
    }

    public static void RecordFailedTransaction(string type, string reason)
    {
        TransactionsFailed.WithLabels(type, reason).Inc();
    }

    public static void RecordInsufficientBalance()
    {
        InsufficientBalanceAttempts.Inc();
    }

    public static IDisposable TrackApiRequest(string method, string endpoint, int statusCode)
    {
        return ApiRequestDuration
            .WithLabels(method, endpoint, statusCode.ToString())
            .NewTimer();
    }

    public static IDisposable TrackDatabaseOperation(string operation, string table)
    {
        return DatabaseOperationDuration
            .WithLabels(operation, table)
            .NewTimer();
    }

    public static IDisposable TrackConcurrentOperation(string operationType)
    {
        ConcurrentOperations.WithLabels(operationType).Inc();
        
        return new DisposableAction(() => 
            ConcurrentOperations.WithLabels(operationType).Dec());
    }

    private class DisposableAction : IDisposable
    {
        private readonly Action _action;
        private bool _disposed;

        public DisposableAction(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _action();
                _disposed = true;
            }
        }
    }
}
