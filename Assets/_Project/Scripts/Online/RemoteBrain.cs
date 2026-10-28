namespace FartRafters
{
    public class RemoteBrain : RiderBrain
    {
        public RiderCommand Command { get; set; }

        public override RiderCommand GetCommand(Rider rider)
        {
            return Command;
        }
    }
}
