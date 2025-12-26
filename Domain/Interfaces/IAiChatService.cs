using Domain.Filters;

namespace Domain.Interfaces;

public interface IAiChatService
{
     Task<string> GetResponseAsync(
         AiQuestion question,
         CancellationToken cancellationToken);       
}