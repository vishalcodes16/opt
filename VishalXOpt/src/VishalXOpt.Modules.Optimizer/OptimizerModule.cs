using VishalXOpt.Core.Models;
using VishalXOpt.Core.Services;

namespace VishalXOpt.Modules.Optimizer;

public sealed record OptimizerOperation(
    string TweakId,
    string Name,
    string Description,
    string Risk,
    bool RequiresAdmin,
    bool RequiresRestart,
    bool Supported,
    string CurrentValue,
    string RecommendedValue);

public sealed record OptimizerPreview(
    string Profile,
    string Description,
    IReadOnlyList<OptimizerOperation> Operations,
    bool RequiresAdmin)
{
    public bool HasSupportedOperations => Operations.Any(x => x.Supported);
}

public sealed record OptimizerApplyResult(
    string Profile,
    int Applied,
    int Skipped,
    IReadOnlyList<string> Messages,
    bool RestartRequired)
{
    /// <summary>
    /// A no-op profile is successful because it intentionally leaves the
    /// system unchanged. Profiles with requested operations succeed only when
    /// every operation was applied and verified.
    /// </summary>
    public bool Success => Skipped == 0;
}

/// <summary>
/// Provides explicit, reversible profile plans.  A profile contains only tweaks
/// backed by <see cref="TweakService"/>, which records the previous registry
/// value and verifies the write before reporting success.
/// </summary>
public sealed class OptimizerModule
{
    private readonly TweakService _tweaks;

    private static readonly IReadOnlyDictionary<string, ProfileDefinition> Profiles =
        new Dictionary<string, ProfileDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["Default"] = new(
                "Default",
                "Makes no changes. Use this profile to inspect the current state before choosing an optimization.",
                []),
            ["Optimal"] = new(
                "Optimal",
                "Applies conservative, user-level responsiveness and gaming settings.",
                ["input.mouse-acceleration", "gaming.game-mode", "gaming.game-dvr", "custom.transparency"]),
            ["Maximum"] = new(
                "Maximum",
                "Applies the Optimal profile and requests HAGS when the system supports it and the app is elevated.",
                ["input.mouse-acceleration", "gaming.game-mode", "gaming.game-dvr", "custom.transparency", "gaming.hags"]),
            ["Gaming / FPS Maximum"] = new(
                "Gaming / FPS Maximum",
                "Applies gaming-focused settings. It does not claim or guarantee an FPS increase.",
                ["input.mouse-acceleration", "gaming.game-mode", "gaming.game-dvr", "gaming.hags"])
        };

    public OptimizerModule() : this(new TweakService()) { }

    public OptimizerModule(TweakService tweaks)
    {
        _tweaks = tweaks ?? throw new ArgumentNullException(nameof(tweaks));
    }

    public string Name => "Optimizer";
    public string Status => "Profiles are previewable, backed up, verified, and safety-gated.";
    public IReadOnlyList<string> ProfileNames => Profiles.Keys.ToList();

    public OptimizerPreview Preview(string profile)
    {
        var definition = GetProfile(profile);
        var states = _tweaks.Detect().ToDictionary(x => x.Definition.Id, StringComparer.OrdinalIgnoreCase);
        var operations = new List<OptimizerOperation>();

        foreach (var tweakId in definition.TweakIds)
        {
            if (!states.TryGetValue(tweakId, out var state))
            {
                operations.Add(new OptimizerOperation(tweakId, tweakId, "The requested tweak definition is unavailable.", "UNKNOWN", false, false, false, "Unavailable", "Unavailable"));
                continue;
            }

            var tweak = state.Definition;
            operations.Add(new OptimizerOperation(
                tweak.Id,
                tweak.Name,
                tweak.Description,
                tweak.Risk,
                tweak.RequiresAdmin,
                tweak.RequiresRestart,
                state.Supported,
                state.CurrentValue,
                state.RecommendedValue));
        }

        return new OptimizerPreview(definition.Name, definition.Description, operations, operations.Any(x => x.RequiresAdmin));
    }

    public async Task<OptimizerApplyResult> ApplyAsync(string profile, bool isAdministrator, CancellationToken token = default)
    {
        var preview = Preview(profile);
        if (preview.Operations.Count == 0)
        {
            return new OptimizerApplyResult(preview.Profile, 0, 0, ["Default profile selected; no system settings were changed."], false);
        }

        var states = _tweaks.Detect().ToDictionary(x => x.Definition.Id, StringComparer.OrdinalIgnoreCase);
        var messages = new List<string>();
        var applied = 0;
        var skipped = 0;
        var restartRequired = false;

        foreach (var operation in preview.Operations)
        {
            token.ThrowIfCancellationRequested();
            if (!operation.Supported || !states.TryGetValue(operation.TweakId, out var state))
            {
                skipped++;
                messages.Add($"{operation.Name}: unavailable; no change was made.");
                continue;
            }

            if (operation.RequiresAdmin && !isAdministrator)
            {
                skipped++;
                messages.Add($"{operation.Name}: requires administrator privileges; no change was made.");
                continue;
            }

            var result = await _tweaks.ApplyAsync(state.Definition, token);
            messages.Add($"{operation.Name}: {result.Message}");
            if (result.Success)
            {
                applied++;
                restartRequired |= result.RestartRequired;
            }
            else
            {
                skipped++;
            }
        }

        return new OptimizerApplyResult(preview.Profile, applied, skipped, messages, restartRequired);
    }

    private static ProfileDefinition GetProfile(string profile)
    {
        if (string.IsNullOrWhiteSpace(profile) || !Profiles.TryGetValue(profile, out var definition))
        {
            throw new ArgumentException("Unknown optimizer profile.", nameof(profile));
        }

        return definition;
    }

    private sealed record ProfileDefinition(string Name, string Description, IReadOnlyList<string> TweakIds);
}