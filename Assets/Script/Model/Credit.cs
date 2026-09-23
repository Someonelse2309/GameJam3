using System.Collections.Generic;

// This mirrors the lowest level of your JSON (the actual credit details)
public class CreditEntry
{
    public string Title { get; set; }
    public string Creator { get; set; }
    public string Link { get; set; }
}

// This mirrors the root of your JSON containing the dictionaries
public class CreditsDatabase
{
    public Dictionary<string, CreditEntry> Music { get; set; }
    public Dictionary<string, CreditEntry> SFX { get; set; }
    public Dictionary<string, CreditEntry> Visual { get; set; }
}