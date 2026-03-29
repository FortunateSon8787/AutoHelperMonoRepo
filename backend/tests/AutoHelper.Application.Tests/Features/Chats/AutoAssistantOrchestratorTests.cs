using AutoFixture;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Chats.Orchestration;
using AutoHelper.Domain.Chats;
using AutoHelper.Domain.Customers;
using AutoHelper.Domain.ServiceRecords;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Chats;

public class AutoAssistantOrchestratorTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<ILlmProvider> _llm = new();
    private readonly Mock<IInvalidChatRequestRepository> _invalidRequests = new();
    private readonly Mock<IVehicleRepository> _vehicles = new();
    private readonly Mock<IServiceRecordRepository> _serviceRecords = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILlmModelSelector> _modelSelector = new();
    private readonly Mock<ILogger<AutoAssistantOrchestrator>> _logger = new();
    private readonly AutoAssistantOrchestrator _sut;

    public AutoAssistantOrchestratorTests()
    {
        _modelSelector.Setup(m => m.RouterModel).Returns("gpt-4.1-nano");
        _modelSelector.Setup(m => m.DefaultModel).Returns("gpt-4.1-mini");
        _modelSelector.Setup(m => m.EscalationModel).Returns("gpt-4.1");

        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _serviceRecords
            .Setup(s => s.GetByVehicleIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _sut = new AutoAssistantOrchestrator(
            _llm.Object,
            _invalidRequests.Object,
            _vehicles.Object,
            _serviceRecords.Object,
            _unitOfWork.Object,
            _modelSelector.Object,
            _logger.Object);
    }

    // ─── Valid request flow ───────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_WithValidClassification_ShouldReturnAssistantReply()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        var classification = new ClassificationResult
        {
            Mode = "FaultHelp",
            IsValid = true,
            ShouldEscalate = false,
            ShouldDecrementQuota = true
        };

        _llm.Setup(l => l.GenerateStructuredAsync<ClassificationResult>(
                "gpt-4.1-nano",
                It.IsAny<string>(),
                "My car makes a noise",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(classification);

        _llm.Setup(l => l.GenerateTextAsync(
                "gpt-4.1-mini",
                It.IsAny<string>(),
                "My car makes a noise",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("It could be a wheel bearing issue.");

        // Act
        var result = await _sut.ProcessAsync(chat, customer, "My car makes a noise", "ru", CancellationToken.None);

        // Assert
        result.WasValid.ShouldBeTrue();
        result.AssistantReply.ShouldBe("It could be a wheel bearing issue.");
        result.QuotaDecremented.ShouldBeTrue();
    }

    [Fact]
    public async Task ProcessAsync_WithValidClassification_ShouldCallSaveChanges()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        _llm.Setup(l => l.GenerateStructuredAsync<ClassificationResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClassificationResult { Mode = "FaultHelp", IsValid = true, ShouldDecrementQuota = true });

        _llm.Setup(l => l.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Some answer");

        // Act
        await _sut.ProcessAsync(chat, customer, "My car noise", "ru", CancellationToken.None);

        // Assert — save called for both exchange persistence and quota decrement
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessAsync_WithEscalation_ShouldUseEscalationModel()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        _llm.Setup(l => l.GenerateStructuredAsync<ClassificationResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClassificationResult
            {
                Mode = "FaultHelp",
                IsValid = true,
                ShouldEscalate = true,
                ShouldDecrementQuota = true
            });

        _llm.Setup(l => l.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Complex diagnosis");

        // Act
        await _sut.ProcessAsync(chat, customer, "Complex fault", "ru", CancellationToken.None);

        // Assert — escalation model used
        _llm.Verify(l => l.GenerateTextAsync(
            "gpt-4.1",
            It.IsAny<string>(),
            "Complex fault",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── PartnerAdvice — quota not decremented ────────────────────────────────

    [Fact]
    public async Task ProcessAsync_PartnerAdviceMode_ShouldNotDecrementQuota()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.PartnerAdvice, "Partner chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        _llm.Setup(l => l.GenerateStructuredAsync<ClassificationResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClassificationResult
            {
                Mode = "PartnerAdvice",
                IsValid = true,
                ShouldEscalate = false,
                ShouldDecrementQuota = false  // PartnerAdvice never decrements
            });

        _llm.Setup(l => l.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Here are some partners.");

        // Act
        var result = await _sut.ProcessAsync(chat, customer, "Find me a mechanic", "ru", CancellationToken.None);

        // Assert
        result.QuotaDecremented.ShouldBeFalse();
    }

    // ─── Invalid / off-topic request ─────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_WithInvalidClassification_ShouldNotCallGenerateText()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        _llm.Setup(l => l.GenerateStructuredAsync<ClassificationResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClassificationResult
            {
                Mode = "out_of_scope",
                IsValid = false,
                RejectionReason = "off_topic"
            });

        // Act
        var result = await _sut.ProcessAsync(chat, customer, "Tell me a recipe", "ru", CancellationToken.None);

        // Assert
        result.WasValid.ShouldBeFalse();
        result.QuotaDecremented.ShouldBeFalse();

        _llm.Verify(l => l.GenerateTextAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WithInvalidClassification_ShouldLogToInvalidChatRequests()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        _llm.Setup(l => l.GenerateStructuredAsync<ClassificationResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClassificationResult
            {
                IsValid = false,
                RejectionReason = "unsafe"
            });

        // Act
        await _sut.ProcessAsync(chat, customer, "Illegal question", "ru", CancellationToken.None);

        // Assert — audit record written
        _invalidRequests.Verify(r => r.Add(It.Is<InvalidChatRequest>(req =>
            req.ChatId == chat.Id &&
            req.CustomerId == customer.Id &&
            req.RejectionReason == "unsafe")),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WithInvalidClassification_ShouldSaveChangesOnce()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        _llm.Setup(l => l.GenerateStructuredAsync<ClassificationResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClassificationResult { IsValid = false, RejectionReason = "off_topic" });

        // Act
        await _sut.ProcessAsync(chat, customer, "Off topic", "ru", CancellationToken.None);

        // Assert — only one save (for the invalid message + audit record)
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── Classifier failure — fail open ──────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_WhenClassifierThrows_ShouldFailOpenAndProceed()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        _llm.Setup(l => l.GenerateStructuredAsync<ClassificationResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("LLM unavailable"));

        _llm.Setup(l => l.GenerateTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Fallback answer");

        // Act — should not throw
        var result = await _sut.ProcessAsync(chat, customer, "Noise in engine", "ru", CancellationToken.None);

        // Assert — proceeded despite classifier failure
        result.WasValid.ShouldBeTrue();
        result.AssistantReply.ShouldBe("Fallback answer");
    }
}
