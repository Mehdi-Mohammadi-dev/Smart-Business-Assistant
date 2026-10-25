using FluentAssertions;
using Vexora.Domain.Entities.Sunscriptions;
using Vexora.Domain.Enums.Subscriptions;
using Xunit;

namespace Vexora.Application.Test.Platform;

public sealed class SubscriptionExtendTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Extend_adds_days_to_the_current_end_when_active()
    {
        var subscription = Subscription.Create(1, SubscriptionPlanType.Basic, Now.AddDays(-10), Now.AddDays(5));
        subscription.Activate();

        subscription.Extend(30, Now);

        subscription.EndAt.Should().Be(Now.AddDays(35));
        subscription.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public void Extend_starts_a_new_period_when_expired()
    {
        var subscription = Subscription.Create(1, SubscriptionPlanType.Basic, Now.AddDays(-40), Now.AddDays(-1));
        subscription.Activate();

        subscription.Extend(30, Now);

        subscription.StartAt.Should().Be(Now);
        subscription.EndAt.Should().Be(Now.AddDays(30));
        subscription.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(366)]
    public void Extend_rejects_days_outside_the_allowed_range(int days)
    {
        var subscription = Subscription.Create(1, SubscriptionPlanType.Basic, Now, Now.AddDays(5));
        var act = () => subscription.Extend(days, Now);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
