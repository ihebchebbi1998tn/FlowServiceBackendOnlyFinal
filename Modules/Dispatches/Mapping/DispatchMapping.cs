using System;
using MyApi.Modules.Dispatches.DTOs;
using MyApi.Modules.Dispatches.Models;

namespace MyApi.Modules.Dispatches.Mapping
{
    // Lightweight mapping helpers (avoid adding AutoMapper dependency in scaffold)
    public static class DispatchMapping
    {
        public static DispatchDto ToDto(Dispatch src)
        {
            return new DispatchDto
            {
                Id = src.Id,
                DispatchNumber = src.DispatchNumber,
                JobId = src.JobId,
                ServiceOrderId = src.ServiceOrderId,
                Status = src.Status,
                Priority = src.Priority,
                CompletionPercentage = src.CompletionPercentage,
                DispatchedAt = src.DispatchedAt,
                DispatchedBy = src.DispatchedBy,
                CreatedAt = src.CreatedAt,
                UpdatedAt = src.UpdatedAt
            };
        }
    }
}
