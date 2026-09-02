using VishalXOpt.Core.Services;
using Xunit;

namespace Tests;

public sealed class FrameTimeServiceTests
{
    [Fact]
    public void ParseCsv_reads_frame_times_from_the_header_selected_column()
    {
        var result = FrameTimeService.ParseCsv(
        [
            "Application,MsBetweenPresents,Other",
            "Game.exe,10,1",
            "Game.exe,20,1",
            "Game.exe,15,1"
        ]);

        Assert.True(result.Available);
        Assert.Equal("PresentMon", result.Source);
        Assert.Equal(15, result.AverageFrameTimeMs, precision: 3);
        Assert.Equal(20, result.OnePercentLowMs, precision: 3);
        Assert.Equal(1000d / 15d, result.Fps, precision: 3);
    }

    [Fact]
    public void ParseCsv_reports_missing_frame_time_column()
    {
        var result = FrameTimeService.ParseCsv(["Application,FrameTime", "Game.exe,10"]);

        Assert.False(result.Available);
        Assert.Contains("MsBetweenPresents", result.Message);
    }
}
