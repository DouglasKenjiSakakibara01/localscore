using LocalScore.Application.Common;

namespace LocalScore.Application.Championships;

public static class ChampionshipErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "championship.not_found",
        "The championship was not found.");

    public static readonly Error SlugConflict = Error.Conflict(
        "championship.slug_conflict",
        "The championship slug is already in use.");

    public static readonly Error InvalidTransition = Error.Conflict(
        "championship.invalid_transition",
        "The requested championship status transition is not allowed.");

    public static readonly Error ReadOnly = Error.Conflict(
        "championship.read_only",
        "A finished championship must be reopened before it can be edited.");

    public static readonly Error SlugLocked = Error.Conflict(
        "championship.slug_locked",
        "The slug cannot be changed after the championship has been published.");

    public static readonly Error DeleteNotAllowed = Error.Conflict(
        "championship.delete_not_allowed",
        "Only a draft championship without participation history can be deleted.");
}
