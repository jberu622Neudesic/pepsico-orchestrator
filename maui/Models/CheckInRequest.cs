using System.Collections.Specialized;

namespace MauiShellApp.Models;

/// <summary>
/// Represents the data received when an external app invokes the MAUI shell via deep link
/// </summary>
public class CheckInRequest
{
    /// <summary>
    /// Full deep link URI as received by the app
    /// </summary>
    public Uri RawUri { get; set; }

    /// <summary>
    /// All query parameters as key-value pairs
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; }

    /// <summary>
    /// UTC timestamp when deep link was received
    /// </summary>
    public DateTime ReceivedTimestamp { get; set; }

    /// <summary>
    /// User identifier if provided in parameters
    /// </summary>
    public string? UserId => Parameters.ContainsKey("userId") ? Parameters["userId"] : null;

    /// <summary>
    /// Location name if provided in parameters
    /// </summary>
    public string? Location => Parameters.ContainsKey("location") ? Parameters["location"] : null;

    /// <summary>
    /// Event name if provided in parameters
    /// </summary>
    public string? Event => Parameters.ContainsKey("event") ? Parameters["event"] : null;

    /// <summary>
    /// All parameters NOT in the known set (userId, location, event, timestamp, requestId, returnUrl)
    /// </summary>
    public Dictionary<string, string> CustomFields
    {
        get
        {
            var knownKeys = new HashSet<string> { "userId", "location", "event", "timestamp", "requestId", "returnUrl" };
            return Parameters
                .Where(kvp => !knownKeys.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    public CheckInRequest(Uri uri, Dictionary<string, string> parameters)
    {
        RawUri = uri;
        Parameters = parameters;
        ReceivedTimestamp = DateTime.UtcNow;
    }
}
