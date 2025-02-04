using Quartz;

public class AttachmentCleanupJob : IJob
{
    private readonly IServiceProvider _services;

    public AttachmentCleanupJob(IServiceProvider services)
    {
        _services = services;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {       
            using var scope = _services.CreateScope();
            var cleanupService = scope.ServiceProvider.GetRequiredService<AttachmentCleanupService>();
            
            await cleanupService.CleanupDeletedAttachments();
        }
        catch (Exception)
        {
            throw; // Rethrow so Quartz can handle retry if configured
        }
    }
}