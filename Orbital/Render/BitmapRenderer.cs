using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Orbital.Render
{
    public class BitmapRenderer
    {
        public static class Resources
        {
            [SupportedOSPlatform("windows")]
            public static readonly Brush WhiteSolidBrush = new SolidBrush(Color.White);

            [SupportedOSPlatform("windows")]
            public static readonly Font Arial16Font = new("Arial", 16);
        }

        public Universe Universe { get; }

        public PointF UniverseTimeStringPosition { get; set; } = new PointF(10, 10);
        private double Zoom { get; set; }
        public PointF UniverseZoomStringPosition { get; set; } = new PointF(10, 30);

        public BitmapRenderer(Universe universe)
        {
            Universe = universe;
        }

        [SupportedOSPlatform("windows")]
        public Bitmap Render(int renderWidth, int renderHeight, double offsetX, double offsetY, double zoom)
        {
            Zoom = zoom;

            // Scale to viewport properly without stretching the render
            double scaleRatio = Math.Min(renderWidth, renderHeight);

            // TO-DO: should viewport be the max magnitude of all the bodies?
            double viewport = Universe.Bodies
                .Select(_ => _.Position.Magnitude)
                .Max();
            // Add extra x% of padding space
            // TO-DO: is this necessary for other cases?
            viewport += viewport * 0.01;
            var viewportVector = new Vector3(viewport);

            var bitmap = new Bitmap(renderWidth, renderHeight);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Black);

                var renderLock = new object();
                //foreach (var body in Universe.Bodies)
                Parallel.ForEach(Universe.Bodies, body =>
                {
                    // Viewport relative ratio
                    Vector3 bodyViewportVector = body.Position / viewportVector;

                    // Scale
                    bodyViewportVector.X *= scaleRatio * zoom / 2;
                    bodyViewportVector.Y *= scaleRatio * zoom / 2;

                    // Offset
                    bodyViewportVector.X += (renderWidth + offsetX) / 2;
                    bodyViewportVector.Y += (renderHeight + offsetY) / 2;

                    float renderX = (float)bodyViewportVector.X;
                    float renderY = (float)bodyViewportVector.Y;

                    // TO-DO: Colouring (if any)
                    Brush brush = new SolidBrush(body.Metadata.Color);

                    lock (renderLock)
                    {
                        graphics.FillEllipse(brush, renderX, renderY, 10, 10);
                    }
                });

                var universeTime = new TimeSpan(0, 0, (int)Universe.T);
                graphics.DrawString(
                    $"Universe Time: {universeTime:d\\:hh\\:mm\\:ss}",
                    Resources.Arial16Font,
                    Resources.WhiteSolidBrush,
                    UniverseTimeStringPosition);
                graphics.DrawString(
                    $"Zoom: {zoom}",
                    Resources.Arial16Font,
                    Resources.WhiteSolidBrush,
                    UniverseZoomStringPosition);
            }

            return bitmap;
        }
    }
}
