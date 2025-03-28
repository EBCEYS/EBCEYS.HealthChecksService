﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace EBCEYS.HealthChecksService.CustomHealthChecks.Services
{
    public partial class ServiceHealthChecksOptions
    {
        public required Dictionary<SupportedHealthCheckServices, IEnumerable<ServiceOptions>> HealthChecks { get; set; }

        #region JsonOptions
        public static JsonSerializerOptions JsonOpts { get; } = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter<SupportedHealthCheckServices>() }
        };
        public static ServiceHealthChecksOptions? CreateFromJson(byte[] json)
        {
            return JsonSerializer.Deserialize<ServiceHealthChecksOptions>(json, JsonOpts);
        }
        public static ServiceHealthChecksOptions? CreateFromJsonFile(string? filePath)
        {
            if (filePath == null)
            {
                return null;
            }
            return CreateFromJson(File.ReadAllBytes(filePath));
        }
        #endregion
    }
    public enum SupportedHealthCheckServices
    {
        None,
        RabbitMQ,
        PostgreSQL,
        Redis,
        MongoDB,
        ElasticSearch
    }
    public partial class ServiceOptions
    {
        public required string ServiceName { get; set; }
        public required string ConnectionString { get; set; }
        public required string Container { get; set; }
    }
}
