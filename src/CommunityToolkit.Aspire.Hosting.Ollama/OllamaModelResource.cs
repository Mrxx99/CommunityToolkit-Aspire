using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Aspire.Hosting.ApplicationModel;

public class OllamaModelResource(string name, string modelName, OllamaResource parent) : Resource(name), IResourceWithParent<OllamaResource>, IResourceWithConnectionString
{
    public OllamaResource Parent { get; } = ThrowIfNull(parent);

    /// <summary>
    /// Gets the connection string expression for the Ollama model.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression => ReferenceExpression.Create($"{Parent}?Model={ModelName}");

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string ModelName { get; } = ThrowIfNull(modelName);

    private static T ThrowIfNull<T>([NotNull] T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    => argument ?? throw new ArgumentNullException(paramName);
}
