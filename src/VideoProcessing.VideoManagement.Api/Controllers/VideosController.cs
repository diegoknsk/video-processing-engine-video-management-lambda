using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoProcessing.VideoManagement.Application.Models;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.UseCases.UploadVideo;

namespace VideoProcessing.VideoManagement.Api.Controllers;

/// <summary>
/// Endpoints de gerenciamento de vídeos: registro, upload presigned, listagem e atualização.
/// </summary>
[ApiController]
[Route("videos")]
[Produces("application/json")]
[Authorize]
public class VideosController(IUploadVideoUseCase uploadVideoUseCase) : ControllerBase
{
    /// <summary>
    /// Registra um novo vídeo e retorna URL presigned para upload no S3.
    /// Requer autenticação JWT (Cognito) — claim "sub" usado como userId.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UploadVideoResponseModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadVideo(
        [FromBody] UploadVideoInputModel input,
        CancellationToken cancellationToken)
    {
        var sub = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Unauthorized();

        // Cognito sub é UUID-format — conversão para Guid segura neste contexto (Domain exige Guid)
        if (!Guid.TryParse(sub, out var userId))
            return Unauthorized();

        try
        {
            var response = await uploadVideoUseCase.ExecuteAsync(input, userId, cancellationToken);
            return CreatedAtAction(nameof(GetVideo), new { id = response.VideoId }, response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ErrorResponse(
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                "Validation Failed",
                StatusCodes.Status400BadRequest,
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                HttpContext.TraceIdentifier
            ));
        }
    }

    /// <summary>
    /// Lista vídeos do usuário autenticado com paginação.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(VideoListResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult ListVideos([FromQuery] int? pageSize, [FromQuery] string? nextToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "Not implemented yet (Story 05).");
    }

    /// <summary>
    /// Obtém um vídeo pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VideoResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateVideo(Guid id, [FromBody] UpdateVideoInputModel input)
    {
        return StatusCode(StatusCodes.Status501NotImplemented, "Not implemented yet (Story 06).");
    }
}
