using AutoHelper.Domain.Chats;
using AutoHelper.Domain.Exceptions;
using Shouldly;

namespace AutoHelper.Domain.Tests.Chats;

public class ChatTests
{
    // ─── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldHaveActiveStatus()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Engine noise");

        chat.Status.ShouldBe(ChatStatus.Active);
        chat.AllowOneAdditionalQuestion.ShouldBeFalse();
        chat.CanReceiveMessage().ShouldBeTrue();
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrow()
    {
        Should.Throw<DomainException>(() =>
            Chat.Create(Guid.Empty, ChatMode.FaultHelp, "Title"));
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrow()
    {
        Should.Throw<DomainException>(() =>
            Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "   "));
    }

    // ─── TransitionToAwaitingAnswers ──────────────────────────────────────────

    [Fact]
    public void TransitionToAwaitingAnswers_WhenFaultHelpAndActive_ShouldSucceed()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Fault");

        chat.TransitionToAwaitingAnswers();

        chat.Status.ShouldBe(ChatStatus.AwaitingUserAnswers);
        chat.CanReceiveMessage().ShouldBeTrue();
    }

    [Fact]
    public void TransitionToAwaitingAnswers_WhenNotFaultHelp_ShouldThrow()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.PartnerAdvice, "Partner");

        Should.Throw<DomainException>(() => chat.TransitionToAwaitingAnswers())
            .Message.ShouldContain("FaultHelp");
    }

    [Fact]
    public void TransitionToAwaitingAnswers_WhenNotActive_ShouldThrow()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Fault");
        chat.TransitionToAwaitingAnswers();

        Should.Throw<DomainException>(() => chat.TransitionToAwaitingAnswers());
    }

    // ─── TransitionBackToActive ───────────────────────────────────────────────

    [Fact]
    public void TransitionBackToActive_WhenAwaitingAnswers_ShouldSucceed()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Fault");
        chat.TransitionToAwaitingAnswers();

        chat.TransitionBackToActive();

        chat.Status.ShouldBe(ChatStatus.Active);
    }

    [Fact]
    public void TransitionBackToActive_WhenActive_ShouldThrow()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Fault");

        Should.Throw<DomainException>(() => chat.TransitionBackToActive());
    }

    // ─── TransitionToFinalAnswerSent ──────────────────────────────────────────

    [Fact]
    public void TransitionToFinalAnswerSent_WhenFaultHelpAndActive_ShouldSetFlagAndStatus()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Fault");

        chat.TransitionToFinalAnswerSent();

        chat.Status.ShouldBe(ChatStatus.FinalAnswerSent);
        chat.AllowOneAdditionalQuestion.ShouldBeTrue();
        chat.CanReceiveMessage().ShouldBeTrue();
    }

    [Fact]
    public void TransitionToFinalAnswerSent_WhenNotFaultHelp_ShouldThrow()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.WorkClarification, "Service");

        Should.Throw<DomainException>(() => chat.TransitionToFinalAnswerSent())
            .Message.ShouldContain("FaultHelp");
    }

    [Fact]
    public void TransitionToFinalAnswerSent_WhenNotActive_ShouldThrow()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Fault");
        chat.TransitionToFinalAnswerSent();

        Should.Throw<DomainException>(() => chat.TransitionToFinalAnswerSent());
    }

    // ─── Complete ─────────────────────────────────────────────────────────────

    [Fact]
    public void Complete_WhenFinalAnswerSent_ShouldSetCompleted()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Fault");
        chat.TransitionToFinalAnswerSent();

        chat.Complete();

        chat.Status.ShouldBe(ChatStatus.Completed);
        chat.AllowOneAdditionalQuestion.ShouldBeFalse();
        chat.CanReceiveMessage().ShouldBeFalse();
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldThrow()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Fault");
        chat.TransitionToFinalAnswerSent();
        chat.Complete();

        Should.Throw<DomainException>(() => chat.Complete())
            .Message.ShouldContain("already completed");
    }

    // ─── CanReceiveMessage ────────────────────────────────────────────────────

    [Theory]
    [InlineData(ChatMode.FaultHelp)]
    [InlineData(ChatMode.WorkClarification)]
    [InlineData(ChatMode.PartnerAdvice)]
    public void CanReceiveMessage_WhenActive_ShouldReturnTrue(ChatMode mode)
    {
        var chat = Chat.Create(Guid.NewGuid(), mode, "Title");

        chat.CanReceiveMessage().ShouldBeTrue();
    }

    [Fact]
    public void CanReceiveMessage_WhenCompleted_ShouldReturnFalse()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Fault");
        chat.TransitionToFinalAnswerSent();
        chat.Complete();

        chat.CanReceiveMessage().ShouldBeFalse();
    }

    // ─── Full state machine ───────────────────────────────────────────────────

    [Fact]
    public void FullFaultHelpFlow_WithFollowUp_ShouldTransitionCorrectly()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Fault");

        // Active
        chat.Status.ShouldBe(ChatStatus.Active);

        // Follow-up question asked
        chat.AddExchange("My car makes noise", "When does it occur?");
        chat.TransitionToAwaitingAnswers();
        chat.Status.ShouldBe(ChatStatus.AwaitingUserAnswers);
        chat.CanReceiveMessage().ShouldBeTrue();

        // User answered, back to active
        chat.TransitionBackToActive();
        chat.Status.ShouldBe(ChatStatus.Active);

        // Final answer delivered
        chat.AddExchange("It occurs when turning", "Diagnosis: wheel bearing");
        chat.TransitionToFinalAnswerSent();
        chat.Status.ShouldBe(ChatStatus.FinalAnswerSent);
        chat.AllowOneAdditionalQuestion.ShouldBeTrue();
        chat.CanReceiveMessage().ShouldBeTrue();

        // Additional question answered → complete
        chat.AddExchange("How long can I drive?", "Max 100 km");
        chat.Complete();
        chat.Status.ShouldBe(ChatStatus.Completed);
        chat.CanReceiveMessage().ShouldBeFalse();
    }

    // ─── SoftDelete ───────────────────────────────────────────────────────────

    [Fact]
    public void SoftDelete_WhenNotDeleted_ShouldSetIsDeletedTrue()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");

        chat.SoftDelete();

        chat.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ShouldThrowDomainException()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        chat.SoftDelete();

        Should.Throw<DomainException>(() => chat.SoftDelete())
            .Message.ShouldContain("already deleted");
    }

    [Fact]
    public void SoftDelete_ShouldNotAffectChatStatus()
    {
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.WorkClarification, "Work review");

        chat.SoftDelete();

        chat.Status.ShouldBe(ChatStatus.Active);
        chat.IsDeleted.ShouldBeTrue();
    }
}
