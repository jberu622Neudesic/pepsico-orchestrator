namespace MauiShellApp.Models;

/// <summary>
/// Launch mode enumeration
/// </summary>
public enum LaunchMode
{
	STANDALONE,
	DEEP_LINK
}

/// <summary>
/// Tracks how the application was launched (standalone vs deep link)
/// </summary>
public class LaunchContext
{
	/// <summary>
	/// The mode in which the application was launched
	/// </summary>
	public LaunchMode LaunchMode { get; set; }

	/// <summary>
	/// Orchestrator return URL for handoff response (null if standalone)
	/// </summary>
	public string? OrchestratorReturnUrl { get; set; }

	/// <summary>
	/// Original request ID from orchestrator (null if standalone)
	/// </summary>
	public string? OriginalRequestId { get; set; }

	/// <summary>
	/// UTC timestamp when app was launched
	/// </summary>
	public DateTime LaunchTimestamp { get; set; }

	public LaunchContext(LaunchMode mode, string? returnUrl = null, string? requestId = null)
	{
		LaunchMode = mode;
		OrchestratorReturnUrl = returnUrl;
		OriginalRequestId = requestId;
		LaunchTimestamp = DateTime.UtcNow;
	}
}
