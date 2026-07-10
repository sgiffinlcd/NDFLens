namespace NDFLens.Model.App
{
    /// <summary>
    /// Severity level of a log entry.
    /// </summary>
    public enum LogSeverity
    {
        /// <summary>
        /// Diagnostic detail useful during development.
        /// </summary>
        Debug,

        /// <summary>
        /// Normal operational event.
        /// </summary>
        Info,

        /// <summary>
        /// Unexpected condition that did not prevent the operation.
        /// </summary>
        Warn,

        /// <summary>
        /// Operation failed.
        /// </summary>
        Error,

        /// <summary>
        /// Unrecoverable failure; the process is terminating or unusable.
        /// </summary>
        Fatal
    }
}
