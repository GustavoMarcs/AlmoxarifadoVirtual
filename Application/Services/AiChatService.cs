using Domain.Filters;
using Domain.Interfaces;
using Microsoft.Extensions.AI;

namespace Application.Services;

public class AiChatService : IAiChatService
{
    private readonly IChatClient _chatClient;

    public AiChatService(IChatClient chatClient) => _chatClient = chatClient;

    public async Task<string> GetResponseAsync(AiQuestion question, 
        CancellationToken cancellationToken)
    {
        var chatMessage = new ChatMessage(ChatRole.Tool, question.ToString());

        var response = await _chatClient.GetResponseAsync(question.ToString(), 
            cancellationToken: cancellationToken);
        
        return response.Text;
    }
}