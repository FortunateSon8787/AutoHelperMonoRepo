using AutoFixture;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Chats.Orchestration;
using AutoHelper.Domain.Chats;
using AutoHelper.Domain.Customers;
using Microsoft.Extensions.Configuration;
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
    private readonly Mock<IMarketPriceGateway> _marketPrices = new();
    private readonly Mock<IPartnerSearchService> _partnerSearch = new();
    private readonly Mock<IConfiguration> _configuration = new();
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

        _marketPrices
            .Setup(m => m.GetMarketPriceBenchmarksAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _partnerSearch
            .Setup(p => p.FindPartnersAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _configuration.Setup(c => c["PartnerAdvice:MaxResults"]).Returns("5");

        _sut = new AutoAssistantOrchestrator(
            _llm.Object,
            _invalidRequests.Object,
            Mock.Of<IChatRepository>(),
            _vehicles.Object,
            _serviceRecords.Object,
            _marketPrices.Object,
            _partnerSearch.Object,
            _configuration.Object,
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

    // ─── WorkClarification — initial processing ───────────────────────────────

    [Fact]
    public async Task ProcessWorkClarificationInitialAsync_ShouldReturnStructuredReplyAndCompleteChat()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.WorkClarification, "Work check");
        var customer = Customer.CreateWithPassword("Bob", "bob@test.com", "hash");

        var input = new WorkClarificationInput
        {
            WorksPerformed = "Brake pad replacement",
            WorkReason = "Squealing noise",
            LaborCost = 3000m,
            PartsCost = 5000m,
            Guarantees = "6 months"
        };

        _llm.Setup(l => l.GenerateStructuredAsync<WorkClarificationLlmResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkClarificationLlmResult
            {
                WorkReasonRelevance = "high",
                WorkReasonExplanation = "Squealing is a classic sign of worn pads.",
                LaborPriceAssessment = "near_market",
                LaborPriceExplanation = "Labor is within market range.",
                PartsPriceAssessment = "near_market",
                PartsPriceExplanation = "Parts cost is typical.",
                GuaranteeAssessment = "normal",
                GuaranteeExplanation = "Standard 6-month guarantee.",
                OverallHonesty = "good",
                OverallExplanation = "Service appears honest.",
                Disclaimer = "Estimate only."
            });

        // Act
        var result = await _sut.ProcessWorkClarificationInitialAsync(
            chat, customer, input, "ru", CancellationToken.None);

        // Assert
        result.WasValid.ShouldBeTrue();
        result.QuotaDecremented.ShouldBeTrue();
        result.ResponseStage.ShouldBe("work_clarification_result");
        result.ChatStatus.ShouldBe(ChatStatus.Completed);
        result.AssistantReply.ShouldContain("Хорошая");       // OverallHonesty = "good"
        result.AssistantReply.ShouldContain("Высокая");       // WorkReasonRelevance = "high"
        result.AssistantReply.ShouldContain("По рынку");      // LaborPriceAssessment = "near_market"
        result.AssistantReply.ShouldContain("Estimate only.");
    }

    [Fact]
    public async Task ProcessWorkClarificationInitialAsync_ShouldDecrementQuotaAndSaveTwice()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.WorkClarification, "Work check");
        var customer = Customer.CreateWithPassword("Bob", "bob@test.com", "hash");

        var input = new WorkClarificationInput
        {
            WorksPerformed = "Oil change",
            WorkReason = "Scheduled maintenance",
            LaborCost = 500m,
            PartsCost = 1500m
        };

        _llm.Setup(l => l.GenerateStructuredAsync<WorkClarificationLlmResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkClarificationLlmResult
            {
                WorkReasonRelevance = "high",
                WorkReasonExplanation = "Scheduled.",
                LaborPriceAssessment = "below_market",
                LaborPriceExplanation = "Cheap.",
                PartsPriceAssessment = "near_market",
                PartsPriceExplanation = "Normal.",
                GuaranteeAssessment = "unclear",
                GuaranteeExplanation = "No guarantees stated.",
                OverallHonesty = "fair",
                OverallExplanation = "Looks ok.",
                Disclaimer = "Estimate."
            });

        // Act
        await _sut.ProcessWorkClarificationInitialAsync(chat, customer, input, "ru", CancellationToken.None);

        // Assert — SaveChanges called twice: once for chat exchange, once for quota decrement
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessWorkClarificationInitialAsync_WhenBenchmarksAvailable_ShouldInjectIntoSystemPrompt()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.WorkClarification, "Work check");
        var customer = Customer.CreateWithPassword("Bob", "bob@test.com", "hash");

        var input = new WorkClarificationInput
        {
            WorksPerformed = "Timing belt replacement",
            WorkReason = "Preventive maintenance",
            LaborCost = 10000m,
            PartsCost = 8000m
        };

        _marketPrices
            .Setup(m => m.GetMarketPriceBenchmarksAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Typical labor: 8000–12000 RUB. Typical parts: 6000–10000 RUB.");

        string? capturedSystemPrompt = null;
        _llm.Setup(l => l.GenerateStructuredAsync<WorkClarificationLlmResult>(
                It.IsAny<string>(), It.Is<string>(p => true), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((_, prompt, _, _) => capturedSystemPrompt = prompt)
            .ReturnsAsync(new WorkClarificationLlmResult
            {
                WorkReasonRelevance = "high", WorkReasonExplanation = "Ok.",
                LaborPriceAssessment = "near_market", LaborPriceExplanation = "Ok.",
                PartsPriceAssessment = "near_market", PartsPriceExplanation = "Ok.",
                GuaranteeAssessment = "unclear", GuaranteeExplanation = "Ok.",
                OverallHonesty = "good", OverallExplanation = "Ok.",
                Disclaimer = "Estimate."
            });

        // Act
        await _sut.ProcessWorkClarificationInitialAsync(chat, customer, input, "ru", CancellationToken.None);

        // Assert — benchmarks should be included in system prompt
        capturedSystemPrompt.ShouldNotBeNull();
        capturedSystemPrompt.ShouldContain("MARKET_BENCHMARKS");
        capturedSystemPrompt.ShouldContain("Typical labor");
    }

    // ─── PartnerAdvice — initial processing ──────────────────────────────────

    [Fact]
    public async Task ProcessPartnerAdviceInitialAsync_ShouldReturnFormattedReplyAndCompleteChat()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.PartnerAdvice, "Find tire shop");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        var input = new PartnerAdviceInput
        {
            Request = "I need a tire change",
            Lat = 50.45,
            Lng = 30.52
        };

        // Step 1: classifier returns category
        _llm.SetupSequence(l => l.GenerateStructuredAsync<PartnerAdviceLlmResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PartnerAdviceLlmResult { ServiceCategory = "tire_service", Urgency = "medium" })
            .ReturnsAsync(new PartnerAdviceLlmResult { ResponseText = "Here are 2 tire shops near you." });

        _partnerSearch
            .Setup(p => p.FindPartnersAsync(50.45, 30.52, "tire_service", "ru", 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new PartnerCard("own_partner", true, "TireMax", "Khreshchatyk 1", "+380501234567", null, null, null, true, 0.8, "Tire service", false)
            ]);

        // Act
        var result = await _sut.ProcessPartnerAdviceInitialAsync(chat, customer, input, "ru", CancellationToken.None);

        // Assert
        result.WasValid.ShouldBeTrue();
        result.QuotaDecremented.ShouldBeFalse();
        result.ResponseStage.ShouldBe("partner_advice_result");
        result.ChatStatus.ShouldBe(ChatStatus.Completed);
        result.AssistantReply.ShouldBe("Here are 2 tire shops near you.");
    }

    [Fact]
    public async Task ProcessPartnerAdviceInitialAsync_ShouldNotDecrementQuota()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.PartnerAdvice, "Find mechanic");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");
        var input = new PartnerAdviceInput { Request = "Need a car repair shop", Lat = 50.0, Lng = 30.0 };

        _llm.SetupSequence(l => l.GenerateStructuredAsync<PartnerAdviceLlmResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PartnerAdviceLlmResult { ServiceCategory = "car_service", Urgency = "low" })
            .ReturnsAsync(new PartnerAdviceLlmResult { ResponseText = "Found 3 services." });

        // Act
        var result = await _sut.ProcessPartnerAdviceInitialAsync(chat, customer, input, "ru", CancellationToken.None);

        // Assert
        result.QuotaDecremented.ShouldBeFalse();
        // SaveChanges called once (chat exchange + complete) — no quota save
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPartnerAdviceInitialAsync_WhenPartnerSearchFails_ShouldStillReturnResponse()
    {
        // Arrange
        var chat = Chat.Create(Guid.NewGuid(), ChatMode.PartnerAdvice, "Find tow truck");
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");
        var input = new PartnerAdviceInput { Request = "Need a tow truck", Lat = 50.0, Lng = 30.0 };

        _llm.SetupSequence(l => l.GenerateStructuredAsync<PartnerAdviceLlmResult>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PartnerAdviceLlmResult { ServiceCategory = "tow_truck", Urgency = "high" })
            .ReturnsAsync(new PartnerAdviceLlmResult { ResponseText = "No partners found near you." });

        _partnerSearch
            .Setup(p => p.FindPartnersAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Search service unavailable"));

        // Act — should not throw; fallback to empty cards
        var result = await _sut.ProcessPartnerAdviceInitialAsync(chat, customer, input, "ru", CancellationToken.None);

        // Assert
        result.WasValid.ShouldBeTrue();
        result.AssistantReply.ShouldBe("No partners found near you.");
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
