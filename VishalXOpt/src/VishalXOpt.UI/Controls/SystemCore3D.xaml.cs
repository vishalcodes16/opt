using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace VishalXOpt.UI.Controls;

public partial class SystemCore3D : UserControl
{
    private readonly AxisAngleRotation3D _outerRotation = new(new Vector3D(0, 1, 0), 0);
    private readonly AxisAngleRotation3D _middleRotation = new(new Vector3D(1, 1, 0), 0);
    private readonly AxisAngleRotation3D _innerRotation = new(new Vector3D(0, 0, 1), 0);
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(30) };

    public SystemCore3D()
    {
        InitializeComponent();
        Build();
        _timer.Tick += OnTick;
        Loaded += (_, _) => _timer.Start();
        Unloaded += (_, _) => _timer.Stop();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _outerRotation.Angle = (_outerRotation.Angle + 0.35) % 360;
        _middleRotation.Angle = (_middleRotation.Angle - 0.6 + 360) % 360;
        _innerRotation.Angle = (_innerRotation.Angle + 1.1) % 360;
    }

    private void Build()
    {
        var group = new Model3DGroup();
        group.Children.Add(new AmbientLight(Color.FromRgb(18, 47, 83)));
        group.Children.Add(new DirectionalLight(Color.FromRgb(108, 203, 255), new Vector3D(-1, -1, -2)));
        group.Children.Add(CreateCoreLayer(2.15, Color.FromRgb(26, 86, 165), _outerRotation));
        group.Children.Add(CreateCoreLayer(1.58, Color.FromRgb(25, 144, 226), _middleRotation));
        group.Children.Add(CreateCoreLayer(0.94, Color.FromRgb(99, 211, 255), _innerRotation));
        CoreModel.Content = group;
    }

    private static GeometryModel3D CreateCoreLayer(double size, Color color, AxisAngleRotation3D rotation)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        var material = new MaterialGroup();
        material.Children.Add(new DiffuseMaterial(brush));
        material.Children.Add(new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(90, color.R, color.G, color.B))));
        return new GeometryModel3D(Meshes.Cube(size), material)
        {
            Transform = new RotateTransform3D(rotation, new Point3D())
        };
    }

    private static class Meshes
    {
        public static MeshGeometry3D Cube(double size)
        {
            var half = size / 2;
            var points = new[]
            {
                new Point3D(-half, -half, -half), new Point3D(half, -half, -half),
                new Point3D(half, half, -half), new Point3D(-half, half, -half),
                new Point3D(-half, -half, half), new Point3D(half, -half, half),
                new Point3D(half, half, half), new Point3D(-half, half, half)
            };
            var triangles = new[] { 0, 1, 2, 0, 2, 3, 4, 6, 5, 4, 7, 6, 0, 4, 5, 0, 5, 1, 2, 6, 7, 2, 7, 3, 0, 3, 7, 0, 7, 4, 1, 5, 6, 1, 6, 2 };
            var mesh = new MeshGeometry3D();
            foreach (var point in points) mesh.Positions.Add(point);
            foreach (var triangle in triangles) mesh.TriangleIndices.Add(triangle);
            return mesh;
        }
    }
}
