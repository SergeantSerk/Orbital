using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Orbital.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Universe? Universe { get; set; }
        private IEnumerable<Body>? Bodies { get; set; }

        private Thread UniverseThread { get; set; }
        private bool Running { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (UniverseThread != null)
            {
                return;
            }

            // https://nssdc.gsfc.nasa.gov/planetary/planetfact.html
            Bodies = new List<Body>
            {
                new Body(
                    metadata: new Metadata("Sun", Color.Yellow),
                    position: Vector3.Zero,
                    velocity: Vector3.Zero,
                    mass: 1988500e24,
                    radius: 695700e3),
                new Body(
                    metadata: new Metadata("Mercury", Color.SlateGray),
                    position: new Vector3(57.909e9, 0, 0),
                    velocity: new Vector3(0, 47.36e3, 0),
                    mass: 0.33010e24,
                    radius: 2439.7e3),
                new Body(
                    metadata: new Metadata("Venus", Color.LightYellow),
                    position: new Vector3(108.21e9, 0, 0),
                    velocity: new Vector3(0, 35.02e3, 0),
                    mass: 4.8673e24,
                    radius: 6051.8e3),
                new Body(
                    metadata: new Metadata("Earth", Color.Cyan),
                    position: new Vector3(149.598e9, 0, 0),
                    velocity: new Vector3(0, 29.78e3, 0),
                    mass: 5.9722e24,
                    radius: 6371.0e3),
                new Body(
                    metadata: new Metadata("Mars", Color.Red),
                    position: new Vector3(227.956e9, 0, 0),
                    velocity: new Vector3(0, 24.07e3, 0),
                    mass: 0.64169e24,
                    radius: 3389.5e3),
                new Body(
                    metadata: new Metadata("Jupiter", Color.OrangeRed),
                    position: new Vector3(778.479e9, 0, 0),
                    velocity: new Vector3(0, 13.06e3, 0),
                    mass: 1898.13e24,
                    radius: 69911.0e3),
                new Body(
                    metadata: new Metadata("Saturn", Color.SandyBrown),
                    position: new Vector3(1432.041e9, 0, 0),
                    velocity: new Vector3(0, 9.68e3, 0),
                    mass: 568.32e24,
                    radius: 58232.0e3),
                new Body(
                    metadata: new Metadata("Uranus", Color.LightBlue),
                    position: new Vector3(2867.043e9, 0, 0),
                    velocity: new Vector3(0, 6.8e3, 0),
                    mass: 86.811e24,
                    radius: 25362.0e3),
                new Body(
                    metadata: new Metadata("Neptune", Color.Blue),
                    position: new Vector3(4514.953e9, 0, 0),
                    velocity: new Vector3(0, 5.43e3, 0),
                    mass: 102.409e24,
                    radius: 24622.0e3),
                new Body(
                    metadata: new Metadata("Pluto", Color.WhiteSmoke),
                    position: new Vector3(5869.656e9, 0, 0),
                    velocity: new Vector3(0, 4.67e3, 0),
                    mass: 0.01303e24,
                    radius: 1188.0e3)
            };

            // Maximum radius with padding
            double zoom = 0.25;
            double viewport = Bodies
                .Select(_ => _.Position.Magnitude)
                .Max() * zoom;
            // Add extra x% of padding space
            viewport += viewport * 0.01;
            var viewportVector = new Vector3(viewport);

            float scale = 0.1f;

            double minEllipseRadius = 1f;
            double minBodyRadius = Bodies
                .Select(_ => _.Radius)
                .Min();

            Running = true;
            bool infinite = true;
            double T_0 = 0;
            double T = T_0;
            double T_End = 86400 * 365 * 10; // approximately a decade in seconds
            var universe = new Universe(604800);

            UniverseThread = new Thread(() =>
            {
                do
                {
                    //int width = 1000;
                    //int height = width;
                    int width = -1, height = -1;
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            height = (int)RenderSize.Height;
                            width = height;
                        });
                    }
                    catch (TaskCanceledException)
                    {
                        Running = false;
                        break;
                    }

                    if (width < 1 || height < 1)
                    {
                        throw new ArgumentException();
                    }

                    using var bitmap = new Bitmap(width, height);
                    using Graphics graphics = Graphics.FromImage(bitmap);
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphics.Clear(Color.Black);

                    foreach (var body in Bodies)
                    {
                        // Viewport relative ratio
                        Vector3 bodyViewportVector = body.Position / viewportVector;

                        // Scale
                        bodyViewportVector.X *= width / 2;
                        bodyViewportVector.Y *= height / 2;

                        // Offset
                        bodyViewportVector.X += width / 2;
                        bodyViewportVector.Y += height / 2;

                        // Centering
                        float ellipseDiameter = (float)(body.Radius * minEllipseRadius / minBodyRadius) * scale;
                        float renderX = (float)bodyViewportVector.X - (ellipseDiameter / 2);
                        float renderY = (float)bodyViewportVector.Y - (ellipseDiameter / 2);

                        // Colouring (if any) TO-DO
                        Brush brush = new SolidBrush(body.Metadata.Color);

                        graphics.FillEllipse(brush, renderX, renderY, ellipseDiameter, ellipseDiameter);
                    }

                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            OrbitalRender.Source = Convert(bitmap);
                        });
                    }
                    catch (TaskCanceledException)
                    {
                        Running = false;
                        break;
                    }

                    // see https://en.wikipedia.org/wiki/N-body_simulation#Example_Simulations (accessed on 29/04/2022)
                    foreach (var bodyA in Bodies)
                    {
                        Vector3 a_g = Vector3.Zero;

                        foreach (var bodyB in Bodies)
                        {
                            if (bodyB != bodyA)
                            {
                                Vector3 radiusVector = bodyA.Position - bodyB.Position;
                                double radiusVectorMag = radiusVector.Magnitude;

                                double acceleration = -Universe.BIG_G * bodyB.Mass / Math.Pow(radiusVectorMag, 2);
                                var accelerationVector = new Vector3(acceleration);

                                var radiusUnitVector = new Vector3(
                                    x: radiusVector.X / radiusVectorMag,
                                    y: radiusVector.Y / radiusVectorMag,
                                    z: radiusVector.Z / radiusVectorMag);

                                a_g += radiusUnitVector * accelerationVector;
                            }
                        }

                        bodyA.Velocity += a_g * universe.UpdateDtVector;
                    }

                    foreach (var body in Bodies)
                    {
                        body.Position += body.Velocity * universe.UpdateDtVector;
                    }

                    T += universe.UpdateDt;
                } while (Running && (T < T_End || infinite));
            });
            UniverseThread.Start();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Running = false;
            UniverseThread.Join();
        }

        public static BitmapImage Convert(Bitmap src)
        {
            var ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
