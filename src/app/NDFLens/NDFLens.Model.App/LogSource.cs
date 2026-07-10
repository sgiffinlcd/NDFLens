namespace NDFLens.Model.App
{
    /// <summary>
    /// A logging source that Lens can attach to.
    /// </summary>
    public class LogSource
    {
        /// <summary>
        /// Environment the source runs in (prod, test, dev).
        /// </summary>
        public string Environment { get; set; } = string.Empty;

        /// <summary>
        /// Name of the source application or service.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
