using System;

namespace AI
{
    [Serializable]
    [Flags]
    public enum PawnTags
    {
        Character = 1 << 1,
        Destructible = 1 << 2,
        Structure = 1 << 3,
        Gate = 1 << 4,
        Neutral = 1 << 5,
        All = ~0
    }
}
