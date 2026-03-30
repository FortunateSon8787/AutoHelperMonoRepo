using AutoFixture;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Chats;
using AutoHelper.Application.Features.Chats.Orchestration;
using AutoHelper.Application.Features.Chats.SendMessage;
using AutoHelper.Domain.Chats;
using AutoHelper.Domain.Customers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace AutoHelper.Application.Tests.Features.Chats;

public class SendMessageCommandHandlerTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IChatRepository> _chats = new();
    private readonly Mock<ICustomerRepository> _customers = new();
    private readonly Mock<ILlmProvider> _llm = new();
    private readonly Mock<IInvalidChatRequestRepository> _invalidRequests = new();
    private readonly Mock<IVehicleRepository> _vehicles = new();
    private readonly Mock<IServiceRecordRepository> _serviceRecords = new();
    private readonly Mock<IMarketPriceGateway> _marketPrices = new();
    private readonly Mock<IPartnerSearchService> _partnerSearch = new();
    private readonly Mock<IConfiguration> _configuration = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILlmModelSelector> _modelSelector = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly SendMessageCommandHandler _sut;

    public SendMessageCommandHandlerTests()
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

        var orchestrator = new AutoAssistantOrchestrator(
            _llm.Object,
            _invalidRequests.Object,
            _vehicles.Object,
            _serviceRecords.Object,
            _marketPrices.Object,
            _partnerSearch.Object,
            _configuration.Object,
            _unitOfWork.Object,
            _modelSelector.Object,
            Mock.Of<ILogger<AutoAssistantOrchestrator>>());

        _sut = new SendMessageCommandHandler(
            _chats.Object,
            _customers.Object,
            orchestrator,
            _currentUser.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnNotAuthenticated()
    {
        _currentUser.Setup(u => u.Id).Returns((Guid?)null);

        var result = await _sut.Handle(
            new SendMessageCommand(Guid.NewGuid(), "Hello", "ru"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(ChatErrors.NotAuthenticated);
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldReturnCustomerNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.Id).Returns(userId);
        _customers.Setup(c => c.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var result = await _sut.Handle(
            new SendMessageCommand(Guid.NewGuid(), "Hello", "ru"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(ChatErrors.CustomerNotFound);
    }

    [Fact]
    public async Task Handle_WhenChatNotFound_ShouldReturnChatNotFound()
    {
        var userId = Guid.NewGuid();
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");
        _currentUser.Setup(u => u.Id).Returns(userId);
        _customers.Setup(c => c.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _chats.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chat?)null);

        var result = await _sut.Handle(
            new SendMessageCommand(Guid.NewGuid(), "Hello", "ru"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(ChatErrors.ChatNotFound);
    }

    [Fact]
    public async Task Handle_WhenFreeUserUsesNonPartnerMode_ShouldReturnSubscriptionRequired()
    {
        var userId = Guid.NewGuid();
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");
        // Free customer, mode = FaultHelp (requires Premium)
        var chat = Chat.Create(userId, ChatMode.FaultHelp, "Fault chat");

        _currentUser.Setup(u => u.Id).Returns(userId);
        _customers.Setup(c => c.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _chats.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chat);

        var result = await _sut.Handle(
            new SendMessageCommand(chat.Id, "My engine is failing", "ru"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(ChatErrors.SubscriptionRequired);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnSuccessWithReply()
    {
        var userId = Guid.NewGuid();
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");
        // PartnerAdvice is accessible without Premium
        var chat = Chat.Create(userId, ChatMode.PartnerAdvice, "Partner chat");

        _currentUser.Setup(u => u.Id).Returns(userId);
        _customers.Setup(c => c.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _chats.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chat);

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
            .ReturnsAsync("Here are some partners nearby.");

        var result = await _sut.Handle(
            new SendMessageCommand(chat.Id, "Find me a tire shop", "ru"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value!.AssistantReply.ShouldBe("Here are some partners nearby.");
        result.Value.WasValid.ShouldBeTrue();
    }
}
