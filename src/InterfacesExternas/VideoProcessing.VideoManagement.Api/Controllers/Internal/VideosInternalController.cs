using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoProcessing.VideoManagement.Application.Models;
using VideoProcessing.VideoManagement.Application.Models.ResponseModels;
using VideoProcessing.VideoManagement.Application.UseCases.GetVideoById;


namespace VideoProcessing.VideoManagement.Api.Controllers.Internal
{
    [ApiController]
    [Route("internal/videos")]
    [Authorize(Policy = "ScopeAnalyzeRun")]
    [Produces("application/json")]
    public class VideosInternalController (IGetVideoByIdUseCase getVideoByIdUseCase) : Controller
    {

        /// <summary>
        /// Obtém um vídeo pelo identificador. Retorna 404 se não encontrado ou não pertence ao usuário.
        /// </summary>
        [HttpGet("{userId:guid}/{id:guid}")]
        [ProducesResponseType(typeof(VideoResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVideo(Guid userId,Guid id, CancellationToken cancellationToken)
        {         

            var response = await getVideoByIdUseCase.ExecuteAsync(userId.ToString(), id.ToString(), cancellationToken);
            if (response is null)
                return NotFound();

            return Ok(response);
        }
    }
}
