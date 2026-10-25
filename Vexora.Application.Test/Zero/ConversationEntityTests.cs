using FluentAssertions;
using Vexora.Domain.Entities.Conversations;
using Vexora.Domain.Enums.Conversations;
using Vexora.Domain.Exceptions.Conversations;
using Xunit;

namespace Vexora.Application.Test.Zero;

public sealed class ConversationEntityTests
{
    private static readonly DateTimeOffset T0 = new(2026, 10, 24, 9, 0, 0, TimeSpan.Zero);

    private static Conversation Start() => Conversation.Start(7, T0);

    [Fact]
    public void Should_Start_Empty_With_A_New_Id()
    {
        var conversation = Start();

        conversation.Id.Should().NotBe(Guid.Empty);
        conversation.UserId.Should().Be(7);
        conversation.Title.Should().Be("");
        conversation.Messages.Should().BeEmpty();
    }

    [Fact]
    public void Should_Reject_An_Invalid_User()
    {
        var action = () => Conversation.Start(0, T0);

        action.Should().Throw<Exception>();
    }

    [Fact]
    public void First_User_Message_Should_Become_The_Title()
    {
        var conversation = Start();

        conversation.AddMessage(MessageRole.User, "علی رضایی ۵۰۰ هزار تومان داد", T0.AddMinutes(1));
        conversation.AddMessage(MessageRole.User, "پیام دوم", T0.AddMinutes(2));

        conversation.Title.Should().Be("علی رضایی ۵۰۰ هزار تومان داد");
    }

    [Fact]
    public void An_Assistant_Message_Should_Not_Set_The_Title()
    {
        var conversation = Start();

        conversation.AddMessage(MessageRole.Assistant, "سلام", T0.AddMinutes(1));

        conversation.Title.Should().Be("");
    }

    [Fact]
    public void Long_Title_Should_Be_Cut_On_One_Line()
    {
        var title = Conversation.BuildTitle(new string('ا', 100) + "\nخط دوم");

        title.Length.Should().Be(Conversation.TitleLength);
        title.EndsWith("…").Should().BeTrue();
    }

    [Fact]
    public void Adding_A_Message_Should_Move_Last_Message_Time()
    {
        var conversation = Start();

        conversation.AddMessage(MessageRole.User, "سلام", T0.AddMinutes(5));

        conversation.LastMessageAt.Should().Be(T0.AddMinutes(5));
        conversation.Messages.Should().HaveCount(1);
    }

    [Fact]
    public void Should_Reject_Empty_Text()
    {
        var action = () => Start().AddMessage(MessageRole.User, "  ", T0);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void A_Pending_Card_Should_Be_Confirmed_Only_Once()
    {
        var message = Start().AddMessage(
            MessageRole.Assistant,
            "کارت",
            T0,
            "{}",
            CardState.Pending);

        message.IsPending.Should().BeTrue();

        message.Confirm();

        message.CardState.Should().Be(CardState.Confirmed);
        message.IsPending.Should().BeFalse();

        var action = () => message.Confirm();

        action.Should().Throw<CardNotPendingException>();
    }

    [Fact]
    public void A_Cancelled_Card_Cannot_Be_Confirmed()
    {
        var message = Start().AddMessage(MessageRole.Assistant, "کارت", T0, "{}", CardState.Pending);

        message.Cancel();

        var action = () => message.Confirm();

        action.Should().Throw<CardNotPendingException>();
    }

    [Fact]
    public void A_Normal_Message_Is_Not_A_Card()
    {
        var message = Start().AddMessage(MessageRole.Assistant, "سلام", T0);

        message.CardState.Should().Be(CardState.None);

        var action = () => message.Confirm();

        action.Should().Throw<CardNotPendingException>();
    }

    [Fact]
    public void An_Old_Card_Can_Expire()
    {
        var message = Start().AddMessage(MessageRole.Assistant, "کارت", T0, "{}", CardState.Pending);

        message.Expire();

        message.CardState.Should().Be(CardState.Expired);
    }

    [Fact]
    public void Soft_Delete_Should_Keep_The_First_Time()
    {
        var conversation = Start();

        conversation.SoftDelete(T0.AddHours(1));
        conversation.SoftDelete(T0.AddHours(2));

        conversation.IsDelete.Should().BeTrue();
        conversation.DeletedAt.Should().Be(T0.AddHours(1));
    }
}
