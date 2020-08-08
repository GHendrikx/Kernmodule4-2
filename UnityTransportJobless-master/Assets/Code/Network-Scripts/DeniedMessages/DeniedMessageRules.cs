using System.Collections.Generic;

public static class DeniedMessageRules
{
    public static Dictionary<uint, string> DeniedMessages = new Dictionary<uint, string>
    {
        { 0, "Cannot connect because the game has already started." },
        { 1, "Too many people have joined the server. Try again in a couple of minutes."},
        { 2, "Can't Escape from a room with an monster in it." },
        { 3, "It's not your turn." },
        { 4, "Can't Attack when there isn't a monster."},
        { 5, "Can't Claim a treasure when there isn't a treasure." },
        { 6, "Can't Claim a treasure with an monster in it."}
    };
}
