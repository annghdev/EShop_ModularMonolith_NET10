namespace Kernel.Application;
public interface IEmailService
{
    Task SendEmailAsync(SendEmailRequest request);
}
