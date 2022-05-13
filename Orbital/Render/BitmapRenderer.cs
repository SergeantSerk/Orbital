using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Orbital.Render
{
    public class BitmapRenderer
    {
        public Universe Universe { get; }

        public BitmapRenderer(Universe universe)
        {
            Universe = universe;
        }

        [SupportedOSPlatform("windows")]
        public Bitmap Render(int imageWidth, int imageHeight, double offsetX, double offsetY, double zoom)
        {
            // Scale to viewport properly without stretching the render
            double scaleRatio = Math.Min(imageWidth, imageHeight);

            // TO-DO: should viewport be the max magnitude of all the bodies?
            double viewport = Universe.Bodies
                .Select(_ => _.Position.Magnitude)
                .Max();
            // Add extra x% of padding space
            // TO-DO: is this necessary for other cases?
            viewport += viewport * 0.01;
            var viewportVector = new Vector3(viewport);

            var bitmap = new Bitmap(imageWidth, imageHeight);

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
                    bodyViewportVector.X += (imageWidth + offsetX) / 2;
                    bodyViewportVector.Y += (imageHeight + offsetY) / 2;

                    float renderX = (float)bodyViewportVector.X;
                    float renderY = (float)bodyViewportVector.Y;

                    // TO-DO: Colouring (if any)
                    Brush brush = new SolidBrush(body.Metadata.Color);

                    lock (renderLock)
                    {
                        graphics.FillEllipse(brush, renderX, renderY, 10, 10);
                    }
                });
            }

            return bitmap;
        }
    }
}
