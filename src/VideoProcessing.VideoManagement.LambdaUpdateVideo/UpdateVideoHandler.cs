using FluentValidation;
using VideoProcessing.VideoManagement.Application.Models.InputModels;
using VideoProcessing.VideoManagement.Application.UseCases.UpdateVideo;
using VideoProcessing.VideoManagement.Domain.Exceptions;
using VideoProcessing.VideoManagement.LambdaUpdateVideo.Models;

namespace VideoProcessing.VideoManagement.LambdaUpdateVideo;

/// <summary>
/// Borda da Lambda: valida o evento (UpdateVideoInputModel), delega ao Use Case e mapeia resultado para resposta.
/// Nenhuma lógica de negócio; apenas interface externa.
/// </summary>
public class UpdateVideoHandler(
    IUpdateVideoUseCase updateVideoUseCase,
    IValidator<UpdateVideoInputModel> validator) : IUpdateVideoHandler
{
    public async Task<UpdateVideoLambdaResponse> HandleAsync(UpdateVideoLambdaEvent evt, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(evt, cancellationToken);
        if (!validation.IsValid)
            return UpdateVideoLambdaResponse.ValidationError(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            var response = await updateVideoUseCase.ExecuteAsync(evt.VideoId, evt, cancellationToken);
            if (response is null)
                return UpdateVideoLambdaResponse.NotFound();

            return UpdateVideoLambdaResponse.Ok(response);
        }
        catch (VideoUpdateConflictException ex)
        {
            return UpdateVideoLambdaResponse.Conflict(ex.Message);
        }
    }
}
