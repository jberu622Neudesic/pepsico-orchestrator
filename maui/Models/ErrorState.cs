namespace MauiShellApp.Models;

/// <summary>
/// Represents an error state with user-friendly and technical details
/// </summary>
public class ErrorState
{
	/// <summary>
	/// Error code for categorization
	/// </summary>
	public string ErrorCode { get; set; }

	/// <summary>
	/// User-friendly error message
	/// </summary>
	public string UserMessage { get; set; }

	/// <summary>
	/// Technical details for debugging (not shown to user)
	/// </summary>
	public string? TechnicalDetails { get; set; }

	/// <summary>
	/// UTC timestamp when error occurred
	/// </summary>
	public DateTime OccurredAt { get; set; }

	/// <summary>
	/// Suggested recovery action for user
	/// </summary>
	public string? RecoveryAction { get; set; }

	public ErrorState(string errorCode, string userMessage)
	{
		ErrorCode = errorCode;
		UserMessage = userMessage;
		OccurredAt = DateTime.UtcNow;
	}
}
