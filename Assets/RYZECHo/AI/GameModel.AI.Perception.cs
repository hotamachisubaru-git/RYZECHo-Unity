namespace RYZECHo;

internal sealed partial class GameModel
{
    private Actor? PickBestTarget(Vector2 origin, float range, ActorType sourceType)
    {
        IEnumerable<Actor> candidates = sourceType == ActorType.Enemy
            ? LivePlayerTeam()
            : _enemies.Where(actor => actor.IsAlive);

        var target = candidates
            .Where(actor => Distance(origin, actor.Position) <= range && HasLineOfSight(sourceType, origin, actor.Position))
            .OrderBy(actor => Distance(origin, actor.Position))
            .FirstOrDefault();

        if (target is not null && sourceType != ActorType.Enemy && target.Type == ActorType.Enemy)
        {
            RevealEnemyToTeam(target);
        }

        return target;
    }

    private Actor? PickEnemyTarget(Actor enemy)
    {
        var defenders = LivePlayerTeam()
            .Where(actor => actor.Type != ActorType.Player || _playerGhostTimer <= 0f)
            .Where(actor => actor.GhostTimer <= 0f)
            .Where(actor => Distance(enemy.Position, actor.Position) <= _weaponStats[enemy.Weapon].ProjectileRange + 30f)
            .OrderBy(actor => Distance(enemy.Position, actor.Position))
            .ToList();

        return defenders.FirstOrDefault(actor => HasLineOfSight(enemy, actor.Position));
    }

    private Actor? PickRaycastTarget(Vector2 origin, Vector2 targetPoint, float range)
    {
        var direction = new Vector2(targetPoint.X - origin.X, targetPoint.Y - origin.Y);
        var length = MathF.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
        if (length <= 1f)
        {
            return null;
        }

        direction = new Vector2(direction.X / length, direction.Y / length);

        var bestDistance = float.MaxValue;
        Actor? best = null;

        foreach (var enemy in _enemies.Where(actor => actor.IsAlive))
        {
            var toEnemy = new Vector2(enemy.Position.X - origin.X, enemy.Position.Y - origin.Y);
            var projection = (toEnemy.X * direction.X) + (toEnemy.Y * direction.Y);
            if (projection < 0f || projection > range)
            {
                continue;
            }

            var closest = new Vector2(origin.X + (direction.X * projection), origin.Y + (direction.Y * projection));
            if (Distance(closest, enemy.Position) <= enemy.Radius + 5f && projection < bestDistance && HasLineOfSight(_player, enemy.Position))
            {
                best = enemy;
                bestDistance = projection;
            }
        }

        return best;
    }

    private bool PlayerHasDirectSightTo(Vector2 position)
    {
        return ActorHasDirectSightTo(_player, position);
    }

