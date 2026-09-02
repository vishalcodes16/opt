using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using VishalXOpt.Core.Models;
using VishalXOpt.Core.Services;
using VishalXOpt.UI;
using VishalXOpt.UI.Controls;
using VishalXOpt.UI.ViewModels;

namespace VishalXOpt.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel vm = new();
    private readonly ColumnDefinition sidebar;
    private readonly Brush panel = new SolidColorBrush(Color.FromRgb(18, 28, 54));
    private readonly Brush border = new SolidColorBrush(Color.FromRgb(45, 83, 138));
    private readonly Brush blue = new SolidColorBrush(Color.FromRgb(53, 167, 255));

    public MainWindow()
    {
        InitializeComponent();
        sidebar = ((Grid)Content).ColumnDefinitions[0];
        DataContext = vm;

        SearchBox.KeyDown += SearchBox_KeyDown;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.SelectedPage))
            {
                RenderPage(vm.SelectedPage);
            }
            else if (e.PropertyName == nameof(vm.IsSidebarCollapsed))
            {
                sidebar.Width = vm.IsSidebarCollapsed ? new GridLength(72) : new GridLength(250);
            }
        };

        RenderPage(vm.SelectedPage);
    }

    private void SearchBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        var query = SearchBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(query))
            return;

        var hit = vm.Pages.FirstOrDefault(x => x.Contains(query, StringComparison.OrdinalIgnoreCase));
        if (hit is not null)
        {
            vm.Navigate(hit);
            e.Handled = true;
        }
        else
        {
            MessageBox.Show(
                "No matching module found.",
                "Vishal X Opt • Search",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    private Border Card(UIElement child) => new()
    {
        Background = panel,
        BorderBrush = border,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(18),
        Padding = new Thickness(18),
        Margin = new Thickness(6),
        Child = child
    };

    private TextBlock H(string text, double size = 20) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = FontWeights.SemiBold
    };

    private Button B(string text, ICommand? command = null) => new()
    {
        Content = text,
        Command = command,
        MinWidth = 125,
        Height = 38,
        Margin = new Thickness(4)
    };

    private TextBlock T(string text) => new()
    {
        Text = text,
        Opacity = 0.66,
        TextWrapping = TextWrapping.Wrap
    };

    private StackPanel ActionCard(string title, string value, string description, Action action)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = title, Opacity = 0.55 });
        stack.Children.Add(H(value, 19));
        stack.Children.Add(T(description));

        var button = B("Inspect / Apply");
        button.Click += (_, _) =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                vm.StatusMessage = $"Operation failed: {ex.Message}";
            }
        };

        stack.Children.Add(button);
        return stack;
    }

    private void RenderPage(string page)
    {
        PageHost.Content = page switch
        {
            "Home" => BuildHome(),
            "Optimizer" => BuildOptimizer(),
            "Gaming / FPS" => BuildGaming(),
            "Power Management" => BuildPower(),
            "Cleanup" => BuildCleanup(),
            "Devices" => BuildDevices(false),
            "MSI Utility" => BuildDevices(true),
            "Interrupts" => BuildInterrupts(),
            "Autoruns" => BuildAutoruns(),
            "Tasks" => BuildTasks(),
            "Network Adapters" => BuildNetwork(),
            "Components" => BuildComponents(),
            "Tweaks" => BuildTweaks(),
            "Windows Tweaker" => BuildTweaker(),
            "Privacy" => BuildPrivacy(),
            "Debloat" => BuildDebloat(),
            "WinUtil" => BuildWinUtil(),
            "Tools" => BuildTools(),
            "Settings" => BuildSettings(),
            "Deprecated / Advanced" => BuildAdvanced(),
            _ => BuildGeneric(page)
        };
    }

    private FrameworkElement BuildHome()
    {
        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(270) });
        root.RowDefinitions.Add(new RowDefinition());

        var hero = new Grid();
        hero.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.65, GridUnitType.Star) });
        hero.ColumnDefinitions.Add(new ColumnDefinition());

        var left = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        left.Children.Add(new TextBlock
        {
            Text = "VISHAL X OPT",
            Foreground = blue,
            FontSize = 16,
            FontWeight = FontWeights.Bold
        });
        left.Children.Add(new TextBlock
        {
            Text = "3D GAMING SYSTEM OPTIMIZER",
            FontSize = 30,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 6, 0, 5)
        });
        left.Children.Add(T("Hardware-aware • Version-aware • Backup-first • Measurable • Reversible"));

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 18, 0, 0)
        };
        buttons.Children.Add(B("Scan System", vm.ScanCommand));
        buttons.Children.Add(B("Optimize Now", vm.OptimizeCommand));
        buttons.Children.Add(B("Gaming Mode", vm.GamingCommand));
        buttons.Children.Add(B("Restore", vm.RestoreCommand));
        left.Children.Add(buttons);

        hero.Children.Add(left);

        var core = new SystemCore3D { Margin = new Thickness(10) };
        Grid.SetColumn(core, 1);
        hero.Children.Add(core);

        root.Children.Add(Card(hero));

        var cards = new UniformGrid { Columns = 4 };
        Grid.SetRow(cards, 1);
        cards.Children.Add(Card(Metric("CPU", $"{vm.CpuUsagePercent:0}%", $"Temp {vm.CpuTempText}")));
        cards.Children.Add(Card(Metric("GPU", vm.GpuUsageText, $"Temp {vm.GpuTempText}")));
        cards.Children.Add(Card(Metric("RAM", vm.RamText, "Live system memory")));
        cards.Children.Add(Card(Metric("NETWORK", vm.NetworkText, $"Power: {vm.PowerPlan}")));
        cards.Children.Add(Card(Metric("DISK", $"{vm.DiskUsagePercent:0}%", "System drive")));
        cards.Children.Add(Card(Metric("SCORE", $"{vm.OptimizationScore}%", "Explainable score")));
        cards.Children.Add(Card(Metric("ADMIN", vm.AdminText, "Elevation state")));
        cards.Children.Add(Card(Metric("SECURE BOOT", vm.Snapshot?.SecureBoot ?? "Unknown", vm.Snapshot?.Build ?? "")));

        root.Children.Add(cards);
        return root;
    }

    private StackPanel Metric(string name, string value, string hint)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = name, Opacity = 0.55 });
        stack.Children.Add(new TextBlock
        {
            Text = value,
            FontSize = 26,
            FontWeight = FontWeights.Bold,
            Foreground = blue,
            Margin = new Thickness(0, 10, 0, 3)
        });
        stack.Children.Add(new TextBlock { Text = hint, Opacity = 0.55 });
        return stack;
    }

    private FrameworkElement BuildOptimizer()
    {
        var stack = new StackPanel();
        stack.Children.Add(H("Optimization Profiles", 26));
        stack.Children.Add(T("Profiles are hardware-aware and preview-oriented. Current supported state is inspected before changes are applied."));

        foreach (var profile in vm.Profiles)
        {
            var row = new Grid { Margin = new Thickness(0, 6, 0, 0) };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var details = new StackPanel();
            details.Children.Add(H(profile, 18));
            details.Children.Add(T(profile switch
            {
                "Default" => "Conservative settings",
                "Optimal" => "Balanced performance and usability",
                "Maximum" => "Aggressive performance-oriented settings",
                "Gaming / FPS Maximum" => "Gaming-focused measurable settings",
                _ => "Profile"
            }));

            row.Children.Add(details);

            var actions = new StackPanel { Orientation = Orientation.Horizontal };
            var preview = B("Preview");
            preview.Click += (_, _) => PreviewProfile(profile);
            actions.Children.Add(preview);

            var apply = B("Apply");
            apply.Margin = new Thickness(8, 0, 0, 0);
            apply.Click += (_, _) => vm.ApplyProfileCommand.Execute(profile);
            actions.Children.Add(apply);
            Grid.SetColumn(actions, 1);
            row.Children.Add(actions);

            stack.Children.Add(Card(row));
        }

        var detection = new StackPanel();
        detection.Children.Add(H("Current detection", 18));
        detection.Children.Add(new TextBlock
        {
            Text =
                $"Power: {vm.PowerPlan}\n" +
                $"CPU: {vm.CpuUsagePercent:0}%\n" +
                $"RAM: {vm.RamUsedPercent:0}%\n" +
                $"Disk: {vm.DiskUsagePercent:0}%\n" +
                $"Latency: {vm.NetworkText}",
            Margin = new Thickness(0, 8, 0, 0)
        });
        stack.Children.Add(Card(detection));

        return Scroll(stack);
    }

    private void PreviewProfile(string profile)
    {
        var preview = AppServices.Optimizer.Preview(profile);
        var operations = preview.Operations.Count == 0
            ? "No system settings will be changed."
            : string.Join("\n", preview.Operations.Select(x =>
                $"• {x.Name}: {x.CurrentValue} → {x.RecommendedValue}" +
                (x.RequiresAdmin ? " (administrator required)" : "") +
                (x.RequiresRestart ? " (restart required)" : "")));
        var message = $"{preview.Profile}\n\n{preview.Description}\n\n{operations}\n\n" +
                      "Successful changes are backed up and verified. No guaranteed FPS increase is claimed.";

        MessageBox.Show(
            message,
            "Vishal X Opt • Preview",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        vm.StatusMessage = $"Previewed profile: {profile}.";
    }

    private FrameworkElement BuildGaming()
    {
        var root = new ScrollViewer();
        var stack = new StackPanel();

        var hero = new Grid { Height = 230, Margin = new Thickness(6) };
        hero.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.35, GridUnitType.Star) });
        hero.ColumnDefinitions.Add(new ColumnDefinition());
        var heroText = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(22) };
        heroText.Children.Add(new TextBlock { Text = "GAMING PERFORMANCE LAB", Foreground = blue, FontWeight = FontWeights.Bold, FontSize = 13 });
        heroText.Children.Add(H("Tune. Measure. Verify.", 30));
        heroText.Children.Add(T("Apply only supported Windows settings, then use actual frame-time telemetry to compare before and after. No FPS increase is guaranteed."));
        var processName = new TextBox
        {
            Width = 250,
            Height = 32,
            Margin = new Thickness(4),
            ToolTip = "Game process name, for example: game.exe"
        };
        processName.SetValue(TextBox.TextProperty, "game.exe");
        heroText.Children.Add(processName);
        var capture = B("Capture Frame Time");
        capture.Click += (_, _) => vm.CaptureFpsCommand.Execute(processName.Text.Trim());
        heroText.Children.Add(capture);
        hero.Children.Add(heroText);
        var visual = new SystemCore3D { Margin = new Thickness(12) };
        Grid.SetColumn(visual, 1);
        hero.Children.Add(visual);
        stack.Children.Add(Card(hero));

        stack.Children.Add(H("Gaming / FPS", 26));
        stack.Children.Add(T("Controls are preview-first and hardware-aware. Apply a setting, then measure frame time or system telemetry to validate its effect."));

        var grid = new UniformGrid { Columns = 2 };

        grid.Children.Add(Card(ActionCard(
            "POWER",
            $"Current: {vm.PowerPlan}",
            "Activate a performance-oriented Windows power plan.",
            () => vm.ApplyHighPerformance())));

        grid.Children.Add(Card(ActionCard(
            "GAME MODE",
            "Windows Game Mode",
            "Enable the Windows Game Mode setting.",
            () => vm.ApplyGameModeCommand.Execute(null))));

        grid.Children.Add(Card(ActionCard(
            "GAME DVR",
            "Background recording",
            "Disable Windows background Game DVR recording.",
            () => vm.DisableGameDvrCommand.Execute(null))));

        grid.Children.Add(Card(ActionCard(
            "HAGS",
            "Hardware scheduling",
            "Enable Hardware-accelerated GPU scheduling where Windows exposes it.",
            () => vm.EnableHagsCommand.Execute(null))));

        grid.Children.Add(Card(ActionCard(
            "VISUAL EFFECTS",
            "Best Performance",
            "Apply Windows visual-effects performance settings.",
            () => vm.BestPerformanceVisualsCommand.Execute(null))));

        grid.Children.Add(Card(ActionCard(
            "INPUT",
            "Mouse acceleration",
            "Apply the configured pointer acceleration tweak.",
            () => vm.ApplyMouseAccelerationCommand.Execute(null))));

        grid.Children.Add(Card(ActionCard(
            "NETWORK",
            "Low Latency",
            "Scan actual network adapters and their exposed properties.",
            () => vm.ScanNetworkCommand.Execute(null))));

        grid.Children.Add(Card(ActionCard(
            "MEASURE",
            "Before / After",
            "Capture CPU, RAM, disk and network measurements.",
            MeasureNow)));

        stack.Children.Add(grid);
        root.Content = stack;
        return root;
    }

    private void MeasureNow()
    {
        var samples = AppServices.Performance.Sample(3, 300);
        if (samples.Count == 0)
        {
            vm.StatusMessage = "No performance samples were captured.";
            return;
        }

        var averageCpu = samples.Average(x => x.CpuPercent);
        var last = samples[^1];

        MessageBox.Show(
            $"Measured samples: {samples.Count}\n" +
            $"Average CPU: {averageCpu:0.0}%\n" +
            $"Latest RAM: {last.RamPercent:0.0}%\n" +
            $"Latest Disk: {last.DiskPercent:0.0}%\n" +
            $"Ping: {(last.PingMs < 0 ? "N/A" : $"{last.PingMs:0} ms")}",
            "Performance Measurement",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        vm.StatusMessage = "Performance measurement captured.";
    }

    private FrameworkElement BuildPower()
    {
        var stack = new StackPanel();
        stack.Children.Add(H("Power Management", 26));
        stack.Children.Add(T($"Current power plan: {vm.PowerPlan}."));

        var grid = new UniformGrid { Columns = 2 };
        grid.Children.Add(Card(ActionCard("BALANCED", "Balanced", "Normal daily-use profile.", vm.ApplyBalancedPower)));
        grid.Children.Add(Card(ActionCard("HIGH PERFORMANCE", "High Performance", "Performance-focused Windows power plan.", vm.ApplyHighPerformance)));
        grid.Children.Add(Card(ActionCard("ULTIMATE", "Ultimate Performance", "Create and activate Ultimate Performance.", () => vm.ApplyUltimatePerformanceCommand.Execute(null))));
        grid.Children.Add(Card(ActionCard("RESTORE POINT", "Windows Restore Point", "Create a Windows restore point.", () => vm.CreateRestorePointCommand.Execute(null))));

        stack.Children.Add(grid);
        return Scroll(stack);
    }

    private FrameworkElement BuildCleanup()
    {
        var dock = new DockPanel();
        var top = new StackPanel { Orientation = Orientation.Horizontal };
        top.Children.Add(B("Scan", vm.ScanCleanupCommand));
        top.Children.Add(B("Clean Safe", vm.CleanSafeCommand));
        DockPanel.SetDock(top, Dock.Top);
        dock.Children.Add(top);

        var list = new ListView { ItemsSource = vm.CleanupItems };
        var view = new GridView();
        view.Columns.Add(new GridViewColumn { Header = "Name", DisplayMemberBinding = new Binding("Name") });
        view.Columns.Add(new GridViewColumn { Header = "Category", DisplayMemberBinding = new Binding("Category") });
        view.Columns.Add(new GridViewColumn { Header = "Size", DisplayMemberBinding = new Binding("SizeBytes") });
        view.Columns.Add(new GridViewColumn { Header = "Safe", DisplayMemberBinding = new Binding("Safe") });
        view.Columns.Add(new GridViewColumn { Header = "Path", DisplayMemberBinding = new Binding("Path") });
        list.View = view;

        dock.Children.Add(list);
        return Card(dock);
    }

    private FrameworkElement BuildDevices(bool msi)
    {
        var dock = new DockPanel();

        var top = new StackPanel { Orientation = Orientation.Horizontal };
        top.Children.Add(B("Refresh", vm.ScanDevicesCommand));
        if (msi)
            top.Children.Add(B("Apply MSI Toggle", vm.ApplyMsiSelectedCommand));

        DockPanel.SetDock(top, Dock.Top);
        dock.Children.Add(top);

        var list = new ListView
        {
            ItemsSource = vm.Devices,
            DisplayMemberPath = "Name"
        };
        list.SelectionChanged += (_, _) =>
        {
            if (list.SelectedItem is DeviceInfo device)
                vm.SelectedDeviceName = device.Name;
        };

        dock.Children.Add(list);

        var info = new StackPanel { Margin = new Thickness(10) };
        info.Children.Add(H(msi ? "MSI Utility" : "Devices", 22));
        info.Children.Add(T("Real Windows device enumeration. PNP/PCI values show Not available when the driver does not expose them."));
        info.Children.Add(T("Select a device from the list to inspect/apply available MSI settings."));
        dock.Children.Add(info);

        var visual = new SystemCore3D
        {
            Width = 270,
            Height = 210,
            Margin = new Thickness(10)
        };
        DockPanel.SetDock(visual, Dock.Right);
        dock.Children.Add(visual);

        return Card(dock);
    }

    private FrameworkElement BuildInterrupts()
    {
        var stack = new StackPanel();
        stack.Children.Add(H("Interrupts / Affinity", 26));
        stack.Children.Add(T("Logical processor tiles are generated dynamically from the actual processor count."));

        var tiles = new WrapPanel();
        var checks = new List<CheckBox>();

        for (var i = 0; i < Environment.ProcessorCount; i++)
        {
            var check = new CheckBox
            {
                Content = $"CPU {i}",
                Margin = new Thickness(5),
                Padding = new Thickness(7),
                IsChecked = true
            };
            checks.Add(check);
            tiles.Children.Add(check);
        }

        stack.Children.Add(Card(tiles));

        var validate = B("Validate Selected Mask");
        validate.Click += (_, _) =>
        {
            ulong mask = 0;
            var selected = 0;

            for (var i = 0; i < checks.Count; i++)
            {
                if (checks[i].IsChecked == true)
                {
                    selected++;
                    if (i < 64)
                        mask |= 1UL << i;
                }
            }

            vm.StatusMessage =
                $"Selected {selected} logical processors. Affinity mask: 0x{mask:X}.";
        };
        stack.Children.Add(validate);

        stack.Children.Add(Card(new StackPanel
        {
            Children =
            {
                H("MSI / Routing", 18),
                T("The MSI Utility integrates device MSI mode and interrupt-related registry properties."),
                B("Open MSI Utility", new RelayCommand(() => vm.Navigate("MSI Utility")))
            }
        }));

        return Scroll(stack);
    }

    private FrameworkElement BuildAutoruns()
    {
        var list = new ListView { ItemsSource = vm.Autoruns };
        var view = new GridView();
        view.Columns.Add(new GridViewColumn { Header = "Name", DisplayMemberBinding = new Binding("Name") });
        view.Columns.Add(new GridViewColumn { Header = "Classification", DisplayMemberBinding = new Binding("Classification") });
        view.Columns.Add(new GridViewColumn { Header = "Location", DisplayMemberBinding = new Binding("Location") });
        view.Columns.Add(new GridViewColumn { Header = "Command", DisplayMemberBinding = new Binding("Command") });
        view.Columns.Add(new GridViewColumn { Header = "Enabled", DisplayMemberBinding = new Binding("Enabled") });
        list.View = view;

        list.SelectionChanged += (_, _) =>
        {
            if (list.SelectedItem is AutorunEntry entry)
                vm.SelectedAutorunName = entry.Name;
        };

        var dock = new DockPanel();
        var top = new StackPanel { Orientation = Orientation.Horizontal };
        top.Children.Add(B("Refresh", vm.ScanAutorunsCommand));
        top.Children.Add(B("Disable Selected", vm.DisableSelectedAutorunCommand));
        top.Children.Add(B("Restore Selected", vm.RestoreSelectedAutorunCommand));
        DockPanel.SetDock(top, Dock.Top);
        dock.Children.Add(top);
        dock.Children.Add(list);

        return Card(dock);
    }

    private FrameworkElement BuildTasks()
    {
        var list = new ListView { ItemsSource = vm.Tasks };
        var view = new GridView();
        view.Columns.Add(new GridViewColumn { Header = "Task", DisplayMemberBinding = new Binding("Name") });
        view.Columns.Add(new GridViewColumn { Header = "Status", DisplayMemberBinding = new Binding("Status") });
        view.Columns.Add(new GridViewColumn { Header = "Author", DisplayMemberBinding = new Binding("Author") });
        list.View = view;

        list.SelectionChanged += (_, _) =>
        {
            if (list.SelectedItem is TaskInfo task)
                vm.SelectedTaskName = task.Name;
        };

        var dock = new DockPanel();
        var top = new StackPanel { Orientation = Orientation.Horizontal };
        top.Children.Add(B("Refresh", vm.ScanTasksCommand));
        top.Children.Add(B("Disable", vm.DisableSelectedTaskCommand));
        top.Children.Add(B("Enable", vm.EnableSelectedTaskCommand));
        DockPanel.SetDock(top, Dock.Top);
        dock.Children.Add(top);
        dock.Children.Add(list);

        return Card(dock);
    }

    private FrameworkElement BuildNetwork()
    {
        var list = new ListView { ItemsSource = vm.Adapters };
        var view = new GridView();
        view.Columns.Add(new GridViewColumn { Header = "Name", DisplayMemberBinding = new Binding("Name") });
        view.Columns.Add(new GridViewColumn { Header = "Status", DisplayMemberBinding = new Binding("Status") });
        view.Columns.Add(new GridViewColumn { Header = "Speed", DisplayMemberBinding = new Binding("Speed") });
        view.Columns.Add(new GridViewColumn { Header = "MAC", DisplayMemberBinding = new Binding("Mac") });
        view.Columns.Add(new GridViewColumn { Header = "IP", DisplayMemberBinding = new Binding("Ips") });
        list.View = view;

        list.SelectionChanged += (_, _) =>
        {
            if (list.SelectedItem is AdapterInfo adapter)
                vm.SelectedAdapterName = adapter.Name;
        };

        var dock = new DockPanel();
        var top = new StackPanel { Orientation = Orientation.Horizontal };
        top.Children.Add(B("Refresh", vm.ScanNetworkCommand));
        top.Children.Add(B("Inspect Selected", vm.InspectSelectedAdapterCommand));
        top.Children.Add(B("Latency Test", new RelayCommand(vm.TestInternet)));
        DockPanel.SetDock(top, Dock.Top);
        dock.Children.Add(top);
        dock.Children.Add(list);

        dock.Children.Add(Card(T("Adapter-specific options are shown only when the current driver exposes the property.")));
        return Card(dock);
    }

    private FrameworkElement BuildComponents()
    {
        var list = new ListView { ItemsSource = vm.Components };
        var view = new GridView();
        view.Columns.Add(new GridViewColumn { Header = "Feature", DisplayMemberBinding = new Binding("Name") });
        view.Columns.Add(new GridViewColumn { Header = "State", DisplayMemberBinding = new Binding("State") });
        list.View = view;

        list.SelectionChanged += (_, _) =>
        {
            if (list.SelectedItem is ComponentInfo component)
                vm.SelectedComponentName = component.Name;
        };

        var dock = new DockPanel();
        var top = new StackPanel { Orientation = Orientation.Horizontal };
        top.Children.Add(B("Scan", vm.ScanComponentsCommand));
        top.Children.Add(B("Enable", vm.EnableSelectedComponentCommand));
        top.Children.Add(B("Disable", vm.DisableSelectedComponentCommand));
        DockPanel.SetDock(top, Dock.Top);
        dock.Children.Add(top);
        dock.Children.Add(list);
        dock.Children.Add(Card(T("DISM-backed Windows feature inventory.")));

        return Card(dock);
    }

    private FrameworkElement BuildTweaks()
    {
        return BuildTweakList("Data-Driven Tweaks", vm.TweakStates);
    }

    private FrameworkElement BuildTweaker()
    {
        return BuildTweakList(
            "Windows Tweaker",
            vm.TweakStates,
            "Basic, Security, Customization, Power management, Debloat, Cleanup, Privacy, Tweaks, Autoruns, Interrupts, Devices, Network adapters, Tasks, Components, Deprecated");
    }

    private FrameworkElement BuildTweakList(
        string title,
        ObservableCollection<TweakState> states,
        string? subtitle = null)
    {
        var stack = new StackPanel();
        stack.Children.Add(H(title, 26));
        stack.Children.Add(T(subtitle ?? "Each setting exposes current value, recommendation, risk and support status."));

        var list = new ListView
        {
            ItemsSource = states,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var view = new GridView();
        view.Columns.Add(new GridViewColumn { Header = "Name", DisplayMemberBinding = new Binding("Definition.Name") });
        view.Columns.Add(new GridViewColumn { Header = "Current", DisplayMemberBinding = new Binding("CurrentValue") });
        view.Columns.Add(new GridViewColumn { Header = "Recommended", DisplayMemberBinding = new Binding("RecommendedValue") });
        view.Columns.Add(new GridViewColumn { Header = "Risk", DisplayMemberBinding = new Binding("Definition.Risk") });
        view.Columns.Add(new GridViewColumn { Header = "Supported", DisplayMemberBinding = new Binding("Supported") });
        list.View = view;

        var actionPanel = new StackPanel { Orientation = Orientation.Horizontal };
        var apply = B("Apply Selected");
        apply.Click += async (_, _) =>
        {
            if (list.SelectedItem is TweakState state)
                await vm.ApplyTweakAsync(state);
            else
                vm.StatusMessage = "Select a tweak first.";
        };

        actionPanel.Children.Add(apply);
        actionPanel.Children.Add(B("Detect Current State", vm.DetectTweaksCommand));

        stack.Children.Add(list);
        stack.Children.Add(actionPanel);

        return Card(stack);
    }

    private FrameworkElement BuildPrivacy()
    {
        var items = new[]
        {
            ".NET telemetry", "PowerShell telemetry", "Developer / CLI telemetry",
            "Telemetry services", "Compatibility Appraiser", "CEIP",
            "Windows Error Reporting", "Voice activation", "Location",
            "Windows Search data collection", "Targeted advertising", "Cloud sync",
            "Cloud speech recognition", "Feedback / diagnostics", "Text and handwriting",
            "Sensors", "Inventory collection", "Language list access",
            "Steps Recorder", "Activity feed", "Location permissions",
            "Account information", "Motion data", "Phone", "Contacts",
            "Calendar", "Call history", "Email", "Tasks", "Messaging",
            "Radio", "Bluetooth devices", "Documents", "Pictures",
            "Videos", "Other file systems", "Device synchronization"
        };

        var stack = new StackPanel();
        stack.Children.Add(H("Privacy / Telemetry", 26));
        stack.Children.Add(T("Privacy controls are shown separately from performance tuning so each setting can be reviewed independently."));
        stack.Children.Add(new ListBox { ItemsSource = items, Height = 480 });

        return Scroll(stack);
    }

    private FrameworkElement BuildDebloat()
    {
        var list = new ListView
        {
            ItemsSource = AppServices.Debloat.GetPackages()
        };

        var view = new GridView();
        view.Columns.Add(new GridViewColumn { Header = "Name", DisplayMemberBinding = new Binding("Name") });
        view.Columns.Add(new GridViewColumn { Header = "Publisher", DisplayMemberBinding = new Binding("Publisher") });
        view.Columns.Add(new GridViewColumn { Header = "Version", DisplayMemberBinding = new Binding("Version") });
        list.View = view;

        list.SelectionChanged += (_, _) =>
        {
            if (list.SelectedItem is not AppPackageInfo package)
                return;

            if (MessageBox.Show(
                    $"Remove package?\n\n{package.Name}\n{package.FullName}",
                    "Debloat • Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var ok = AppServices.Debloat.Remove(package.FullName);
                vm.StatusMessage = ok
                    ? $"Removed {package.Name}."
                    : $"Unable to remove {package.Name}.";
            }
        };

        var stack = new StackPanel();
        stack.Children.Add(H("Debloat / UWP Applications", 26));
        stack.Children.Add(T("Packages are enumerated from the current PC. Removal is explicit and confirmed."));
        stack.Children.Add(list);

        return Scroll(stack);
    }

    private FrameworkElement BuildWinUtil()
    {
        var inner = new StackPanel();
        inner.Children.Add(H("Remote PowerShell command", 18));
        inner.Children.Add(new TextBox
        {
            Text = WinUtilService.Command,
            IsReadOnly = true,
            Margin = new Thickness(0, 8, 0, 8)
        });
        inner.Children.Add(T("Inspect the official source before running a remote script."));

        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        buttons.Children.Add(B("Open Official Source", vm.OpenWinUtilSourceCommand));
        buttons.Children.Add(B("Run (confirmation required)", vm.RunWinUtilConfirmedCommand));
        inner.Children.Add(buttons);

        return Card(inner);
    }

    private sealed record ToolDefinition(string Name, string Description, Action Action);

    private FrameworkElement BuildTools()
    {
        var tools = new[]
        {
            new ToolDefinition(
                "StoreX",
                "Open Microsoft Store",
                () => TryOpen("ms-windows-store:")),
            new ToolDefinition(
                "GameModeX",
                "Open gaming settings",
                () => TryOpen("ms-settings:gaming-gamemode")),
            new ToolDefinition(
                "ProcessX",
                "Inspect running processes",
                () => vm.ScanProcesses()),
            new ToolDefinition(
                "GodMode",
                "Open God Mode folder",
                OpenGodMode),
            new ToolDefinition(
                "PC Latency Test",
                "Ping common public resolvers",
                () => vm.TestInternet()),
            new ToolDefinition(
                "Internet Test",
                "Ping 1.1.1.1 and 8.8.8.8",
                () => vm.TestInternet()),
            new ToolDefinition(
                "GameReadyX",
                "Open Windows graphics settings",
                () => TryOpen("ms-settings:display-advancedgraphics")),
            new ToolDefinition(
                "Steam",
                "Open Steam if installed",
                () => TryOpen("steam://open/home")),
            new ToolDefinition(
                "Bottleneck",
                "Show detected CPU/GPU/RAM information",
                () => MessageBox.Show(
                    $"CPU threads: {Environment.ProcessorCount}\n" +
                    $"RAM: {vm.Snapshot?.TotalRamBytes / 1024d / 1024d / 1024d:0.0} GB\n" +
                    "GPU: see Devices / Hardware Monitor",
                    "Bottleneck",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information))
        };

        var grid = new UniformGrid { Columns = 3 };

        foreach (var tool in tools)
        {
            var button = B("Open");
            button.Click += (_, _) => ExecuteTool(tool.Action);

            var stack = new StackPanel();
            stack.Children.Add(H(tool.Name, 17));
            stack.Children.Add(T(tool.Description));
            stack.Children.Add(button);

            grid.Children.Add(Card(stack));
        }

        return Scroll(grid);
    }

    private void ExecuteTool(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            vm.StatusMessage = ex.Message;
        }
    }

    private void OpenGodMode()
    {
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var folder = Path.Combine(
            desktop,
            "Vishal X Opt God Mode.{ED7BA470-8E54-465E-825C-99712043E01C}");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        TryOpen(folder);
    }

    private static void TryOpen(string uri)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri)
        {
            UseShellExecute = true
        });
    }

    private FrameworkElement BuildSettings()
    {
        var settings = AppServices.Settings.Load();
        var stack = new StackPanel();

        stack.Children.Add(H("Settings", 26));
        stack.Children.Add(SettingCheck("Reduce Motion", settings.ReduceMotion, value => settings.ReduceMotion = value));
        stack.Children.Add(SettingCheck("Disable 3D", settings.Disable3D, value => settings.Disable3D = value));
        stack.Children.Add(SettingCheck("Disable Transparency", settings.DisableTransparency, value => settings.DisableTransparency = value));
        stack.Children.Add(SettingCheck("Create restore point before optimization", settings.CreateRestorePointBeforeOptimization, value => settings.CreateRestorePointBeforeOptimization = value));
        stack.Children.Add(SettingCheck("Automatic backup", settings.AutomaticBackup, value => settings.AutomaticBackup = value));
        stack.Children.Add(SettingCheck("Confirm dangerous operations", settings.ConfirmDangerousOperations, value => settings.ConfirmDangerousOperations = value));

        var save = B("Save Settings");
        save.Click += (_, _) =>
        {
            AppServices.Settings.Save(settings);
            vm.StatusMessage = "Settings saved.";
        };
        stack.Children.Add(save);

        var restore = new StackPanel();
        restore.Children.Add(H("Backup / Restore", 18));
        restore.Children.Add(T("Restore uses saved registry states recorded by the application."));
        restore.Children.Add(B("Restore Saved Registry States", vm.RestoreCommand));
        stack.Children.Add(Card(restore));

        var logs = new StackPanel();
        logs.Children.Add(H("In-App Logs", 18));
        logs.Children.Add(B("Refresh Log Viewer", vm.RefreshLogsCommand));
        logs.Children.Add(new ListBox
        {
            ItemsSource = vm.Logs,
            Height = 180,
            Margin = new Thickness(0, 8, 0, 0)
        });
        stack.Children.Add(Card(logs));

        return Scroll(stack);
    }

    private CheckBox SettingCheck(string text, bool value, Action<bool> action)
    {
        var check = new CheckBox
        {
            Content = text,
            IsChecked = value,
            Margin = new Thickness(8),
            FontSize = 15
        };

        check.Checked += (_, _) => action(true);
        check.Unchecked += (_, _) => action(false);
        return check;
    }

    private FrameworkElement BuildAdvanced()
    {
        var items = new[]
        {
            ("NetworkThrottlingIndex", "Legacy/advanced network scheduling value", "Inspect before changing; no automatic FPS guarantee."),
            ("TSX", "Processor feature", "Vendor/CPU dependent."),
            ("LargeSystemCache", "Memory cache behavior", "Advanced system setting."),
            ("SystemResponsiveness", "Multimedia scheduling", "Legacy/tuning context dependent."),
            ("MouseDataQueueSize", "Mouse queue size", "Advanced input queue setting."),
            ("KeyboardDataQueueSize", "Keyboard queue size", "Advanced input queue setting."),
            ("Windows Platform Binary Table", "Platform security component", "Do not change without documented requirement.")
        };

        var list = new ListBox();
        foreach (var item in items)
        {
            list.Items.Add(new ListBoxItem
            {
                Content = $"{item.Item1} — {item.Item2}\n{item.Item3}",
                Padding = new Thickness(8)
            });
        }

        var stack = new StackPanel();
        stack.Children.Add(H("Advanced / Deprecated", 26));
        stack.Children.Add(T("Advanced values are shown for inspection and are not presented as guaranteed performance improvements."));
        stack.Children.Add(list);

        return Scroll(stack);
    }

    private FrameworkElement BuildGeneric(string page)
    {
        var stack = new StackPanel();
        stack.Children.Add(H(page, 28));
        stack.Children.Add(T("Use Scan/Refresh actions to obtain real machine state; unsupported values are shown as Not available."));
        stack.Children.Add(B("Scan System", vm.ScanCommand));
        stack.Children.Add(B("Open Logs", vm.RefreshLogsCommand));
        return Card(stack);
    }

    private static ScrollViewer Scroll(UIElement element) => new()
    {
        Content = element,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
    };
}
