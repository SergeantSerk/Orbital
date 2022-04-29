using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Orbital.Cli
{
    public static class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public static void Main()
        {
            // https://nssdc.gsfc.nasa.gov/planetary/planetfact.html
            var bodies = new List<Body>
            {
                new Body(
                    metadata: new Metadata("Sun"),
                    position: Vector3.Zero,
                    velocity: Vector3.Zero,
                    mass: 1988500e24),
                new Body(
                    metadata: new Metadata("Mercury"),
                    position: new Vector3(57.909e9, 0, 0),
                    velocity: new Vector3(0, 47.36e3, 0),
                    mass: 0.33010e24),
                new Body(
                    metadata: new Metadata("Venus"),
                    position: new Vector3(108.21e9, 0, 0),
                    velocity: new Vector3(0, 35.02e3, 0),
                    mass: 4.8673e24),
                new Body(
                    metadata: new Metadata("Earth"),
                    position: new Vector3(149.598e9, 0, 0),
                    velocity: new Vector3(0, 29.78e3, 0),
                    mass: 5.9722e24),
                new Body(
                    metadata: new Metadata("Mars"),
                    position: new Vector3(227.956e9, 0, 0),
                    velocity: new Vector3(0, 24.07e3, 0),
                    mass: 0.64169e24),
                new Body(
                    metadata: new Metadata("Jupiter"),
                    position: new Vector3(778.479e9, 0, 0),
                    velocity: new Vector3(0, 13.06e3, 0),
                    mass: 1898.13e24),
                new Body(
                    metadata: new Metadata("Saturn"),
                    position: new Vector3(1432.041e9, 0, 0),
                    velocity: new Vector3(0, 9.68e3, 0),
                    mass: 568.32e24),
                new Body(
                    metadata: new Metadata("Uranus"),
                    position: new Vector3(2867.043e9, 0, 0),
                    velocity: new Vector3(0, 6.8e3, 0),
                    mass: 86.811e24),
                new Body(
                    metadata: new Metadata("Neptune"),
                    position: new Vector3(4514.953e9, 0, 0),
                    velocity: new Vector3(0, 5.43e3, 0),
                    mass: 102.409e24),
                new Body(
                    metadata: new Metadata("Pluto"),
                    position: new Vector3(5869.656e9, 0, 0),
                    velocity: new Vector3(0, 4.67e3, 0),
                    mass: 0.01303e24)
            };

            double zoom = bodies
                .Select(_ => _.Position.X)
                .Max() * 0.2;
            // Add extra x% of space
            zoom += zoom * 0.01;
            var zoomVector = new Vector3(zoom);

            bool infinite = true;
            double t_0 = 0;
            double t = t_0;
            double t_end = 86400 * 365 * 10; // approximately a decade in seconds
            double dt = 60*60*24; // simulation step (seconds)
            var dtVector = new Vector3(dt);

            using var pen = new Pen(Color.White, 1f);

            string directoryName = "renders";
            string currentDirectory = Directory.GetCurrentDirectory();

            if (Directory.Exists(Path.Combine(currentDirectory, directoryName)))
            {
                Directory.Delete(directoryName, true);

            }
            Directory.CreateDirectory(directoryName);

            double i = 0;
            do
            {
                int width = 1000;
                int height = width;
                using var bitmap = new Bitmap(width, height);
                using Graphics graphics = Graphics.FromImage(bitmap);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                graphics.Clear(Color.Black);

                foreach (var body in bodies)
                {
                    Vector3 bmpVector = body.Position / zoomVector;
                    bmpVector.X *= width / 2;
                    bmpVector.Y *= height / 2;

                    bmpVector.X += width / 2;
                    bmpVector.Y += height / 2;

                    Color color;
                    if (body.Metadata.Name == "Sun")
                    {
                        color = Color.Yellow;
                    }
                    else
                    {
                        color = Color.White;
                    }

                    graphics.FillEllipse(new SolidBrush(color), (float)bmpVector.X, (float)bmpVector.Y, 8, 8);
                }

                bitmap.Save(Path.Combine(currentDirectory, directoryName, $"{i++}.png"), ImageFormat.Png);

                if (t % 1000 == 0)
                {
                    // Reduce console spam
                    Console.WriteLine($"Time: {t}" + Environment.NewLine);
                    PrintBodies(bodies);
                }

                // see https://en.wikipedia.org/wiki/N-body_simulation#Example_Simulations (accessed on 29/04/2022)
                foreach (var bodyA in bodies)
                {
                    Vector3 a_g = Vector3.Zero;

                    foreach (var bodyB in bodies)
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

                    bodyA.Velocity += a_g * dtVector;
                }

                foreach (var body in bodies)
                {
                    body.Position += body.Velocity * dtVector;
                }

                t += dt;

                //Thread.Sleep(100);
                Console.SetCursorPosition(0, 0);
            } while (t < t_end || infinite);
        }

        private static void PrintBodies(IEnumerable<Body> bodies)
        {
            int metadataNameMaxLength = bodies
                .Select(_ => _.Metadata.Name.Length)
                .Max();

            string masterOutput = string.Empty;
            foreach (var body in bodies)
            {
                masterOutput += $"{($"{body.Metadata.Name}:").PadRight(metadataNameMaxLength + 1)} " +
                    $"Position[{body.Position}] " +
                    $"Velocity[{body.Velocity}] " +
                    Environment.NewLine;
            }

            Console.Write(masterOutput);
        }
    }
}
