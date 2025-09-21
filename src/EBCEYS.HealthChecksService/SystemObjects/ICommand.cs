namespace EBCEYS.HealthChecksService.SystemObjects;

public interface ICommand<in TContext, TResult> where TContext : class where TResult : class
{
    Task<TResult> ExecuteAsync(TContext context, CancellationToken token);
}