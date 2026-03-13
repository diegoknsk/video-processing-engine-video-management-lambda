namespace VideoProcessing.VideoManagement.Application.Models.ResponseModels;

/// <summary>
/// Item resumido de um chunk para a lista no GET /videos/{id}.
/// </summary>
public record ChunkItemResponseModel(
    string ChunkId,
    double StartSec,
    double EndSec,
    string Status);
