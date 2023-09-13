namespace Microsoft.Robots
{
    public class MoveCommand
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public double rx { get; set; }
        public double ry { get; set; }
        public double rz { get; set; }
        public double velocity { get; set; }
        public double acceleration { get; set; }
    }
}