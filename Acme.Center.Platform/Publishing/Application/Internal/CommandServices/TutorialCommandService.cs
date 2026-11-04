using Acme.Center.Platform.Publishing.Application.CommandServices;
using Acme.Center.Platform.Publishing.Domain.Model;
using Acme.Center.Platform.Publishing.Domain.Model.Aggregates;
using Acme.Center.Platform.Publishing.Domain.Model.Commands;
using Acme.Center.Platform.Publishing.Domain.Model.Errors;
using Acme.Center.Platform.Publishing.Domain.Repositories;
using Acme.Center.Platform.Shared.Application.Model;
using Acme.Center.Platform.Shared.Domain.Repositories;

namespace Acme.Center.Platform.Publishing.Application.Internal.CommandServices;

public class TutorialCommandService(
    ITutorialRepository tutorialRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ITutorialCommandService
{
    /// <inheritdoc />
    public async Task<Result<Tutorial>> Handle(AddVideoAssetToTutorialCommand command,
        CancellationToken cancellationToken)
    {
        var tutorial = await tutorialRepository.FindByIdAsync(command.TutorialId, cancellationToken);
        if (tutorial is null)
            return Result<Tutorial>.Failure(PublishingErrors.TutorialNotFound, PublishingError.TutorialNotFound);

        tutorial.AddVideo(command.VideoUrl);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Tutorial>.Success(tutorial);
    }

    /// <inheritdoc />
    public async Task<Result<Tutorial>> Handle(CreateTutorialCommand command, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.FindByIdAsync(command.CategoryId, cancellationToken);
        if (category is null)
            return Result<Tutorial>.Failure(PublishingErrors.CategoryNotFound, PublishingError.CategoryNotFound);

        if (await tutorialRepository.ExistsByTitleAsync(command.Title, cancellationToken))
            return Result<Tutorial>.Failure(PublishingErrors.DuplicateTutorialTitleWithTitle(command.Title),
                PublishingError.DuplicateTutorialTitle);

        var tutorial = new Tutorial(command);
        await tutorialRepository.AddAsync(tutorial, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        tutorial.Category = category;
        return Result<Tutorial>.Success(tutorial);
    }
}
