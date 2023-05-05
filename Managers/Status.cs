namespace GaleForceCore.Managers
{
    public enum Status
    {
        None = 0,
        Awaiting = 1,
        Ready = 2,
        Granted = 3, //only used to hand to caller when consuming - server is InUse
        Allocated = 4,
        InUse = 5,
        Completed = 6,
        Released = 7,
        Abandoned = 8,
        Deleted = 9
    }
}
