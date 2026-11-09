using LocalScore.Application.Championships;
using LocalScore.Application.Common;
using LocalScore.Domain.Championships;
using LocalScore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalScore.Infrastructure.Championships;

internal sealed class ChampionshipService(
    LocalScoreDbContext dbContext,
    TimeProvider timeProvider) : IChampionshipService
{
    public async Task<IReadOnlyList<ChampionshipModel>> ListAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken)
    {
        var championships = await dbContext.Championships
            .AsNoTracking()
            .Where(championship => championship.OwnerUserId == ownerUserId)
            .OrderByDescending(championship => championship.UpdatedAt)
            .ThenBy(championship => championship.Name)
            .ThenBy(championship => championship.Id)
            .ToListAsync(cancellationToken);

        return championships.Select(Map).ToArray();
    }

    public async Task<Result<ChampionshipModel>> GetAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken)
    {
        var championship = await FindOwnedAsync(ownerUserId, championshipId, false, cancellationToken);
        return championship is null
            ? Result<ChampionshipModel>.Failure(ChampionshipErrors.NotFound)
            : Result<ChampionshipModel>.Success(Map(championship));
    }

    public async Task<Result<ChampionshipModel>> CreateAsync(
        Guid ownerUserId,
        CreateChampionshipCommand command,
        CancellationToken cancellationToken)
    {
        var validation = Validate(command.Name, command.Description, command.StartDate, command.EndDate);
        if (validation is not null)
        {
            return Result<ChampionshipModel>.Failure(validation);
        }

        var name = command.Name.Trim();
        var description = TextNormalizer.Optional(command.Description);
        var requestedSlug = TextNormalizer.Optional(command.Slug);
        var slugBase = TextNormalizer.Slugify(requestedSlug ?? name);

        var slugValidation = ValidateSlug(slugBase);
        if (slugValidation is not null)
        {
            return Result<ChampionshipModel>.Failure(slugValidation);
        }

        string slug;
        if (requestedSlug is null)
        {
            slug = await FindAvailableSlugAsync(slugBase, null, cancellationToken);
        }
        else
        {
            if (await dbContext.Championships.AnyAsync(
                    championship => championship.Slug == slugBase,
                    cancellationToken))
            {
                return Result<ChampionshipModel>.Failure(ChampionshipErrors.SlugConflict);
            }

            slug = slugBase;
        }

        var championship = Championship.Create(
            ownerUserId,
            name,
            slug,
            description,
            command.StartDate,
            command.EndDate,
            timeProvider.GetUtcNow());

        dbContext.Championships.Add(championship);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result<ChampionshipModel>.Failure(ChampionshipErrors.SlugConflict);
        }

        return Result<ChampionshipModel>.Success(Map(championship));
    }

    public async Task<Result<ChampionshipModel>> UpdateAsync(
        Guid ownerUserId,
        Guid championshipId,
        UpdateChampionshipCommand command,
        CancellationToken cancellationToken)
    {
        var validation = Validate(command.Name, command.Description, command.StartDate, command.EndDate);
        if (validation is not null)
        {
            return Result<ChampionshipModel>.Failure(validation);
        }

        var slug = TextNormalizer.Slugify(command.Slug);
        var slugValidation = ValidateSlug(slug);
        if (slugValidation is not null)
        {
            return Result<ChampionshipModel>.Failure(slugValidation);
        }

        var championship = await FindOwnedAsync(ownerUserId, championshipId, true, cancellationToken);
        if (championship is null)
        {
            return Result<ChampionshipModel>.Failure(ChampionshipErrors.NotFound);
        }

        if (championship.StatusCode == ChampionshipStatusCode.Finished)
        {
            return Result<ChampionshipModel>.Failure(ChampionshipErrors.ReadOnly);
        }

        if (!string.Equals(championship.Slug, slug, StringComparison.Ordinal))
        {
            if (!championship.CanChangeSlug)
            {
                return Result<ChampionshipModel>.Failure(ChampionshipErrors.SlugLocked);
            }

            if (await dbContext.Championships.AnyAsync(
                    other => other.Id != championshipId && other.Slug == slug,
                    cancellationToken))
            {
                return Result<ChampionshipModel>.Failure(ChampionshipErrors.SlugConflict);
            }
        }

        championship.Update(
            command.Name.Trim(),
            slug,
            TextNormalizer.Optional(command.Description),
            command.StartDate,
            command.EndDate,
            timeProvider.GetUtcNow());

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result<ChampionshipModel>.Failure(ChampionshipErrors.SlugConflict);
        }

        return Result<ChampionshipModel>.Success(Map(championship));
    }

    public async Task<Result> DeleteAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken)
    {
        var championship = await FindOwnedAsync(ownerUserId, championshipId, true, cancellationToken);
        if (championship is null)
        {
            return Result.Failure(ChampionshipErrors.NotFound);
        }

        var hasParticipationHistory = await dbContext.ChampionshipTeams.AnyAsync(
            participation => participation.ChampionshipId == championshipId,
            cancellationToken);

        if (championship.StatusCode != ChampionshipStatusCode.Draft || hasParticipationHistory)
        {
            return Result.Failure(ChampionshipErrors.DeleteNotAllowed);
        }

        dbContext.Championships.Remove(championship);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public Task<Result<ChampionshipModel>> PublishAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken) =>
        ChangeStatusAsync(ownerUserId, championshipId, static (championship, now) => championship.Publish(now), cancellationToken);

    public Task<Result<ChampionshipModel>> ReturnToDraftAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken) =>
        ChangeStatusAsync(
            ownerUserId,
            championshipId,
            static (championship, now) => championship.ReturnToDraft(hasSchedule: false, now),
            cancellationToken);

    public Task<Result<ChampionshipModel>> FinishAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken) =>
        ChangeStatusAsync(ownerUserId, championshipId, static (championship, now) => championship.Finish(now), cancellationToken);

    public Task<Result<ChampionshipModel>> ReopenAsync(
        Guid ownerUserId,
        Guid championshipId,
        CancellationToken cancellationToken) =>
        ChangeStatusAsync(ownerUserId, championshipId, static (championship, now) => championship.Reopen(now), cancellationToken);

    public async Task<Result<PublicChampionshipModel>> GetPublicAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        var normalizedSlug = TextNormalizer.Slugify(slug);
        var championship = await dbContext.Championships
            .AsNoTracking()
            .Include(item => item.Teams)
            .ThenInclude(participation => participation.Team)
            .SingleOrDefaultAsync(
                item => item.Slug == normalizedSlug &&
                        (item.StatusCode == ChampionshipStatusCode.Published ||
                         item.StatusCode == ChampionshipStatusCode.Finished),
                cancellationToken);

        if (championship is null)
        {
            return Result<PublicChampionshipModel>.Failure(ChampionshipErrors.NotFound);
        }

        var teams = championship.Teams
            .Where(participation => participation.RemovedAt is null)
            .Select(participation => participation.Team)
            .OrderBy(team => TextNormalizer.SortKey(team.Name), StringComparer.Ordinal)
            .ThenBy(team => team.Id)
            .Select(team => new PublicTeamModel(
                team.Id,
                team.Name,
                team.ShortName,
                team.Acronym,
                team.City,
                team.PrimaryColor,
                team.SecondaryColor))
            .ToArray();

        return Result<PublicChampionshipModel>.Success(new PublicChampionshipModel(
            championship.Name,
            championship.Slug,
            championship.Description,
            championship.StartDate,
            championship.EndDate,
            MapStatus(championship.StatusCode),
            teams));
    }

    private async Task<Result<ChampionshipModel>> ChangeStatusAsync(
        Guid ownerUserId,
        Guid championshipId,
        Func<Championship, DateTimeOffset, bool> transition,
        CancellationToken cancellationToken)
    {
        var championship = await FindOwnedAsync(ownerUserId, championshipId, true, cancellationToken);
        if (championship is null)
        {
            return Result<ChampionshipModel>.Failure(ChampionshipErrors.NotFound);
        }

        if (!transition(championship, timeProvider.GetUtcNow()))
        {
            return Result<ChampionshipModel>.Failure(ChampionshipErrors.InvalidTransition);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<ChampionshipModel>.Success(Map(championship));
    }

    private Task<Championship?> FindOwnedAsync(
        Guid ownerUserId,
        Guid championshipId,
        bool tracked,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Championships
            .Where(championship => championship.Id == championshipId &&
                                   championship.OwnerUserId == ownerUserId);

        return (tracked ? query : query.AsNoTracking()).SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<string> FindAvailableSlugAsync(
        string slugBase,
        Guid? excludedChampionshipId,
        CancellationToken cancellationToken)
    {
        var slug = slugBase;
        var suffix = 2;

        while (await dbContext.Championships.AnyAsync(
                   championship => championship.Id != excludedChampionshipId && championship.Slug == slug,
                   cancellationToken))
        {
            var suffixText = $"-{suffix++}";
            var prefixLength = Math.Min(slugBase.Length, 100 - suffixText.Length);
            slug = $"{slugBase[..prefixLength].TrimEnd('-')}{suffixText}";
        }

        return slug;
    }

    private static Error? Validate(
        string name,
        string? description,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        var errors = new Dictionary<string, string[]>();
        var trimmedName = name?.Trim() ?? string.Empty;

        if (trimmedName.Length is < 3 or > 120)
        {
            errors["name"] = ["Name must contain between 3 and 120 characters."];
        }

        if (TextNormalizer.Optional(description)?.Length > 1000)
        {
            errors["description"] = ["Description must contain at most 1000 characters."];
        }

        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
        {
            errors["endDate"] = ["End date cannot be earlier than start date."];
        }

        return errors.Count == 0
            ? null
            : Error.Validation("championship.validation", "The championship data is invalid.", errors);
    }

    private static Error? ValidateSlug(string slug)
    {
        if (slug.Length is >= 3 and <= 100)
        {
            return null;
        }

        return Error.Validation(
            "championship.validation",
            "The championship data is invalid.",
            new Dictionary<string, string[]>
            {
                ["slug"] = ["Slug must contain between 3 and 100 characters after normalization."]
            });
    }

    private static ChampionshipModel Map(Championship championship) =>
        new(
            championship.Id,
            championship.Name,
            championship.Slug,
            championship.Description,
            championship.StartDate,
            championship.EndDate,
            MapStatus(championship.StatusCode),
            championship.CanChangeSlug,
            championship.CreatedAt,
            championship.UpdatedAt);

    private static ChampionshipStatusModel MapStatus(ChampionshipStatusCode status) =>
        new((int)status, status switch
        {
            ChampionshipStatusCode.Draft => "Rascunho",
            ChampionshipStatusCode.Published => "Publicado",
            ChampionshipStatusCode.Finished => "Finalizado",
            _ => status.ToString()
        });
}
