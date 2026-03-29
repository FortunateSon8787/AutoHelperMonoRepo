using AutoHelper.Domain.Chats;
using Shouldly;

namespace AutoHelper.Domain.Tests.Chats;

public class InvalidChatRequestTests
{
    [Fact]
    public void Create_WithValidArgs_ShouldSetAllProperties()
    {
        var chatId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var record = InvalidChatRequest.Create(chatId, customerId, "Tell me a recipe", "off_topic");

        record.Id.ShouldNotBe(Guid.Empty);
        record.ChatId.ShouldBe(chatId);
        record.CustomerId.ShouldBe(customerId);
        record.UserInput.ShouldBe("Tell me a recipe");
        record.RejectionReason.ShouldBe("off_topic");
        record.CreatedAt.ShouldBeGreaterThan(DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public void Create_WithBlankRejectionReason_ShouldDefaultToUnknown()
    {
        var record = InvalidChatRequest.Create(Guid.NewGuid(), Guid.NewGuid(), "input", "");

        record.RejectionReason.ShouldBe("unknown");
    }

    [Theory]
    [InlineData("off_topic")]
    [InlineData("unsafe")]
    [InlineData("missing_context")]
    [InlineData("out_of_scope")]
    public void Create_WithKnownRejectionReasons_ShouldPreserveReason(string reason)
    {
        var record = InvalidChatRequest.Create(Guid.NewGuid(), Guid.NewGuid(), "some input", reason);

        record.RejectionReason.ShouldBe(reason);
    }

    [Fact]
    public void Create_TwoRecords_ShouldHaveDifferentIds()
    {
        var r1 = InvalidChatRequest.Create(Guid.NewGuid(), Guid.NewGuid(), "input1", "off_topic");
        var r2 = InvalidChatRequest.Create(Guid.NewGuid(), Guid.NewGuid(), "input2", "off_topic");

        r1.Id.ShouldNotBe(r2.Id);
    }
}
