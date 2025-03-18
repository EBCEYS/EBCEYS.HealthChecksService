using Docker.DotNet;
using Docker.DotNet.Models;

namespace EBCEYS.HealthChecksService.Docker.Extensions
{
    public static class ContainerListResponseExtensions
    {
        public static string GetName(this ContainerListResponse container)
        {
            return container.Names.FirstOrDefault() ?? container.ID;
        }
        public static async Task<Stream> GetLogStream(this MultiplexedStream stream, CancellationToken token = default)
        {
            Stream inst = new MemoryStream();
            Stream outst = new MemoryStream();
            Stream errst = new MemoryStream();
            await stream.CopyOutputToAsync(inst, outst, errst, token);
            errst.Seek(0, SeekOrigin.Begin);
            await errst.CopyToAsync(outst, token);
            return outst;
        }
        /// <summary>
        /// Gets the container info by name or id.
        /// </summary>
        /// <param name="containers">The containers.</param>
        /// <param name="nameOrID">The container name or id.</param>
        /// <returns>The <see cref="ContainerListResponse"/> if container exists; otherwise <c>null</c>.</returns>
        public static ContainerListResponse? GetByNameOrID(this IEnumerable<ContainerListResponse> containers, string nameOrID)
        {
            return containers.FirstOrDefault(c => c.ID == nameOrID ||
            c.Names.FirstOrDefault(n => n.TrimStart('/') == nameOrID) != null);
        }
    }
}
