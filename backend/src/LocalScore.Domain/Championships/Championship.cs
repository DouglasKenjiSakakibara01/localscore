namespace LocalScore.Domain.Championships;

public sealed class Championship
{
    private Championship()
    {
    }

    private Championship(
        Guid id,
        Guid ownerUserId,
        string name,
        string slug,
        string? description,
        DateOnly? startDate,
        DateOnly? endDate,
        DateTimeOffset now)
    {
        Id = id;
        OwnerUserId = ownerUserId;
        StatusCode = ChampionshipStatusCode.Draft;
        Name = name;
        Slug = slug;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public ChampionshipStatusCode StatusCode { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateOnly? StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public ICollection<ChampionshipTeam> Teams { get; } = [];

    public static Championship Create(
        Guid ownerUserId,
        string name,
        string slug,
        string? description,
        DateOnly? startDate,
        DateOnly? endDate,
        DateTimeOffset now) =>
        new(Guid.NewGuid(), ownerUserId, name, slug, description, startDate, endDate, now);

    public bool CanChangeSlug =>
        StatusCode == ChampionshipStatusCode.Draft && PublishedAt is null;

    public bool Update(
        string name,
        string slug,
        string? description,
        DateOnly? startDate,
        DateOnly? endDate,
        DateTimeOffset now)
    {
        if (StatusCode == ChampionshipStatusCode.Finished ||
            !CanChangeSlug && !string.Equals(Slug, slug, StringComparison.Ordinal))
        {
            return false;
        }

        Name = name;
        Slug = slug;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = now;
        return true;
    }

    public bool Publish(DateTimeOffset now)
    {
        if (StatusCode != ChampionshipStatusCode.Draft)
        {
            return false;
        }

        StatusCode = ChampionshipStatusCode.Published;
        PublishedAt ??= now;
        UpdatedAt = now;
        return true;
    }

    public bool ReturnToDraft(bool hasSchedule, DateTimeOffset now)
    {
        if (StatusCode != ChampionshipStatusCode.Published || hasSchedule)
        {
            return false;
        }

        StatusCode = ChampionshipStatusCode.Draft;
        UpdatedAt = now;
        return true;
    }

    public bool Finish(DateTimeOffset now)
    {
        if (StatusCode != ChampionshipStatusCode.Published)
        {
            return false;
        }

        StatusCode = ChampionshipStatusCode.Finished;
        UpdatedAt = now;
        return true;
    }

    public bool Reopen(DateTimeOffset now)
    {
        if (StatusCode != ChampionshipStatusCode.Finished)
        {
            return false;
        }

        StatusCode = ChampionshipStatusCode.Published;
        UpdatedAt = now;
        return true;
    }
}
