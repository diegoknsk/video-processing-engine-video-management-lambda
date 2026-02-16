using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoProcessing.VideoManagement.Application.Models;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;

namespace VideoProcessing.VideoManagement.Api.Controllers;

/// <summary>
/// Endpoints de gerenciamento de vídeos: registro, upload presigned, listagem e atualização.
/// </summary>
[ApiController]
[Route("videos")]
[Produces("application/json")]
public class VideosController : ControllerBase
{
    /// <summary>
    /// Registra um novo vídeo e retorna URL presigned para upload no S3.
    /// Requer autenticação JWT (Cognito).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UploadVideoResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public IActionResult UploadVideo([FromBody] UploadVideoInputModel input)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "Not implemented yet (Story 04).");
    }

    /// <summary>
    /// Lista vídeos do usuário autenticado com paginação.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(VideoListResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public IActionResult ListVideos([FromQuery] int? pageSize, [FromQuery] string? nextToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "Not implemented yet (Story 05).");
    }

    /// <summary>
    /// Obtém um vídeo pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VideoResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public IActionResult GetVideo(Guid id)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "Not implemented yet (Story 05).");
    }

    /// <summary>
    /// Atualiza parcialmente um vídeo (status, progresso, erros, S3). Idempotente por clientRequestId.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(VideoResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [Authorize]
    public IActionResult UpdateVideo(Guid id, [FromBody] UpdateVideoInputModel input)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "Not implemented yet (Story 06).");
    }
}
