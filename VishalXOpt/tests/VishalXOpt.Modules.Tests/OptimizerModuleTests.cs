using VishalXOpt.Modules.Optimizer;
using Xunit;

namespace Tests;

public sealed class OptimizerModuleTests
{
    [Fact]
    public void Preview_optimal_profile_lists_only_conservative_user_tweaks()
    {
        var module = new OptimizerModule();

        var preview = module.Preview("Optimal");

        Assert.Equal("Optimal", preview.Profile);
        Assert.False(preview.RequiresAdmin);
        Assert.Equal(
            ["input.mouse-acceleration", "gaming.game-mode", "gaming.game-dvr", "custom.transparency"],
            preview.Operations.Select(x => x.TweakId));
    }

    [Fact]
    public void Preview_maximum_profile_marks_hags_as_administrator_only()
    {
        var module = new OptimizerModule();

        var preview = module.Preview("Maximum");

        var hags = Assert.Single(preview.Operations.Where(x => x.TweakId == "gaming.hags"));
        Assert.True(preview.RequiresAdmin);
        Assert.True(hags.RequiresAdmin);
        Assert.True(hags.RequiresRestart);
    }

    [Fact]
    public async Task Apply_default_profile_does_not_change_the_system()
    {
        var module = new OptimizerModule();

        var result = await module.ApplyAsync("Default", isAdministrator: false);

        Assert.Equal(0, result.Applied);
        Assert.Equal(0, result.Skipped);
        Assert.True(result.Success);
        Assert.Contains("no system settings were changed", result.Messages.Single(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Preview_unknown_profile_is_rejected()
    {
        var module = new OptimizerModule();

        Assert.Throws<ArgumentException>(() => module.Preview("Turbo Everything"));
    }
}