using Microsoft.Extensions.Logging;

namespace Shared.Validation;

public record ValidationMessage
{
	public ValidationMessage() : this(string.Empty)
	{

	}

	public ValidationMessage(string message) : this(message, string.Empty)
	{
		
	}

	public ValidationMessage(string code, string message)
	{
		Message = message;
		Code = code;
	}

	public string Message { get; init; }
	public string Code { get; init; }
	public DateTime? TimeStamp { get; init; }
	public LogLevel? Level { get; init; }

	public void Deconstruct(out string code, out string message)
	{
		code = Code;
		message = Message;
	}

	public void Deconstruct(out string code, out string message, out LogLevel? level)
	{
		code = Code;
		message = Message;
		level = Level;
	}
}