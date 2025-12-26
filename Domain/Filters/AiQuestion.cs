namespace Domain.Filters;

public record AiQuestion(string Prompt, string Filter = "ESCREVA EM PORTUGUÊS DO BRASIL")
{
    public override string ToString() => $"{Prompt} - (({Filter}))";
}