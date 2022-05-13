using System;
using System.Collections.Generic;

namespace Orbital
{
    public class Universe
    {
        public const double BIG_G = 6.67e-11;

        public double T { get; set; }

        /// <summary>
        /// Simulation steps
        /// </summary>
        public double TickResolution { get; set; }
        public Vector3 TickResolutionVector => new(TickResolution);

        public IEnumerable<Body> Bodies { get; }

        public Universe(IEnumerable<Body> bodies, double tickResolution)
        {
            Bodies = bodies;
            TickResolution = tickResolution;
        }

        public void Tick()
        {
            // See https://en.wikipedia.org/wiki/N-body_simulation#Example_Simulations (accessed on 29/04/2022)
            foreach (var bodyA in Bodies)
            {
                Vector3 a_g = Vector3.Zero;

                foreach (var bodyB in Bodies)
                {
                    if (bodyB != bodyA)
                    {
                        Vector3 radiusVector = bodyA.Position - bodyB.Position;
                        double radiusVectorMag = radiusVector.Magnitude;

                        double acceleration = -BIG_G * bodyB.Mass / Math.Pow(radiusVectorMag, 2);
                        var accelerationVector = new Vector3(acceleration);

                        var radiusUnitVector = new Vector3(
                            x: radiusVector.X / radiusVectorMag,
                            y: radiusVector.Y / radiusVectorMag,
                            z: radiusVector.Z / radiusVectorMag);

                        a_g += radiusUnitVector * accelerationVector;
                    }
                }

                bodyA.Velocity += a_g * TickResolutionVector;
            }

            foreach (var body in Bodies)
            {
                body.Position += body.Velocity * TickResolutionVector;
            }

            T += TickResolution;
        }
    }
}
