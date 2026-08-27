using BeeDayLab.Web.Components.Pages;
using Bunit;
using Xunit;

namespace BeeDayLab.Web.Tests;

public sealed class HomePageTests
{
    [Fact]
    public void RendersTheLabBootstrapHeading()
    {
        using var context = new BunitContext();

        var cut = context.Render<Home>();

        Assert.Equal("beeday Frontend Lab", cut.Find("h1").TextContent.Trim());
    }
}
