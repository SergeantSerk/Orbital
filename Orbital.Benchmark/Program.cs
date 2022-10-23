using System.Collections.Concurrent;
using System.Diagnostics;

namespace Orbital.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            int bodyCount = 2500;
            ConcurrentBag<Body> bodies = InitialiseBodies(bodyCount, out Stopwatch initialiseBodiesStopwatch);
            Console.WriteLine($"Initialisation took {initialiseBodiesStopwatch.ElapsedMilliseconds / 1000.0} seconds...");

            double universeTimeResolution = 1.0;    // 1 second
            var universe = new Universe(bodies, universeTimeResolution);
            var simulation = new Simulation(universe);

            int loopCount = 20;
            var elapsedTicks = new List<long>();
            for (int i = 0; i < loopCount; ++i)
            {
                TickSimulation(simulation, out Stopwatch stopwatch);
                Console.WriteLine("- Universe Tick Duration: {0} ms", stopwatch.ElapsedMilliseconds);
                elapsedTicks.Add(stopwatch.ElapsedMilliseconds);
            }

            Console.WriteLine($"Results - Min: {elapsedTicks.Min()} Avg: {elapsedTicks.Average()} Max: {elapsedTicks.Max()}");
            Console.WriteLine(string.Join(' ', elapsedTicks));

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }

        private static void TickSimulation(Simulation simulation, out Stopwatch stopwatch)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            simulation.Universe.Tick();

            stopwatch.Stop();
        }

        private static ConcurrentBag<Body> InitialiseBodies(int bodyCount, out Stopwatch stopwatch)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            int minXSpaceConfine = -1000, maxXSpaceConfine = 1000;
            int minYSpaceConfine = -1000, maxYSpaceConfine = 1000;
            int minZSpaceConfine = -1000, maxZSpaceConfine = 1000;

            int minXVelocityConfine = -1000, maxXVelocityConfine = 1000;
            int minYVelocityConfine = -1000, maxYVelocityConfine = 1000;
            int minZVelocityConfine = -1000, maxZVelocityConfine = 1000;

            double massMax = 1000.0;

            var bodies = new ConcurrentBag<Body>();
            Parallel.For(0, bodyCount, i =>
            {
                var random = new Random(i);
                var position = new Vector3(
                    x: random.Next(minXSpaceConfine, maxXSpaceConfine),
                    y: random.Next(minYSpaceConfine, maxYSpaceConfine),
                    z: random.Next(minZSpaceConfine, maxZSpaceConfine));
                var velocity = new Vector3(
                    x: random.Next(minXVelocityConfine, maxXVelocityConfine),
                    y: random.Next(minYVelocityConfine, maxYVelocityConfine),
                    z: random.Next(minZVelocityConfine, maxZVelocityConfine));
                double mass = massMax * (random.NextDouble() + 1.0);

                var body = new Body(
                    metadata: new Metadata(),
                    position: position,
                    velocity: velocity,
                    mass: mass,
                    radius: 1.0);
                bodies.Add(body);
            });

            stopwatch.Stop();

            return bodies;
        }
    }
}