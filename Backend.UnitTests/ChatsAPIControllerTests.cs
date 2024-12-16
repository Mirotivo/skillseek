using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using skillseek.Controllers;

namespace Backend.UnitTests;

public class ChatsAPIControllerTests
    {
        private readonly Mock<IChatService> _mockChatService;
        private readonly Mock<ILogger<ChatsAPIController>> _mockLogger;
        private readonly ChatsAPIController _controller;

        public ChatsAPIControllerTests()
        {
            _mockChatService = new Mock<IChatService>();
            _mockLogger = new Mock<ILogger<ChatsAPIController>>();
            _controller = new ChatsAPIController(_mockChatService.Object, null, _mockLogger.Object);
        }

        [Fact]
        public void GetChats_ReturnsNotFound_WhenNoChatsExist()
        {
            // Arrange
            int userId = 1;
            _mockChatService.Setup(service => service.GetChats(userId)).Returns(new List<ChatDto>());

            // Act
            var result = _controller.GetChats();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No chats found for this user.", notFoundResult.Value);
        }

        [Fact]
        public void GetChats_ReturnsChats_WhenChatsExist()
        {
            // Arrange
            int userId = 1;
            var chats = new List<ChatDto>
            {
                new ChatDto { Id = 1, Name = "Tutor 1" },
                new ChatDto { Id = 2, Name = "Tutor 2" }
            };
            _mockChatService.Setup(service => service.GetChats(userId)).Returns(chats);

            // Act
            var result = _controller.GetChats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedChats = Assert.IsType<List<ChatDto>>(okResult.Value);
            Assert.Equal(2, returnedChats.Count);
        }

        [Fact]
        public void SendMessage_ReturnsBadRequest_WhenMessageDataIsInvalid()
        {
            // Arrange
            var invalidMessageDto = new SendMessageDto
            {
                RecipientId = 0,
                Content = "Hello"
            };

            // Act
            var result = _controller.SendMessage(invalidMessageDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid listing ID.", badRequestResult.Value);
        }

        [Fact]
        public void SendMessage_ReturnsOk_WhenMessageIsSentSuccessfully()
        {
            // Arrange
            var validMessageDto = new SendMessageDto
            {
                RecipientId = 2,
                ListingId = 1,
                Content = "Hello"
            };
            int senderId = 1;

            _mockChatService.Setup(service => service.SendMessage(validMessageDto, senderId)).Returns(true);

            // Act
            var result = _controller.SendMessage(validMessageDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<dynamic>(okResult.Value);
            Assert.True(response.success);
            Assert.Equal("Message sent successfully.", response.message);
        }

        [Fact]
        public void SendMessage_ReturnsBadRequest_WhenMessageSendingFails()
        {
            // Arrange
            var validMessageDto = new SendMessageDto
            {
                RecipientId = 2,
                ListingId = 1,
                Content = "Hello"
            };
            int senderId = 1;

            _mockChatService.Setup(service => service.SendMessage(validMessageDto, senderId)).Returns(false);

            // Act
            var result = _controller.SendMessage(validMessageDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to send the message.", badRequestResult.Value);
        }
    }