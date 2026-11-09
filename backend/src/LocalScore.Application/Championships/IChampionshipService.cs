using LocalScore.Application.Common;

namespace LocalScore.Application.Championships;

public interface IChampionshipService
{
    Task<IReadOnlyList<ChampionshipModel>> ListAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken);

    Task<Result<ChampionshipModel>> GetAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken);

    Task<Result<ChampionshipModel>> CreateAsync(
        Guid ownerUserId,
        CreateChampionshipCommand command,
        CancellationToken cancellationToken);

    Task<Result<ChampionshipModel>> UpdateAsync(
        Guid ownerUserId,
        Guid championshipId,
        UpdateChampionshipCommand command,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken);

    Task<Result<ChampionshipModel>> PublishAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken);

    Task<Result<ChampionshipModel>> ReturnToDraftAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken);

    Task<Result<ChampionshipModel>> FinishAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken);

    Task<Result<ChampionshipModel>> ReopenAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken);

    Task<Result<PublicChampionshipModel>> GetPublicAsync(
        string slug,
        CancellationToken cancellationToken);
}
