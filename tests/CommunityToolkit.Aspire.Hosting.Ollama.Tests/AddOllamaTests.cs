using Aspire.Hosting;

namespace CommunityToolkit.Aspire.Hosting.Ollama.Tests;

public class AddOllamaTests
{
    [Fact]
    public void VerifyDefaultModel()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddOllama("ollama", port: null).WithDefaultModel("llama3");

        using var app = builder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var resource = Assert.Single(appModel.Resources.OfType<OllamaResource>());

        Assert.Equal("ollama", resource.Name);

        Assert.Equal("llama3", resource.DefaultModel);
    }

    [Fact]
    public void VerifyCustomModel()
    {
        var builder = DistributedApplication.CreateBuilder();
        var ollama = builder.AddOllama("ollama", port: null);
        var model = ollama.AddModel("custom:tag");

        using var app = builder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var ollamaResource = Assert.Single(appModel.Resources.OfType<OllamaResource>());
        var modelResource = Assert.Single(appModel.Resources.OfType<OllamaModelResource>());

        Assert.Equal("ollama", ollamaResource.Name);
        Assert.Contains("custom:tag", ollamaResource.Models);

        Assert.Equal("ollama-custom", modelResource.Name);
        Assert.Equal("custom:tag", modelResource.ModelName);
        Assert.Equal(ollamaResource, modelResource.Parent);
    }

    [Fact]
    public void VerifyDefaultPort()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddOllama("ollama", port: null);

        using var app = builder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var resource = Assert.Single(appModel.Resources.OfType<OllamaResource>());

        var endpoint = Assert.Single(resource.Annotations.OfType<EndpointAnnotation>());

        Assert.Equal(11434, endpoint.TargetPort);
    }

    [Fact]
    public void VerifyCustomPort()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddOllama("ollama", port: 12345);

        using var app = builder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var resource = Assert.Single(appModel.Resources.OfType<OllamaResource>());

        var endpoint = Assert.Single(resource.Annotations.OfType<EndpointAnnotation>());

        Assert.Equal(12345, endpoint.Port);
    }

    [Fact]
    public void CanSetMultpleModels()
    {
        var builder = DistributedApplication.CreateBuilder();
        var ollama = builder.AddOllama("ollama", port: null);

        var llama3 = ollama.AddModel("llama3");
        var phi3 = ollama.AddModel("phi3");

        using var app = builder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var ollamaResource = Assert.Single(appModel.Resources.OfType<OllamaResource>());

        var modelResources = appModel.Resources.OfType<OllamaModelResource>();

        Assert.Equal("ollama", ollamaResource.Name);

        Assert.Contains("llama3", ollamaResource.Models);
        Assert.Contains("phi3", ollamaResource.Models);

        Assert.Collection(modelResources,
        model =>
        {
            Assert.Equal("ollama-llama3", model.Name);
            Assert.Equal("llama3", model.ModelName);
            Assert.Equal(ollamaResource, model.Parent);
        },
        model =>
        {
            Assert.Equal("ollama-phi3", model.Name);
            Assert.Equal("phi3", model.ModelName);
            Assert.Equal(ollamaResource, model.Parent);
        });
    }

    [Fact]
    public void DefaultModelAddedToModelList()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddOllama("ollama", port: null).WithDefaultModel("llama3");

        using var app = builder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var resource = Assert.Single(appModel.Resources.OfType<OllamaResource>());

        Assert.Equal("ollama", resource.Name);

        Assert.Single(resource.Models);
        Assert.Contains("llama3", resource.Models);
    }

    [Fact]
    public void DistributedApplicationBuilderCannotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => DistributedApplication.CreateBuilder().AddOllama(null!, port: null));
    }

    [Fact]
    public void ResourceNameCannotBeOmitted()
    {
        Assert.Throws<ArgumentException>(() => DistributedApplication.CreateBuilder().AddOllama("", port: null));
        Assert.Throws<ArgumentException>(() => DistributedApplication.CreateBuilder().AddOllama(" ", port: null));
        Assert.Throws<ArgumentNullException>(() => DistributedApplication.CreateBuilder().AddOllama(null!, port: null));
    }

    [Fact]
    public void ModelNameCannotBeOmmitted()
    {
        var builder = DistributedApplication.CreateBuilder();
        var ollama = builder.AddOllama("ollama", port: null);

        Assert.Throws<ArgumentException>(() => ollama.AddModel(""));
        Assert.Throws<ArgumentException>(() => ollama.AddModel(" "));
        Assert.Throws<ArgumentNullException>(() => ollama.AddModel(null!));
    }

    [Fact]
    public void DefaultModelCannotBeOmitted()
    {
        var builder = DistributedApplication.CreateBuilder();
        var ollama = builder.AddOllama("ollama", port: null);

        Assert.Throws<ArgumentException>(() => ollama.WithDefaultModel(""));
        Assert.Throws<ArgumentException>(() => ollama.WithDefaultModel(" "));
        Assert.Throws<ArgumentNullException>(() => ollama.WithDefaultModel(null!));
    }

    [Fact]
    public void OpenWebUIConfigured()
    {
        var builder = DistributedApplication.CreateBuilder();
        _ = builder.AddOllama("ollama", port: null).WithOpenWebUI();

        using var app = builder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var resource = Assert.Single(appModel.Resources.OfType<OpenWebUIResource>());

        Assert.Equal("ollama-openwebui", resource.Name);
        Assert.Equal("http", resource.PrimaryEndpoint.EndpointName);
    }
}