    private bool ActorHasDirectSightTo(Actor actor, Vector2 position)
    {
        if (!actor.IsAlive)
        {
            return false;
        }

        var weapon = _weaponStats[actor.Weapon];
        var vector = new Vector2(position.X - actor.Position.X, position.Y - actor.Position.Y);
        var distance = MathF.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));
        if (distance > weapon.VisionRange)
        {
            return false;
        }

        if (distance <= 1f)
        {
            return true;
        }

        var angle = MathF.Atan2(vector.Y, vector.X);
        var difference = NormalizeAngle(angle - actor.FacingAngle);
        if (MathF.Abs(difference) > DegreesToRadians(GetFovDegrees(actor.Weapon) / 2f))
        {
            return false;
        }

        return HasLineOfSight(actor, position);
    }

    private void RevealEnemiesInActorVision(Actor actor, float duration = SharedVisionDurationSeconds)
    {
        foreach (var enemy in _enemies.Where(enemy => enemy.IsAlive && ActorHasDirectSightTo(actor, enemy.Position)))
        {
            RevealEnemyToTeam(enemy, duration);
        }
    }

    private bool PlayerCanSee(Actor enemy)
    {
        if (IsEnemySharedVisible(enemy))
        {
            return true;
        }

        if (!PlayerHasDirectSightTo(enemy.Position))
        {
            return false;
        }

        if (_structures.Any(structure => structure.Kind == StructureKind.StaticNest && Distance(enemy.Position, CellCenter(structure.Cell)) <= 90f))
        {
            if (_hunterEyeTimer > 0f && PlayerHasDirectSightTo(enemy.Position))
            {
                return true;
            }

            return Distance(_player.Position, enemy.Position) <= 120f;
        }

        return true;
    }

    private float GetFovDegrees(WeaponType weaponType)
    {
        return _weaponStats[weaponType].ScopedFov ? SniperFovDegrees : DefaultFovDegrees;
    }

    private AudioOcclusionProfile GetAudioOcclusionProfile(Vector2 position)
    {
        return GetAudioOcclusionProfile(_player.Position, position);
    }

    private AudioOcclusionProfile GetAudioOcclusionProfile(Vector2 listenerPosition, Vector2 sourcePosition)
    {
        return AudioRippleVisualRules.CalculateOcclusion(CountOccludingCells(listenerPosition, sourcePosition));
    }

    private bool PlayerCanPerceive(Vector2 position, float strength)
    {
        if (_phase != GamePhase.Hunt)
        {
            return true;
        }

        var hearing = AudioRippleVisualRules.CalculateHearingRange(
            _player.HearingRange,
            _weaponStats[_player.Weapon].HearingMultiplier,
            strength,
            GetAudioOcclusionProfile(position));
        return Distance(_player.Position, position) <= hearing;
    }

    private List<Point> FindPath(Point start, Point goal)
    {
        if (start == goal)
        {
            return new List<Point> { start };
        }

        var frontier = new Queue<Point>();
        frontier.Enqueue(start);

        var cameFrom = new Dictionary<Point, Point?>
        {
            [start] = null,
        };

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            foreach (var neighbor in Neighbors(current))
            {
                if (cameFrom.ContainsKey(neighbor) || IsBlockedCell(neighbor))
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                if (neighbor == goal)
                {
                    frontier.Clear();
                    break;
                }

                frontier.Enqueue(neighbor);
            }
        }

        if (!cameFrom.ContainsKey(goal))
        {
            return new List<Point>();
        }

        var path = new List<Point>();
        Point? cursor = goal;
        while (cursor is not null)
        {
            path.Add(cursor.Value);
            cursor = cameFrom[cursor.Value];
        }

        path.Reverse();
        return path;
    }

    private IEnumerable<Point> Neighbors(Point cell)
    {
        var candidates = new[]
        {
            new Point(cell.X + 1, cell.Y),
            new Point(cell.X - 1, cell.Y),
            new Point(cell.X, cell.Y + 1),
            new Point(cell.X, cell.Y - 1),
        };

        foreach (var candidate in candidates)
        {
            if (candidate.X >= 0 && candidate.X < GridColumns && candidate.Y >= 0 && candidate.Y < GridRows)
            {
                yield return candidate;
            }
        }
    }

    private bool IsBlockedCell(Point cell)
    {
        if (_permanentWalls.Contains(cell))
        {
            return true;
        }

        return _structures.Any(structure => IsRouteBlockingStructure(structure.Kind) && structure.Cell == cell && structure.Health > 0f);
    }

    private Vector2 ResolveCollision(Vector2 desiredPosition, float radius)
    {
        var clamped = new Vector2(
            Math.Clamp(desiredPosition.X, WorldBounds.Left + radius + 2f, WorldBounds.Right - radius - 2f),
            Math.Clamp(desiredPosition.Y, WorldBounds.Top + radius + 2f, WorldBounds.Bottom - radius - 2f));

        foreach (var blockedCell in _permanentWalls.Concat(_structures.Where(structure => IsRouteBlockingStructure(structure.Kind) && structure.Health > 0f).Select(structure => structure.Cell)))
        {
            var expanded = RectangleF.Inflate(CellRectangle(blockedCell), radius, radius);
            if (!expanded.Contains(clamped))
            {
                continue;
            }

            var center = CellCenter(blockedCell);
            var push = new Vector2(clamped.X - center.X, clamped.Y - center.Y);
            var length = MathF.Max(1f, MathF.Sqrt((push.X * push.X) + (push.Y * push.Y)));
            clamped = new Vector2(center.X + ((push.X / length) * (CellSize / 2f + radius + 2f)), center.Y + ((push.Y / length) * (CellSize / 2f + radius + 2f)));
        }

        return clamped;
    }

    private bool HasLineOfSight(Actor actor, Vector2 end)
    {
        return HasLineOfSight(actor.Type, actor.Position, end);
    }

    private bool HasLineOfSight(ActorType sourceType, Vector2 start, Vector2 end)
    {
        if (IsLineBlockedByWorldEffect(start, end))
        {
            return false;
        }

        var distance = Distance(start, end);
        var steps = Math.Max(2, (int)(distance / 8f));

        for (var step = 1; step < steps; step++)
        {
            var progress = step / (float)steps;
            var sample = new Vector2(
                start.X + ((end.X - start.X) * progress),
                start.Y + ((end.Y - start.Y) * progress));
            var cell = WorldToCell(sample);
            if (IsVisionBlockedCell(cell, sourceType))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsVisionBlockedCell(Point cell, ActorType sourceType)
    {
        if (_permanentWalls.Contains(cell))
        {
            return true;
        }

        foreach (var structure in _structures.Where(structure => structure.Cell == cell && structure.Health > 0f))
        {
            if (structure.Kind is StructureKind.BlastDoor or StructureKind.PortableCover)
            {
                return true;
            }

            if (structure.Kind == StructureKind.VisorWall && !SameTeamSide(sourceType, structure.OwnerType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool SameTeamSide(ActorType left, ActorType right)
    {
        return IsFriendlyActorType(left) == IsFriendlyActorType(right);
    }

    private static bool IsFriendlyActorType(ActorType actorType)
    {
        return actorType is ActorType.Player or ActorType.Ally;
    }

    private int CountOccludingCells(Vector2 start, Vector2 end)
    {
        var distance = Distance(start, end);
        var steps = Math.Max(2, (int)(distance / 6f));
        var blockedCells = new HashSet<Point>();

        for (var step = 1; step < steps; step++)
        {
            var progress = step / (float)steps;
            var sample = new Vector2(
                start.X + ((end.X - start.X) * progress),
                start.Y + ((end.Y - start.Y) * progress));
            var cell = WorldToCell(sample);
            if (_permanentWalls.Contains(cell) || _structures.Any(structure => structure.Cell == cell && structure.Health > 0f && structure.Kind is StructureKind.BlastDoor or StructureKind.PortableCover or StructureKind.VisorWall))
            {
                blockedCells.Add(cell);
            }
        }

        return blockedCells.Count;
    }

}
