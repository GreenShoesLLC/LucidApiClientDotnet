using System.Collections.Generic;

namespace LucidApi.Models
{
    public class Items
    {
        public List<Shape> Shapes { get; set; }
        public List<Line> Lines { get; set; }
        public List<Group> Groups { get; set; }
        public List<Layer> Layers { get; set; }
    }
}