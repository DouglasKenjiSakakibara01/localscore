using LocalScore.Domain.Championships;

namespace LocalScore.Tests;

public sealed class ChampionshipDomainTests
{
    private static readonly DateTimeOffset InitialTime = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Championship_follows_the_approved_status_cycle()
    {
        var championship = CreateChampionship();

        Assert.Equal(ChampionshipStatusCode.Draft, championship.StatusCode);
        Assert.True(championship.Publish(InitialTime.AddMinutes(1)));
        Assert.Equal(ChampionshipStatusCode.Published, championship.StatusCode);
        Assert.True(championship.Finish(InitialTime.AddMinutes(2)));
        Assert.Equal(ChampionshipStatusCode.Finished, championship.StatusCode);
        Assert.True(championship.Reopen(InitialTime.AddMinutes(3)));
        Assert.Equal(ChampionshipStatusCode.Published, championship.StatusCode);
        Assert.True(championship.ReturnToDraft(false, InitialTime.AddMinutes(4)));
        Assert.Equal(ChampionshipStatusCode.Draft, championship.StatusCode);
    }

    [Fact]
    public void Slug_stays_locked_after_first_publication_even_back_in_draft()
    {
        var championship = CreateChampionship();
        championship.Publish(InitialTime.AddMinutes(1));
        championship.ReturnToDraft(false, InitialTime.AddMinutes(2));

        var updated = championship.Update(
            "Copa Atualizada",
            "outro-slug",
            null,
            null,
            null,
            InitialTime.AddMinutes(3));

        Assert.False(updated);
        Assert.Equal("copa-local", championship.Slug);
        Assert.False(championship.CanChangeSlug);
    }

    [Fact]
    public void Published_championship_cannot_return_to_draft_with_a_schedule()
    {
        var championship = CreateChampionship();
        championship.Publish(InitialTime.AddMinutes(1));

        Assert.False(championship.ReturnToDraft(true, InitialTime.AddMinutes(2)));
        Assert.Equal(ChampionshipStatusCode.Published, championship.StatusCode);
    }

    [Fact]
    public void Participation_reuses_identity_when_reactivated()
    {
        var participation = ChampionshipTeam.Create(Guid.NewGuid(), Guid.NewGuid(), InitialTime);
        var originalId = participation.Id;

        participation.Remove(InitialTime.AddDays(1));
        participation.Activate(InitialTime.AddDays(2));

        Assert.Equal(originalId, participation.Id);
        Assert.True(participation.IsActive);
        Assert.Null(participation.RemovedAt);
        Assert.Equal(InitialTime.AddDays(2), participation.RegisteredAt);
    }

    private static Championship CreateChampionship() =>
        Championship.Create(
            Guid.NewGuid(),
            "Copa Local",
            "copa-local",
            null,
            null,
            null,
            InitialTime);
}
