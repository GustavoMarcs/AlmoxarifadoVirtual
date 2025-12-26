namespace Domain.Abstractions;

public class ErrorMessageBase
{
    public static Error NotExists(string entityName) =>
        new($"{entityName}.NotExists", $"Esse {entityName.ToLower()} não existe");

    public static Error AlreadyExists(string entityName, string entityType, bool isFemaleName)
    {
        if (isFemaleName)
        {
            return new Error($"{entityName}.AlreadyExists", $"A {entityType.ToLower()} '{entityName}' já existe.");
        }

        return new Error($"${entityName}.AlreadyExists", $"O {entityType.ToLower()} '{entityName}' já existe.");
    }

    public static Error Invalid(string entityName) =>
        new($"{entityName}.Invalid", $"Esse {entityName.ToLower()} é inválido");

    public static Error CannotDelete(string entityName, string reason) =>
        new($"{entityName}.CannotDelete", $"Não é possível excluir {entityName.ToLower()} porque {reason}.");
}