using LocalScore.Application.Common;

namespace LocalScore.Application.Teams;

public interface ITeamService
{
    Task<IReadOnlyList<TeamModel>> ListAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken);

    Task<Result<TeamModel>> GetAsync(
        Guid ownerUserId,
        Guid teamId,
        CancellationToken cancellationToken);

    Task<Result<TeamModel>> CreateAsync(
        Guid ownerUserId,
        SaveTeamCommand command,
        CancellationToken cancellationToken);

    Task<Result<TeamModel>> UpdateAsync(
        Guid ownerUserId,
        Guid teamId,
        SaveTeamCommand command,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(
        Guid ownerUserId,
        Guid teamId,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<ChampionshipTeamModel>>> ListParticipantsAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken);

    Task<Result<ParticipationResult>> ActivateParticipationAsync(
        Guid ownerUserId,
        Guid championshipId,
        Guid teamId,
        CancellationToken cancellationToken);

    Task<Result> RemoveParticipationAsync(
        Guid ownerUserId,
        Guid championshipId,
        Guid teamId,
        CancellationToken cancellationToken);
}
