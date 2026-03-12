namespace AmbiMass.SpectrumStream.Services.TyphoonSupport
{
    public struct SystemStatus
    {
        public OnlineStatus OnlineStatus;
    }

    public enum OnlineStatus
    {
        Offline,
        Online
    }
}