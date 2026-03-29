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

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static DiagnosticsLlmResult MakeFollowUp(string question) =>
        new() { ResponseStage = "follow_up", FollowUpQuestion = question };

    private static DiagnosticsLlmResult MakeDiagnosticResult() =>
        new()
        {
            ResponseStage = "diagnostic_result",
            PotentialProblems =
            [
                new DiagnosticProblem
                {
                    Name = "Wheel bearing",
                    Probability = 0.8,
                    PossibleCauses = "Worn bearing",
                    RecommendedActions = "Replace bearing"
                }
            ],
            Urgency = "medium",
            CurrentRisks = "Increased noise, potential failure",
            SafeToDrive = true,
            Disclaimer = "This is an estimate only."
        };

    private void SetupFaultHelpValidClassification(bool shouldEscalate = false) =>
        _llm.Setup(l => l.GenerateStructuredAsync<ClassificationResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClassificationResult
            {
                Mode = "FaultHelp",
                IsValid = true,
                ShouldEscalate = shouldEscalate,
                ShouldDecrementQuota = true
            });

    // ─── FaultHelp — follow-up questions ─────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_FaultHelp_WhenLlmReturnsFollowUp_ShouldTransitionToAwaitingAnswers()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        SetupFaultHelpValidClassification();
        _llm.Setup(l => l.GenerateStructuredAsync<DiagnosticsLlmResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeFollowUp("When does the noise occur?"));

        // Act
        var result = await _sut.ProcessAsync(chat, customer, "My car makes a noise", "ru", CancellationToken.None);

        // Assert
        result.WasValid.ShouldBeTrue();
        result.ResponseStage.ShouldBe("follow_up");
        result.ChatStatus.ShouldBe(ChatStatus.AwaitingUserAnswers);
        result.AssistantReply.ShouldBe("When does the noise occur?");
    }

    [Fact]
    public async Task ProcessAsync_FaultHelp_WhenLlmReturnsDiagnosticResult_ShouldTransitionToFinalAnswerSent()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        SetupFaultHelpValidClassification();
        _llm.Setup(l => l.GenerateStructuredAsync<DiagnosticsLlmResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDiagnosticResult());

        // Act
        var result = await _sut.ProcessAsync(chat, customer, "My car makes a noise", "ru", CancellationToken.None);

        // Assert
        result.WasValid.ShouldBeTrue();
        result.ResponseStage.ShouldBe("diagnostic_result");
        result.ChatStatus.ShouldBe(ChatStatus.FinalAnswerSent);
        result.AssistantReply.ShouldContain("Wheel bearing");
    }

    [Fact]
    public async Task ProcessAsync_FaultHelp_AfterFollowUp_WhenDiagnosticResult_ShouldTransitionToFinalAnswerSent()
    {
        // Arrange: chat is already in AwaitingUserAnswers state
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");
        chat.AddExchange("My car makes noise", "When does it occur?");
        chat.TransitionToAwaitingAnswers();

        SetupFaultHelpValidClassification();
        _llm.Setup(l => l.GenerateStructuredWithHistoryAsync<DiagnosticsLlmResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDiagnosticResult());

        // Act
        var result = await _sut.ProcessAsync(chat, customer, "It happens when turning", "ru", CancellationToken.None);

        // Assert
        result.ResponseStage.ShouldBe("diagnostic_result");
        result.ChatStatus.ShouldBe(ChatStatus.FinalAnswerSent);
    }

    [Fact]
    public async Task ProcessAsync_FaultHelp_AdditionalQuestionAfterFinalAnswer_ShouldCompleteChat()
    {
        // Arrange: chat is in FinalAnswerSent, one extra question allowed
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");
        chat.AddExchange("Initial", "Diagnosis delivered");
        chat.TransitionToFinalAnswerSent();

        SetupFaultHelpValidClassification();
        // For the additional question, the orchestrator does NOT use FaultHelp structured path
        // (responseStage=null → Complete() is called)
        _llm.Setup(l => l.GenerateStructuredAsync<DiagnosticsLlmResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDiagnosticResult());

        // Act
        var result = await _sut.ProcessAsync(chat, customer, "How long can I drive like this?", "ru", CancellationToken.None);

        // Assert — chat should be completed after additional question is answered
        result.ChatStatus.ShouldBe(ChatStatus.Completed);
    }

    // ─── FaultHelp — diagnostic result formatting ─────────────────────────────

    [Fact]
    public async Task ProcessAsync_FaultHelp_DiagnosticResult_ShouldIncludeUrgencyAndSafety()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        SetupFaultHelpValidClassification();
        _llm.Setup(l => l.GenerateStructuredAsync<DiagnosticsLlmResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DiagnosticsLlmResult
            {
                ResponseStage = "diagnostic_result",
                PotentialProblems =
                [
                    new DiagnosticProblem { Name = "Brake fade", Probability = 0.9 }
                ],
                Urgency = "stop_driving",
                SafeToDrive = false,
                Disclaimer = "Estimate only."
            });

        // Act
        var result = await _sut.ProcessAsync(chat, customer, "Brakes feel soft", "ru", CancellationToken.None);

        // Assert
        result.AssistantReply.ShouldContain("stop_driving");
        result.AssistantReply.ShouldContain("Рекомендуется остановиться");
        result.AssistantReply.ShouldContain("Estimate only.");
    }

    // ─── FaultHelp — escalation model ────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_FaultHelp_WithEscalation_ShouldUseEscalationModel()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        SetupFaultHelpValidClassification(shouldEscalate: true);
        _llm.Setup(l => l.GenerateStructuredAsync<DiagnosticsLlmResult>(
                "gpt-4.1", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDiagnosticResult());

        // Act
        await _sut.ProcessAsync(chat, customer, "Complex fault", "ru", CancellationToken.None);

        // Assert — escalation model used
        _llm.Verify(l => l.GenerateStructuredAsync<DiagnosticsLlmResult>(
            "gpt-4.1",
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── FaultHelp — quota decremented ───────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_FaultHelp_ShouldDecrementQuota()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        SetupFaultHelpValidClassification();
        _llm.Setup(l => l.GenerateStructuredAsync<DiagnosticsLlmResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeFollowUp("Follow-up?"));

        // Act
        var result = await _sut.ProcessAsync(chat, customer, "My car makes a noise", "ru", CancellationToken.None);

        // Assert
        result.QuotaDecremented.ShouldBeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
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
                ShouldDecrementQuota = false
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
    public async Task ProcessAsync_WithInvalidClassification_ShouldNotCallLlmForResponse()
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

        _llm.Verify(l => l.GenerateStructuredAsync<DiagnosticsLlmResult>(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

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
            .ReturnsAsync(new ClassificationResult { IsValid = false, RejectionReason = "unsafe" });

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
    public async Task ProcessAsync_FaultHelp_WhenClassifierThrows_ShouldFailOpenAndProceed()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.FaultHelp, "Test chat");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        _llm.Setup(l => l.GenerateStructuredAsync<ClassificationResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("LLM unavailable"));

        _llm.Setup(l => l.GenerateStructuredAsync<DiagnosticsLlmResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeFollowUp("What type of noise?"));

        // Act — should not throw
        var result = await _sut.ProcessAsync(chat, customer, "Noise in engine", "ru", CancellationToken.None);

        // Assert — proceeded despite classifier failure
        result.WasValid.ShouldBeTrue();
        result.AssistantReply.ShouldBe("What type of noise?");
    }
}
