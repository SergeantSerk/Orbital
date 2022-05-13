namespace Orbital
{
    public class Simulation
    {
        public Universe Universe { get; }

        public double TStart { get; }
        public double TEnd { get; set; }

        public bool Infinite { get; set; }
        public bool Running { get; set; }

        /// <summary>
        /// Infinite simulation with no end.
        /// </summary>
        public Simulation(Universe universe)
        {
            Universe = universe;
            Infinite = true;
            Running = true;
        }

        public Simulation(Universe universe, double tStart, double tEnd) : this(universe)
        {
            Infinite = false;
            Running = true;

            TStart = tStart;
            TEnd = tEnd;
        }
    }
}
