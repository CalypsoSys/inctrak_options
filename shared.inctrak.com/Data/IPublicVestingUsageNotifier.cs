namespace IncTrak.Data
{
    public interface IPublicVestingUsageNotifier
    {
        void Notify(PublicVestingUsageEvent usageEvent);
    }
}
