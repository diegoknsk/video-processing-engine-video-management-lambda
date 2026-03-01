using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoProcessing.VideoManagement.Application.Models;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.UseCases.GetVideoById;
using VideoProcessing.VideoManagement.Application.UseCases.ListVideos;
using VideoProcessing.VideoManagement.Application.UseCases.UpdateVideo;
using VideoProcessing.VideoManagement.Application.UseCases.UploadVideo;
using VideoProcessing.VideoManagement.Api.Models;
using VideoProcessing.VideoManagement.Domain.Exceptions;

namespace VideoProcessing.VideoManagement.Api.Controllers;

/// <summary>
/// Endpoints de gerenciamento de vídeos: registro, upload presigned, listagem e atualização.
/// </summary>
[ApiController]
[Route("videos")]
[Produces("application/json")]
[Authorize]
public class VideosController(
    IUploadVideoUseCase uploadVideoUseCase,
    IListVideosUseCase listVideosUseCase,
    IGetVideoByIdUseCase getVideoByIdUseCase,
    IUpdateVideoUseCase updateVideoUseCase,
    IValidator<UpdateVideoInputModel> updateVideoValidator) : ControllerBase
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
    /// Lista vídeos do usuário autenticado com paginação cursor-based.
    /// Query params: limit (padrão 50, máx 100), nextToken (opcional).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(VideoListResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListVideos(
        [FromQuery] int? limit,
        [FromQuery] string? nextToken,
        CancellationToken cancellationToken)
    {
        var sub = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
            return Unauthorized();

        var response = await listVideosUseCase.ExecuteAsync(userId.ToString(), limit ?? 50, nextToken, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Obtém um vídeo pelo identificador. Retorna 404 se não encontrado ou não pertence ao usuário.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VideoResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetVideo(Guid id, CancellationToken cancellationToken)
    {
        var sub = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
            return Unauthorized();

        var response = await getVideoByIdUseCase.ExecuteAsync(userId.ToString(), id.ToString(), cancellationToken);
        if (response is null)
            return NotFound();

        return Ok(response);
    }

    /// <summary>
    /// Atualiza parcialmente um vídeo (status, progresso, erros, S3). Idempotente.
    /// Internal route for orchestrator/processor/finalizer — acessível sem JWT via [AllowAnonymous].
    /// </summary>
    [AllowAnonymous]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(VideoResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateVideo(Guid id, [FromBody] UpdateVideoInputModel input, CancellationToken cancellationToken)
    {
        var validation = await updateVideoValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(ApiErrorResponse.Create("ValidationFailed", string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))));

        try
        {
            var response = await updateVideoUseCase.ExecuteAsync(id, input, cancellationToken);
            if (response is null)
                return NotFound(ApiErrorResponse.Create("NotFound", "Vídeo não encontrado."));

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ApiErrorResponse.Create("ValidationFailed", ex.Errors.Any() ? string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)) : ex.Message));
        }
        catch (VideoUpdateConflictException ex)
        {
            return Conflict(ApiErrorResponse.Create("UpdateConflict", ex.Message));
        }
    }
}
