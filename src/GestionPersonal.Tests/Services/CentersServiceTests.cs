using GestionPersonal.Tests.Infrastructure;

namespace GestionPersonal.Tests.Services;

public sealed class CentersServiceTests : ApplicationTestBase, IClassFixture<ApplicationFixture>
{
    public CentersServiceTests(ApplicationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task List_ReturnsSeededCenters()
    {
        var result = await Centers.ListAsync();

        AssertSuccess(result);
        Assert.Contains(result.Value, c => c.Id == 1 && c.Name == "Peluquería Sol");
    }

    [Fact]
    public async Task ListByCenter_ReturnsWorkersOfCenter()
    {
        var result = await Workers.ListByCenterAsync(ApplicationFixture.TestCenterId);

        AssertSuccess(result);
        Assert.Equal(3, result.Value.Count);
        Assert.All(result.Value, w => Assert.True(w.Id > 0));
    }

    [Fact]
    public async Task ListByCenter_WithOtherCenter_ReturnsForbidden()
    {
        var result = await Workers.ListByCenterAsync(TestWorkerIds.New());

        AssertFailure(result, "Forbidden");
    }
}
