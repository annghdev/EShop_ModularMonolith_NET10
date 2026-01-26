namespace Contracts;

public class SendEmailRequest
{
    public string ToEmail { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Template { get; set; } = default!;
    public object Model { get; set; } = default!;

    public List<string> CcEmails { get; set; } = [];
    public List<string> BccEmails { get; set; } = [];
}

