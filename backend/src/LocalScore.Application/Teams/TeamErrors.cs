using LocalScore.Application.Common;

namespace LocalScore.Application.Teams;

public static class TeamErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "team.not_found",
        "The team was not found.");

    public static readonly Error NameConflict = Error.Conflict(
        "team.name_conflict",
        "A team with this name already exists in your catalog.");

    public static readonly Error DeleteNotAllowed = Error.Conflict(
        "team.delete_not_allowed",
        "A team that has participated in a championship cannot be deleted.");

    public static readonly Error ChampionshipNotFound = Error.NotFound(
        "championship.not_found",
        "The championship was not found.");

    public static readonly Error ParticipationLocked = Error.Conflict(
        "championship.participation_locked",
        "Participants in a finished championship can only be changed after reopening it.");
}
