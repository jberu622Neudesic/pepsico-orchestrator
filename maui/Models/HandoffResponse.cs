namespace MauiShellApp.Models;

/// <summary>
/// Handoff response status enumeration
/// </summary>
public enum HandoffStatus
{
	SUCCESS,
	FAILURE,
	CANCEL
}

/// <summary>
/// Represents the data sent back to orchestrator when check-in workflow completes
/// </summary>
public class HandoffResponse
{
	/// <summary>
	/// Response status (SUCCESS, FAILURE, or CANCEL)
	/// </summary>
	public HandoffStatus Status { get; set; }

	/// <summary>
	/// UTC timestamp when check-in completed
	/// </summary>
	public DateTime CompletedTimestamp { get; set; }

	/// <summary>
	/// Original request ID from orchestrator (echoed back)
	/// </summary>
	public string? OriginalRequestId { get; set; }

	/// <summary>
	/// User-readable message describing the result
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	/// Error details if Status = FAILURE (null if SUCCESS)
	/// </summary>
	public ErrorState? ErrorDetails { get; set; }

	/// <summary>
	/// Optional key-value data to return to orchestrator
	/// </summary>
	public Dictionary<string, string>? ReturnData { get; set; }

	public HandoffResponse(HandoffStatus status, string message)
	{
		Status = status;
		Message = message;
		CompletedTimestamp = DateTime.UtcNow;
	}
}
