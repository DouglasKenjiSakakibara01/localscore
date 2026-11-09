namespace LocalScore.Domain.Championships;

public sealed class Team
{
    private Team()
    {
    }

    private Team(
        Guid id,
        Guid ownerUserId,
        string name,
        string normalizedName,
        string? shortName,
        string? acronym,
        string? city,
        string? primaryColor,
        string? secondaryColor,
        DateTimeOffset now)
    {
        Id = id;
        OwnerUserId = ownerUserId;
        UpdateValues(name, normalizedName, shortName, acronym, city, primaryColor, secondaryColor, now);
        CreatedAt = now;
    }

    public Guid Id { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string? ShortName { get; private set; }

    public string? Acronym { get; private set; }

    public string? City { get; private set; }

    public string? PrimaryColor { get; private set; }

    public string? SecondaryColor { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public ICollection<ChampionshipTeam> Championships { get; } = [];

    public static Team Create(
        Guid ownerUserId,
        string name,
        string normalizedName,
        string? shortName,
        string? acronym,
        string? city,
        string? primaryColor,
        string? secondaryColor,
        DateTimeOffset now) =>
        new(
            Guid.NewGuid(),
            ownerUserId,
            name,
            normalizedName,
            shortName,
            acronym,
            city,
            primaryColor,
            secondaryColor,
            now);

    public void Update(
        string name,
        string normalizedName,
        string? shortName,
        string? acronym,
        string? city,
        string? primaryColor,
        string? secondaryColor,
        DateTimeOffset now) =>
        UpdateValues(name, normalizedName, shortName, acronym, city, primaryColor, secondaryColor, now);

    private void UpdateValues(
        string name,
        string normalizedName,
        string? shortName,
        string? acronym,
        string? city,
        string? primaryColor,
        string? secondaryColor,
        DateTimeOffset now)
    {
        Name = name;
        NormalizedName = normalizedName;
        ShortName = shortName;
        Acronym = acronym;
        City = city;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        UpdatedAt = now;
    }
}
