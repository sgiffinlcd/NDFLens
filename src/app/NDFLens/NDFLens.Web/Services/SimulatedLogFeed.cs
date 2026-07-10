using NDFLens.Model.App;

namespace NDFLens.Web.Services
{
    /// <summary>
    /// Generates simulated log entries for the Lens viewer until real logging sources are wired up.
    /// Distributions and vocabulary mirror the Lens.dc.html design comp.
    /// </summary>
    public class SimulatedLogFeed
    {
        private static readonly string[] Services =
        {
            "api-gateway", "auth-service", "payment-svc", "user-db", "cache-redis",
            "worker-queue", "cdn-edge", "notif-service", "search-index", "billing-cron"
        };

        private static readonly string[] Paths =
        {
            "/v1/orders", "/v1/users/8f2a", "/auth/token", "/healthz", "/v2/search",
            "/billing/invoice", "/cache/get", "/queue/enqueue", "/cdn/app.js", "/notify/send"
        };

        private static readonly string[] Methods = { "GET", "GET", "GET", "POST", "POST", "PUT", "DELETE" };

        private static readonly string[] Regions = { "us-east-1", "us-west-2", "eu-west-1", "ap-south-1" };

        private static readonly Dictionary<LogSeverity, string[]> Messages = new()
        {
            [LogSeverity.Debug] = new[]
            {
                "parsed request headers", "cache lookup complete", "query plan generated", "token validated",
                "config reloaded", "acquired pool connection", "serialized response body", "feature flag evaluated"
            },
            [LogSeverity.Info] = new[]
            {
                "request completed", "user login successful", "cache warm complete", "job processed",
                "health check ok", "connection established", "session created", "webhook delivered"
            },
            [LogSeverity.Warn] = new[]
            {
                "slow query detected", "retry attempt 2 of 3", "rate limit approaching", "deprecated endpoint used",
                "memory usage above 80%", "connection pool saturated", "clock drift detected", "payload larger than expected"
            },
            [LogSeverity.Error] = new[]
            {
                "connection refused by upstream", "unhandled exception in handler", "payment declined by processor",
                "timeout waiting for upstream", "failed to acquire lock", "invalid auth token",
                "downstream returned 500", "circuit breaker opened"
            },
            [LogSeverity.Fatal] = new[]
            {
                "out of memory, terminating", "database unreachable", "panic: nil pointer dereference", "disk full, cannot write WAL"
            }
        };

        private const string HexDigits = "0123456789abcdef";

        private readonly Random _random = new();
        private readonly List<string> _traces = new();
        private long _nextId;

        /// <summary>
        /// Creates an initial buffer of entries spread over the recent past, newest first.
        /// </summary>
        public List<LogEntry> Seed(int count)
        {
            var entries = new List<LogEntry>(count);
            var timestamp = DateTime.Now.AddMilliseconds(-count * 760);
            for (var i = 0; i < count; i++)
            {
                timestamp = timestamp.AddMilliseconds(_random.Next(280, 1201));
                entries.Add(Create(timestamp));
            }
            entries.Reverse();
            return entries;
        }

        /// <summary>
        /// Number of entries the next live tick should emit.
        /// </summary>
        public int NextBatchSize()
        {
            return _random.NextDouble() < 0.28 ? 2 : 1;
        }

        /// <summary>
        /// Creates a single simulated entry at the supplied timestamp.
        /// </summary>
        public LogEntry Create(DateTime timestamp)
        {
            var level = PickLevel();
            return new LogEntry
            {
                Id = _nextId++,
                Timestamp = timestamp,
                Level = level,
                Service = Pick(Services),
                Message = Pick(Messages[level]),
                Method = Pick(Methods),
                Path = Pick(Paths),
                Status = StatusFor(level),
                DurationMs = DurationFor(level),
                Host = $"ip-10-{_random.Next(0, 61)}-{_random.Next(0, 256)}-{_random.Next(0, 256)}",
                Region = Pick(Regions),
                ProcessId = _random.Next(1000, 10000),
                Thread = $"worker-{_random.Next(1, 17)}",
                Bytes = _random.Next(120, 48201),
                UserId = "usr_" + Hex(8),
                TraceId = TraceFor(),
                SpanId = Hex(16)
            };
        }

        private LogSeverity PickLevel()
        {
            var r = _random.NextDouble() * 100;
            if (r < 42) return LogSeverity.Info;
            if (r < 74) return LogSeverity.Debug;
            if (r < 89) return LogSeverity.Warn;
            if (r < 98) return LogSeverity.Error;
            return LogSeverity.Fatal;
        }

        private int StatusFor(LogSeverity level)
        {
            return level switch
            {
                LogSeverity.Fatal => Pick(new[] { 500, 503 }),
                LogSeverity.Error => Pick(new[] { 500, 502, 503, 408, 401 }),
                LogSeverity.Warn => Pick(new[] { 200, 200, 429, 408, 304 }),
                _ => Pick(new[] { 200, 200, 200, 201, 204, 304 })
            };
        }

        private int DurationFor(LogSeverity level)
        {
            return level switch
            {
                LogSeverity.Fatal => _random.Next(1000, 8201),
                LogSeverity.Error => _random.Next(500, 4201),
                LogSeverity.Warn => _random.Next(380, 1701),
                _ => _random.Next(2, 541)
            };
        }

        private string TraceFor()
        {
            if (_traces.Count > 0 && _random.NextDouble() < 0.55)
            {
                var recent = Math.Min(8, _traces.Count);
                return _traces[_traces.Count - recent + _random.Next(recent)];
            }
            var trace = Hex(32);
            _traces.Add(trace);
            if (_traces.Count > 14) _traces.RemoveAt(0);
            return trace;
        }

        private string Hex(int length)
        {
            Span<char> chars = stackalloc char[length];
            for (var i = 0; i < length; i++) chars[i] = HexDigits[_random.Next(16)];
            return new string(chars);
        }

        private T Pick<T>(T[] values)
        {
            return values[_random.Next(values.Length)];
        }
    }
}
