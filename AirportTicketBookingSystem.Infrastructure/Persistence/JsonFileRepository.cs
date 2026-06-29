using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json;
using AirportTicketBookingSystem.Domain.Common;

namespace AirportTicketBookingSystem.Infrastructure.Persistence
{
    public abstract class JsonFileRepository<TEntity>
    where TEntity : Entity
    {
        private readonly string _filePath;

        protected JsonFileRepository(string fileName)
        {
            var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");

            if (!Directory.Exists(dataDirectory))
                Directory.CreateDirectory(dataDirectory);

            _filePath = Path.Combine(dataDirectory, fileName);

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        protected async Task<IReadOnlyList<TEntity>> ReadAllAsync()
        {
            var json = await File.ReadAllTextAsync(_filePath);

            if (string.IsNullOrWhiteSpace(json))
                return [];

            var entities = JsonSerializer.Deserialize<List<TEntity>>(json, JsonOptions());

            return entities ?? [];
        }

        protected async Task WriteAllAsync(IEnumerable<TEntity> entities)
        {
            var json = JsonSerializer.Serialize(entities, JsonOptions());

            await File.WriteAllTextAsync(_filePath, json);
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            };
        }
    }
}
