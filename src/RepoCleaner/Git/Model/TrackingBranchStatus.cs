namespace Develix.RepoCleaner.Git.Model;

public enum TrackingBranchStatus
{
    //[Display("Ungültig", "???", KnownColor.Magenta)]
    Invalid = 0,

    //[Display("Ohne", "Non", KnownColor.Gray)]
    None,

    //[Display("Aktiv", "Act", KnownColor.Green)]
    Active,

    //[Display("Gelöscht", "Del", KnownColor.Red)]
    Deleted
}
