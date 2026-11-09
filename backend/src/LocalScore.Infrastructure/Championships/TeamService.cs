using System.Text.RegularExpressions;
using LocalScore.Application.Common;
using LocalScore.Application.Teams;
using LocalScore.Domain.Championships;
using LocalScore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalScore.Infrastructure.Championships;

internal sealed partial class TeamService(
    LocalScoreDbContext dbContext,
    TimeProvider timeProvider) : ITeamService
{
    public async Task<IReadOnlyList<TeamModel>> ListAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken)
    {
        var teams = await dbContext.Teams
            .AsNoTracking()
            .Where(team => team.OwnerUserId == ownerUserId)
            .ToListAsync(cancellationToken);

        return teams
            .OrderBy(team => TextNormalizer.SortKey(team.Name), StringComparer.Ordinal)
            .ThenBy(team => team.Id)
            .Select(Map)
            .ToArray();
    }

    public async Task<Result<TeamModel>> GetAsync(
        Guid ownerUserId,
        Guid teamId,
        CancellationToken cancellationToken)
    {
        var team = await FindOwnedAsync(ownerUserId, teamId, false, cancellationToken);
        return team is null
            ? Result<TeamModel>.Failure(TeamErrors.NotFound)
            : Result<TeamModel>.Success(Map(team));
    }

    public async Task<Result<TeamModel>> CreateAsync(
        Guid ownerUserId,
        SaveTeamCommand command,
        CancellationToken cancellationToken)
    {
        var values = Normalize(command);
        var validation = Validate(values);
        if (validation is not null)
        {
            return Result<TeamModel>.Failure(validation);
        }

        var normalizedName = TextNormalizer.NormalizeTeamName(values.Name);
        if (await dbContext.Teams.AnyAsync(
                team => team.OwnerUserId == ownerUserId && team.NormalizedName == normalizedName,
                cancellationToken))
        {
            return Result<TeamModel>.Failure(TeamErrors.NameConflict);
        }

        var team = Team.Create(
            ownerUserId,
            values.Name,
            normalizedName,
            values.ShortName,
            values.Acronym,
            values.City,
            values.PrimaryColor,
            values.SecondaryColor,
            timeProvider.GetUtcNow());

        dbContext.Teams.Add(team);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result<TeamModel>.Failure(TeamErrors.NameConflict);
        }

        return Result<TeamModel>.Success(Map(team));
    }

    public async Task<Result<TeamModel>> UpdateAsync(
        Guid ownerUserId,
        Guid teamId,
        SaveTeamCommand command,
        CancellationToken cancellationToken)
    {
        var values = Normalize(command);
        var validation = Validate(values);
        if (validation is not null)
        {
            return Result<TeamModel>.Failure(validation);
        }

        var team = await FindOwnedAsync(ownerUserId, teamId, true, cancellationToken);
        if (team is null)
        {
            return Result<TeamModel>.Failure(TeamErrors.NotFound);
        }

        var normalizedName = TextNormalizer.NormalizeTeamName(values.Name);
        if (await dbContext.Teams.AnyAsync(
                other => other.Id != teamId &&
                         other.OwnerUserId == ownerUserId &&
                         other.NormalizedName == normalizedName,
                cancellationToken))
        {
            return Result<TeamModel>.Failure(TeamErrors.NameConflict);
        }

        team.Update(
            values.Name,
            normalizedName,
            values.ShortName,
            values.Acronym,
            values.City,
            values.PrimaryColor,
            values.SecondaryColor,
            timeProvider.GetUtcNow());

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result<TeamModel>.Failure(TeamErrors.NameConflict);
        }

        return Result<TeamModel>.Success(Map(team));
    }

    public async Task<Result> DeleteAsync(
        Guid ownerUserId,
        Guid teamId,
        CancellationToken cancellationToken)
    {
        var team = await FindOwnedAsync(ownerUserId, teamId, true, cancellationToken);
        if (team is null)
        {
            return Result.Failure(TeamErrors.NotFound);
        }

        if (await dbContext.ChampionshipTeams.AnyAsync(
                participation => participation.TeamId == teamId,
                cancellationToken))
        {
            return Result.Failure(TeamErrors.DeleteNotAllowed);
        }

        dbContext.Teams.Remove(team);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<ChampionshipTeamModel>>> ListParticipantsAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken)
    {
        if (!await ChampionshipExistsAsync(ownerUserId, championshipId, cancellationToken))
        {
            return Result<IReadOnlyList<ChampionshipTeamModel>>.Failure(TeamErrors.ChampionshipNotFound);
        }

        var participations = await dbContext.ChampionshipTeams
            .AsNoTracking()
            .Include(participation => participation.Team)
            .Where(participation => participation.ChampionshipId == championshipId)
            .ToListAsync(cancellationToken);

        var ordered = participations
            .OrderBy(participation => TextNormalizer.SortKey(participation.Team.Name), StringComparer.Ordinal)
            .ThenBy(participation => participation.TeamId)
            .Select(Map)
            .ToArray();

        return Result<IReadOnlyList<ChampionshipTeamModel>>.Success(ordered);
    }

    public async Task<Result<ParticipationResult>> ActivateParticipationAsync(
        Guid ownerUserId,
        Guid championshipId,
        Guid teamId,
        CancellationToken cancellationToken)
    {
        var championship = await dbContext.Championships.SingleOrDefaultAsync(
            item => item.Id == championshipId && item.OwnerUserId == ownerUserId,
            cancellationToken);
        if (championship is null)
        {
            return Result<ParticipationResult>.Failure(TeamErrors.ChampionshipNotFound);
        }

        if (championship.StatusCode == ChampionshipStatusCode.Finished)
        {
            return Result<ParticipationResult>.Failure(TeamErrors.ParticipationLocked);
        }

        var team = await FindOwnedAsync(ownerUserId, teamId, false, cancellationToken);
        if (team is null)
        {
            return Result<ParticipationResult>.Failure(TeamErrors.NotFound);
        }

        var participation = await dbContext.ChampionshipTeams
            .Include(item => item.Team)
            .SingleOrDefaultAsync(
                item => item.ChampionshipId == championshipId && item.TeamId == teamId,
                cancellationToken);
        var wasCreated = participation is null;

        if (participation is null)
        {
            participation = ChampionshipTeam.Create(championshipId, teamId, timeProvider.GetUtcNow());
            dbContext.ChampionshipTeams.Add(participation);
        }
        else if (!participation.IsActive)
        {
            participation.Activate(timeProvider.GetUtcNow());
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        var mapped = wasCreated
            ? new ChampionshipTeamModel(
                participation.Id,
                Map(team),
                participation.RegisteredAt,
                participation.RemovedAt)
            : Map(participation);

        return Result<ParticipationResult>.Success(new ParticipationResult(mapped, wasCreated));
    }

    public async Task<Result> RemoveParticipationAsync(
        Guid ownerUserId,
        Guid championshipId,
        Guid teamId,
        CancellationToken cancellationToken)
    {
        var championship = await dbContext.Championships.SingleOrDefaultAsync(
            item => item.Id == championshipId && item.OwnerUserId == ownerUserId,
            cancellationToken);
        if (championship is null)
        {
            return Result.Failure(TeamErrors.ChampionshipNotFound);
        }

        if (championship.StatusCode == ChampionshipStatusCode.Finished)
        {
            return Result.Failure(TeamErrors.ParticipationLocked);
        }

        if (!await dbContext.Teams.AnyAsync(
                team => team.Id == teamId && team.OwnerUserId == ownerUserId,
                cancellationToken))
        {
            return Result.Failure(TeamErrors.NotFound);
        }

        var participation = await dbContext.ChampionshipTeams.SingleOrDefaultAsync(
            item => item.ChampionshipId == championshipId && item.TeamId == teamId,
            cancellationToken);

        if (participation is not null)
        {
            participation.Remove(timeProvider.GetUtcNow());
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    private Task<Team?> FindOwnedAsync(
        Guid ownerUserId,
        Guid teamId,
        bool tracked,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Teams.Where(team => team.Id == teamId && team.OwnerUserId == ownerUserId);
        return (tracked ? query : query.AsNoTracking()).SingleOrDefaultAsync(cancellationToken);
    }

    private Task<bool> ChampionshipExistsAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken) =>
        dbContext.Championships.AnyAsync(
            championship => championship.Id == championshipId && championship.OwnerUserId == ownerUserId,
            cancellationToken);

    private static SaveTeamCommand Normalize(SaveTeamCommand command) =>
        new(
            command.Name?.Trim() ?? string.Empty,
            TextNormalizer.Optional(command.ShortName),
            TextNormalizer.UpperOptional(command.Acronym),
            TextNormalizer.Optional(command.City),
            TextNormalizer.UpperOptional(command.PrimaryColor),
            TextNormalizer.UpperOptional(command.SecondaryColor));

    private static Error? Validate(SaveTeamCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        if (command.Name.Length is < 2 or > 100)
        {
            errors["name"] = ["Name must contain between 2 and 100 characters."];
        }

        ValidateOptionalLength(errors, "shortName", command.ShortName, 2, 30);
        ValidateOptionalLength(errors, "city", command.City, 2, 100);

        if (command.Acronym is not null && !AcronymPattern().IsMatch(command.Acronym))
        {
            errors["acronym"] = ["Acronym must contain between 2 and 10 uppercase letters or numbers."];
        }

        if (command.PrimaryColor is not null && !ColorPattern().IsMatch(command.PrimaryColor))
        {
            errors["primaryColor"] = ["Primary color must use the #RRGGBB format."];
        }

        if (command.SecondaryColor is not null)
        {
            if (!ColorPattern().IsMatch(command.SecondaryColor))
            {
                errors["secondaryColor"] = ["Secondary color must use the #RRGGBB format."];
            }
            else if (command.PrimaryColor is null)
            {
                errors["secondaryColor"] = ["Secondary color requires a primary color."];
            }
        }

        return errors.Count == 0
            ? null
            : Error.Validation("team.validation", "The team data is invalid.", errors);
    }

    private static void ValidateOptionalLength(
        IDictionary<string, string[]> errors,
        string field,
        string? value,
        int minimum,
        int maximum)
    {
        if (value is not null && (value.Length < minimum || value.Length > maximum))
        {
            errors[field] = [$"The field must contain between {minimum} and {maximum} characters."];
        }
    }

    private static TeamModel Map(Team team) =>
        new(
            team.Id,
            team.Name,
            team.ShortName,
            team.Acronym,
            team.City,
            team.PrimaryColor,
            team.SecondaryColor,
            team.CreatedAt,
            team.UpdatedAt);

    private static ChampionshipTeamModel Map(ChampionshipTeam participation) =>
        new(
            participation.Id,
            Map(participation.Team),
            participation.RegisteredAt,
            participation.RemovedAt);

    [GeneratedRegex("^[A-Z0-9]{2,10}$")]
    private static partial Regex AcronymPattern();

    [GeneratedRegex("^#[0-9A-F]{6}$")]
    private static partial Regex ColorPattern();
}
