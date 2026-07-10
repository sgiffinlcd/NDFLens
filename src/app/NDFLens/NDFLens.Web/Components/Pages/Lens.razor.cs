using Microsoft.AspNetCore.Components;
using MudBlazor;
using NDFLens.Model.App;
using NDFLens.Web.Services;
using System.Text.Json;

namespace NDFLens.Web.Components.Pages
{
    /// <summary>
    /// Lens log viewer screen. Implements the Lens.dc.html design comp with native MudBlazor
    /// components themed to the Lens palettes, against a simulated log feed until real logging
    /// sources are wired up through the data layer.
    /// </summary>
    public partial class Lens : IDisposable
    {
        private const int PageSize = 50;
        private const int MaxBuffer = 400;
        private const int SeedCount = 46;

        private static readonly LogSeverity[] AllLevels =
        {
            LogSeverity.Debug, LogSeverity.Info, LogSeverity.Warn, LogSeverity.Error, LogSeverity.Fatal
        };

        private static readonly LogSource[] Sources =
        {
            new() { Environment = "prod", Name = "api-gateway" },
            new() { Environment = "prod", Name = "payment-svc" },
            new() { Environment = "prod", Name = "checkout-web" },
            new() { Environment = "test", Name = "api-gateway" },
            new() { Environment = "test", Name = "search-index" },
            new() { Environment = "dev", Name = "api-gateway" },
            new() { Environment = "dev", Name = "worker-queue" }
        };

        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        /// <summary>
        /// Lens design palettes and typography mapped onto MudBlazor theme tokens.
        /// Fatal maps to Secondary, so severity chips can use semantic Mud colors.
        /// </summary>
        private static readonly MudTheme LensTheme = new()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#2563eb",
                Secondary = "#c8519f",
                Info = "#3b82f6",
                Warning = "#d29922",
                Error = "#ef4444",
                Success = "#22c55e",
                Background = "#fafbfc",
                Surface = "#ffffff",
                AppbarBackground = "#ffffff",
                AppbarText = "#141821",
                DrawerBackground = "#ffffff",
                DrawerText = "#5b6472",
                DrawerIcon = "#5b6472",
                TextPrimary = "#141821",
                TextSecondary = "#5b6472",
                ActionDefault = "#5b6472",
                LinesDefault = "#e7e9ee",
                TableLines = "#e7e9ee",
                Divider = "#e7e9ee",
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#2563eb",
                Secondary = "#c8519f",
                Info = "#3b82f6",
                Warning = "#d29922",
                Error = "#ef4444",
                Success = "#22c55e",
                Background = "#0b0e14",
                Surface = "#11151c",
                AppbarBackground = "#11151c",
                AppbarText = "#e6edf3",
                DrawerBackground = "#11151c",
                DrawerText = "#8b949e",
                DrawerIcon = "#8b949e",
                TextPrimary = "#e6edf3",
                TextSecondary = "#8b949e",
                ActionDefault = "#8b949e",
                LinesDefault = "#232a35",
                TableLines = "#232a35",
                Divider = "#232a35",
            },
            Typography = new Typography
            {
                Default = new DefaultTypography
                {
                    FontFamily = new[] { "Inter", "system-ui", "sans-serif" }
                }
            },
            LayoutProperties = new LayoutProperties
            {
                DrawerWidthLeft = "220px",
                DrawerMiniWidthLeft = "62px",
                DrawerWidthRight = "430px",
            }
        };

        private readonly SimulatedLogFeed _feed = new();

        private List<LogEntry> _logs = new();
        private IReadOnlyCollection<LogSeverity>? _selectedLevels = AllLevels;
        private string _query = string.Empty;
        private bool _live = true;
        private bool _dark;
        private bool _navExpanded = true;
        private bool _copied;
        private bool _paged;
        private int _page;
        private string _activeNav = "live";
        private long? _selectedId;
        private string? _traceFilter;
        private LogSource _source = Sources[0];
        private Timer? _timer;

        [Inject]
        private IJsApiService JsApi { get; set; } = default!;

        private IEnumerable<LogEntry> Filtered
        {
            get
            {
                var query = _query.Trim();
                return _logs.Where(l => (_selectedLevels?.Contains(l.Level) ?? false)
                    && (_traceFilter is null || l.TraceId == _traceFilter)
                    && (query.Length == 0 || $"{l.Message} {l.Service} {l.Path} {l.TraceId}".Contains(query, StringComparison.OrdinalIgnoreCase)));
            }
        }

        private LogEntry? SelectedEntry => _selectedId is null ? null : _logs.FirstOrDefault(l => l.Id == _selectedId);

        protected override void OnInitialized()
        {
            _logs = _feed.Seed(SeedCount);
            _timer = new Timer(_ => _ = InvokeAsync(Tick), null, 1000, 1000);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private void Tick()
        {
            if (_paged || !_live) return;
            var count = _feed.NextBatchSize();
            var now = DateTime.Now;
            for (var i = 0; i < count; i++)
            {
                _logs.Insert(0, _feed.Create(now.AddMilliseconds(i)));
            }
            if (_logs.Count > MaxBuffer)
            {
                _logs.RemoveRange(MaxBuffer, _logs.Count - MaxBuffer);
            }
            StateHasChanged();
        }

        private void OnFilterChanged()
        {
            _page = 0;
        }

        private void SetNav(string nav)
        {
            _activeNav = nav;
        }

        private string NavClass(string nav)
        {
            return _activeNav == nav ? "active" : string.Empty;
        }

        private int CountFor(LogSeverity level)
        {
            return _logs.Count(l => l.Level == level);
        }

        private void RowClicked(TableRowClickEventArgs<LogEntry> args)
        {
            if (args.Item is null) return;
            _selectedId = args.Item.Id;
            _copied = false;
        }

        private string RowClass(LogEntry entry, int index)
        {
            return entry.Id == _selectedId ? "lens-selected-row" : string.Empty;
        }

        private void FilterTrace(string traceId)
        {
            _traceFilter = traceId;
            _page = 0;
        }

        private void ClearTrace()
        {
            _traceFilter = null;
            _page = 0;
        }

        private void SelectSource(LogSource source)
        {
            _source = source;
        }

        private void ClearLogs()
        {
            _logs.Clear();
            _selectedId = null;
        }

        private async Task CopyTraceAsync(LogEntry entry)
        {
            try
            {
                await JsApi.CopyToClipboardAsync(entry.TraceId);
            }
            catch
            {
                // Clipboard access can be denied; the visual confirmation still shows.
            }
            _copied = true;
            StateHasChanged();
            await Task.Delay(1400);
            _copied = false;
        }

        private static Color LevelColor(LogSeverity level)
        {
            return level switch
            {
                LogSeverity.Info => Color.Info,
                LogSeverity.Warn => Color.Warning,
                LogSeverity.Error => Color.Error,
                LogSeverity.Fatal => Color.Secondary,
                _ => Color.Default,
            };
        }

        private static Color EnvColor(string environment)
        {
            return environment switch
            {
                "prod" => Color.Error,
                "test" => Color.Warning,
                _ => Color.Success,
            };
        }

        private static string LevelLabel(LogSeverity level)
        {
            return level.ToString().ToUpperInvariant();
        }

        private static string Truncate(string value, int length)
        {
            return value.Length <= length ? value : value[..length];
        }

        private static string Iso(DateTime timestamp)
        {
            return timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        private static IEnumerable<(string Key, string Value)> MetadataFor(LogEntry entry)
        {
            yield return ("trace_id", entry.TraceId);
            yield return ("span_id", entry.SpanId);
            yield return ("timestamp", Iso(entry.Timestamp));
            yield return ("level", LevelLabel(entry.Level));
            yield return ("service", entry.Service);
            yield return ("host", entry.Host);
            yield return ("region", entry.Region);
            yield return ("env", "prod");
            yield return ("method", entry.Method);
            yield return ("path", entry.Path);
            yield return ("status", entry.Status.ToString());
            yield return ("duration_ms", $"{entry.DurationMs} ms");
            yield return ("bytes", entry.Bytes.ToString("N0"));
            yield return ("pid", entry.ProcessId.ToString());
            yield return ("thread", entry.Thread);
            yield return ("user_id", entry.UserId);
        }

        private static string BuildJson(LogEntry entry)
        {
            return JsonSerializer.Serialize(new
            {
                timestamp = Iso(entry.Timestamp),
                level = LevelLabel(entry.Level),
                service = entry.Service,
                message = entry.Message,
                trace_id = entry.TraceId,
                span_id = entry.SpanId,
                http = new
                {
                    method = entry.Method,
                    path = entry.Path,
                    status = entry.Status,
                    duration_ms = entry.DurationMs,
                    bytes = entry.Bytes
                },
                host = entry.Host,
                region = entry.Region,
                pid = entry.ProcessId,
                thread = entry.Thread,
                user_id = entry.UserId,
                env = "prod"
            }, JsonOptions);
        }
    }
}
