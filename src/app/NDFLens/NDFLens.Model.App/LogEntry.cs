namespace NDFLens.Model.App
{
    /// <summary>
    /// A single log event captured from a logging source.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Unique identifier of the entry within the current buffer.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Local time the event occurred.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Severity level of the event.
        /// </summary>
        public LogSeverity Level { get; set; }

        /// <summary>
        /// Name of the service that emitted the event.
        /// </summary>
        public string Service { get; set; } = string.Empty;

        /// <summary>
        /// Log message text.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// HTTP method of the associated request.
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Request path of the associated request.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// HTTP status code returned.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Request duration in milliseconds.
        /// </summary>
        public int DurationMs { get; set; }

        /// <summary>
        /// Host machine that emitted the event.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Deployment region of the host.
        /// </summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>
        /// Operating-system process id.
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// Worker thread that handled the request.
        /// </summary>
        public string Thread { get; set; } = string.Empty;

        /// <summary>
        /// Response size in bytes.
        /// </summary>
        public int Bytes { get; set; }

        /// <summary>
        /// Identifier of the user associated with the request.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Distributed trace identifier.
        /// </summary>
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Span identifier within the trace.
        /// </summary>
        public string SpanId { get; set; } = string.Empty;
    }
}
