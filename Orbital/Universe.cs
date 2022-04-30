namespace Orbital
{
    public class Universe
    {
        public const double BIG_G = 6.67e-11;

        /// <summary>
        /// Simulation steps
        /// </summary>
        public double UpdateDt { get; set; }

        public Vector3 UpdateDtVector => new(UpdateDt);

        public Universe(double updateDt)
        {
            UpdateDt = updateDt;
        }
    }
}
