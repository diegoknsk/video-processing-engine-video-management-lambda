namespace VideoProcessing.VideoManagement.Application.Models;

public record ErrorResponse(
    string Type,
    string Title,
    int Status,
    string? Detail,
    string? TraceId
);
