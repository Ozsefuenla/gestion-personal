using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class WorkersServiceTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public WorkersServiceTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ListActive_ReturnsSeededWorkers()
    {
        var result = await Workers.ListActiveAsync();

        AssertSuccess(result);
        Assert.Equal(3, result.Value.Count);
    }

    [Fact]
    public async Task ListActive_ReturnsWorkersWithExpectedFields()
    {
        var result = await Workers.ListActiveAsync();

        AssertSuccess(result);
        Assert.All(result.Value, worker =>
        {
            Assert.True(worker.Id > 0);
            Assert.False(string.IsNullOrWhiteSpace(worker.FullName));
        });
    }

    [Fact]
    public async Task ListActive_ReturnsSeededWorkerByName()
    {
        var result = await Workers.ListActiveAsync();

        AssertSuccess(result);
        Assert.Contains(result.Value, worker => worker.FullName == "María García");
        Assert.Contains(result.Value, worker => worker.FullName == "Ana López");
    }
}
