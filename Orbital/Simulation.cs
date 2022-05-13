namespace Orbital
{
    public class Simulation
    {
        public Universe Universe { get; }

        public double TStart { get; }
        public double TEnd { get; set; }

        public bool Infinite { get; set; }
        public bool Simulating { get; set; }

        /// <summary>
        /// Infinite simulation with no end.
        /// </summary>
        public Simulation(Universe universe)
        {
            Universe = universe;
            Infinite = true;
            Simulating = true;
        }

        public Simulation(Universe universe, double tStart, double tEnd) : this(universe)
        {
            Infinite = false;
            Simulating = true;

            TStart = tStart;
            TEnd = tEnd;
        }
    }
}
