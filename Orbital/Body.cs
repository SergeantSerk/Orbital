namespace Orbital
{
    public class Body
    {
        public Metadata Metadata { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }

        public double Mass { get; set; }
        public double Radius { get; set; }

        public Body(Metadata metadata, Vector3 position, Vector3 velocity, double mass, double radius)
        {
            Metadata = metadata;
            Position = position;
            Velocity = velocity;
            Mass = mass;
            Radius = radius;
        }
    }
}
