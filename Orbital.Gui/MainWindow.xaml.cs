using Orbital.Render;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private Simulation Simulation { get; set; }

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

            double tickResolution = 60 * 60 * 4;
            var universe = new Universe(Bodies, tickResolution);
            Simulation = new Simulation(universe);
            var renderer = new BitmapRenderer(Simulation.Universe);

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
                            width = (int)RenderSize.Width;
                        });
                    }
                    catch (TaskCanceledException)
                    {
                        Simulation.Simulating = false;
                        break;
                    }

                    if (width < 1 || height < 1)
                    {
                        throw new ArgumentException();
                    }

                    using var bitmap = renderer.Render(width, height, ViewportOffset.X, ViewportOffset.Y, ViewportZoom);
                    try
                    {
                        Dispatcher.Invoke(() =>
                        {
                            OrbitalRender.Source = Convert(bitmap);
                        });
                    }
                    catch (TaskCanceledException)
                    {
                        Simulation.Simulating = false;
                        break;
                    }

                    Simulation.Universe.Tick();
                } while (Simulation.Simulating && (universe.T < Simulation.TEnd || Simulation.Infinite));
            });
            UniverseThread.Start();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Simulation.Simulating = false;
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

        private Vector2 ViewportOffset { get; set; } = Vector2.Zero;
        private double ViewportZoom { get; set; } = 1.0;

        private Vector2 MouseDownVector { get; set; }
        private Vector2 MouseUpVector { get; set; }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Middle &&
                e.MiddleButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var cursorPos = e.GetPosition(this);
                MouseDownVector = new Vector2(cursorPos.X, cursorPos.Y);
            }
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Middle &&
                e.MiddleButton == System.Windows.Input.MouseButtonState.Released)
            {
                var cursorPos = e.GetPosition(this);
                MouseUpVector = new Vector2(cursorPos.X, cursorPos.Y);
                var mouseDiff = MouseUpVector - MouseDownVector;

                if (mouseDiff.Magnitude == 0)
                {
                    // Mouse middle double click
                    ViewportOffset = Vector2.Zero;
                }
                else
                {
                    ViewportOffset += MouseUpVector - MouseDownVector;
                }
            }
        }

        private void Window_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Speed up zooming scaled
            var zoomDelta = (ViewportZoom / 4) * (e.Delta > 0 ? 1 : -1);
            ViewportZoom = Math.Max(1, ViewportZoom + zoomDelta);
        }
    }
}
