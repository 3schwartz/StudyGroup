namespace SignalRShared
{
    internal static class Contracts
    {
        #region Client side methods
        internal static string AddedDroid = "AddedDroid";
        #endregion

        #region Server side methods
        internal static string GetDroids = nameof(Hubs.DroidHub.GetDroids);
        internal static string AddDroids = nameof(Hubs.DroidHub.AddDroids);
        internal static string SubscribeDroidsAdded = nameof(Hubs.DroidHub.SubscribeDroidsAdded);
        #endregion
    }
}
