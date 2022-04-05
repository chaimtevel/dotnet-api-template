using Microsoft.Extensions.Logging;
using System.Collections;

namespace Shared.Validation;

public class ServiceValidation : IEnumerable<ValidationMessage>
{
    public bool IsValid => _errors.Count <= 0;

    private readonly HashSet<ValidationMessage> _errors = new();

    public ServiceValidation Add(string key, string message, LogLevel? level = default)
    {
        if (level.HasValue)
        {
            this.Add(new ValidationMessage(key, message) { Level = level.Value });
        }
        else
        {
            this.Add(new ValidationMessage(key, message));
        }

        return this;
    }

    public ServiceValidation Add(ValidationMessage message)
    {
        _errors.Add(message);
        return this;
    }

    public ServiceValidation Add((string, string) error, params object[] info)
    {
        var (code, msg) = error;
        _errors.Add(new ValidationMessage(code, string.Format(msg, info)));

        return this;
    }

    public ServiceValidation Add((string, string) error, LogLevel level, params object[] info)
    {
        var (code, msg) = error;
        _errors.Add(new ValidationMessage(code, string.Format(msg, info)) { Level = level });

        return this;
    }

    public ServiceValidation CopyTo(ServiceValidation serviceValidation)
    {
        foreach (var (code, message, level) in this)
        {
            serviceValidation.Add(code, message, level);
        }

        return this;
    }

    public ServiceValidation Merge(IEnumerable<ServiceValidation> validations)
    {
        foreach (var validation in validations)
        {
            validation.CopyTo(this);
        }

        return this;
    }

    public string JoinMessages(LogLevel levelFilter, string separator = ", ")
    {
        return JoinMessages(separator, m => m.Level == levelFilter);
    }

    public string JoinMessages(string separator = ", ", Func<ValidationMessage, bool>? filter = default)
    {
        var messages = this.Select(m => m);

        if (filter is not null)
        {
            messages = messages.Where(filter);
        }

        return string.Join(separator ?? Environment.NewLine, messages.Select(m => m.Message));
    }

    public IEnumerator<ValidationMessage> GetEnumerator() => _errors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _errors.GetEnumerator();
}
