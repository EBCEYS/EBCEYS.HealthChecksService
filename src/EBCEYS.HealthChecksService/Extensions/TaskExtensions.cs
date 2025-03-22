namespace EBCEYS.HealthChecksService.Extensions
{
    public static class TaskExtensions
    {
        public static bool TryDispose(this Task task)
        {
            try
            {
                task.Dispose();
                return true;
            }
            catch (Exception) { return false; }
        }
    }
}
