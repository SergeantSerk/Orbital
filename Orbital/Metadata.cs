using System.Drawing;

namespace Orbital
{
    public struct Metadata
    {
        public string Name { get; set; }
        public Color Color { get; set; }

        public Metadata(string name, Color color)
        {
            Name = name;
            Color = color;
        }
    }
}