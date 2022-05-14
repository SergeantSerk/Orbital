using Orbital.Render;
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

            double tickResolution = 86400 * 7;  // 7 days
            var universe = new Universe(bodies, tickResolution);
            var simulation = new Simulation(universe);
            var renderer = new BitmapRenderer(simulation.Universe);

            int width = 1000;
            int height = width;
            double offsetX = 0.0, offsetY = 0.0;
            double zoom = 1;

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
                using var bitmap = renderer.Render(width, height, offsetX, offsetY, zoom);
                bitmap.Save(Path.Combine(currentDirectory, directoryName, $"{i++}.png"), ImageFormat.Png);

                if (simulation.Universe.T % 1000 == 0)
                {
                    // Reduce console spam
                    Console.WriteLine($"Time: {simulation.Universe.T}" + Environment.NewLine);
                    PrintBodies(bodies);
                }

                simulation.Universe.Tick();

                Console.SetCursorPosition(0, 0);
            } while (simulation.Universe.T < simulation.TEnd || simulation.Infinite);
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
