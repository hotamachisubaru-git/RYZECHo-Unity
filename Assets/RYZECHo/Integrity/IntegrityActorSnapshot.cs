namespace RYZECHo;

internal readonly record struct IntegrityActorSnapshot(
    Vector2 Position,
    float Health,
    float Shield,
    bool WasAlive,
    WeaponType Weapon);

