namespace Orbital
{
    public class Simulation
    {
        public double T_0 { get; }
        public double T { get; set; }
        public double T_End { get; set; }

        public bool Infinite { get; set; }
        public bool Running { get; set; }

        /// <summary>
        /// Infinite simulation with no end.
        /// </summary>
        public Simulation()
        {
            Infinite = true;
            Running = true;
        }

        public Simulation(double t_0, double t_end)
        {
            Infinite = false;
            Running = true;

            T_0 = t_0;
            T = t_0;
            T_End = t_end;
        }
    }
}
