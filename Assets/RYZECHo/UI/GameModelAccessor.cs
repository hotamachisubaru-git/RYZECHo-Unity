using System;
using System.Collections.Generic;
using System.Linq;

namespace RYZECHo;

internal sealed class GameUiChoice
{
    public int Index { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public bool Selected { get; init; }
    public bool Enabled { get; init; } = true;
}

internal sealed class GameUiActor
{
    public string Name { get; init; } = string.Empty;
    public string Team { get; init; } = string.Empty;
    public float HealthRatio { get; init; }
    public float ShieldRatio { get; init; }
    public float MapX { get; init; }
    public float MapY { get; init; }
    public bool IsAlive { get; init; }
    public bool IsBoss { get; init; }
}

internal sealed class GameUiAbility
{
    public string Key { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public float Progress { get; init; }
    public bool Ready { get; init; }
}

internal sealed class GameUiState
{
    public GamePhase Phase { get; init; }
    public string PhaseTitle { get; init; } = string.Empty;
    public string ObjectiveTitle { get; init; } = string.Empty;
    public string ObjectiveBody { get; init; } = string.Empty;
    public string Controls { get; init; } = string.Empty;
    public string TimerLabel { get; init; } = string.Empty;
    public string TimerValue { get; init; } = string.Empty;
    public string WeaponCode { get; init; } = string.Empty;
    public string WeaponDetail { get; init; } = string.Empty;
    public string AgentDetail { get; init; } = string.Empty;
    public string ResultMessage { get; init; } = string.Empty;
    public string SiteLabel { get; init; } = string.Empty;
    public int Credits { get; init; }
    public int BuildPoints { get; init; }
    public int PlayerWins { get; init; }
    public int EnemyWins { get; init; }
    public int SelectedInvestment { get; init; }
    public bool IsPaused { get; init; }
    public bool ShowBriefing { get; init; }
    public bool AgentSkillPurchased { get; init; }
    public IReadOnlyList<GameUiChoice> BuildTools { get; init; } = Array.Empty<GameUiChoice>();
    public IReadOnlyList<GameUiChoice> Bosses { get; init; } = Array.Empty<GameUiChoice>();
    public IReadOnlyList<GameUiChoice> PrimaryWeapons { get; init; } = Array.Empty<GameUiChoice>();
    public IReadOnlyList<GameUiChoice> Sidearms { get; init; } = Array.Empty<GameUiChoice>();
    public IReadOnlyList<GameUiActor> Actors { get; init; } = Array.Empty<GameUiActor>();
    public IReadOnlyList<GameUiAbility> Abilities { get; init; } = Array.Empty<GameUiAbility>();
    public IReadOnlyList<string> StatusEffects { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ActivityFeed { get; init; } = Array.Empty<string>();
}

internal sealed partial class GameModel
{
    internal GameUiState GetUiState()
    {
        EnsureFriendlyEconomyState();
        var displayedWeapon = DisplayedWeaponType();
        var weapon = _weaponStats[displayedWeapon];
        var profile = PlayerAgentProfile();

        return new GameUiState
        {
            Phase = _phase,
            PhaseTitle = UiPhaseTitle(),
            ObjectiveTitle = UiObjectiveTitle(),
            ObjectiveBody = UiObjectiveBody(),
            Controls = UiControls(),
            TimerLabel = _phase == GamePhase.Hunt && _bombPlanted ? "爆破まで" : _phase == GamePhase.Hunt ? "残り時間" : "フェーズ",
            TimerValue = _phase == GamePhase.Hunt ? FormatUiTime(_roundTimer) : PhaseLabel(),
            WeaponCode = weapon.Code,
            WeaponDetail = $"{weapon.ShortLabel}  {weapon.MagazineAmmo}/{weapon.ReserveAmmo}  {GetFovDegrees(displayedWeapon):0}°",
            AgentDetail = $"{profile.Name} / {profile.Role}",
            ResultMessage = _resultMessage,
            SiteLabel = _phase == GamePhase.Hunt ? CurrentObjectiveSiteLabel() : "A / B",
            Credits = _credits,
            BuildPoints = _buildPoints,
            PlayerWins = _playerRoundWins,
            EnemyWins = _enemyRoundWins,
            SelectedInvestment = SelectedBossInvestment(),
            IsPaused = IsPaused,
            ShowBriefing = _showBriefing,
            AgentSkillPurchased = _agentSkillPurchased,
            BuildTools = BuildUiTools(),
            Bosses = BuildUiBosses(),
            PrimaryWeapons = BuildUiWeapons(PrimaryWeaponSelectionOrder(), _selectedWeapon),
            Sidearms = BuildUiWeapons(SidearmSelectionOrder(), _selectedSidearmWeapon),
            Actors = BuildUiActors(),
            Abilities = BuildUiAbilities(),
            StatusEffects = BuildUiStatusEffects(),
            ActivityFeed = _activityFeed.ToArray(),
        };
    }

    internal void UiSelectBuildTool(int index)
    {
        var tools = UiBuildToolOrder();
        if (_phase == GamePhase.Construct && index >= 0 && index < tools.Length)
        {
            _selectedBuildTool = tools[index];
        }
    }

    internal void UiSelectBoss(int index)
    {
        var candidates = BossCandidateNames();
        if (_phase == GamePhase.Bet && index >= 0 && index < candidates.Length)
        {
            TrySelectBoss(candidates[index]);
        }
    }

    internal void UiAdjustInvestment(int delta)
    {
        if (_phase == GamePhase.Bet)
        {
            AdjustSelectedInvestment(delta);
        }
    }

    internal void UiSelectPrimaryWeapon(int index)
    {
        var weapons = PrimaryWeaponSelectionOrder();
        if (_phase == GamePhase.Bet && index >= 0 && index < weapons.Length)
        {
            _selectedWeapon = weapons[index];
            SyncSelectedBetTotal();
        }
    }

    internal void UiSelectSidearm(int index)
    {
        var weapons = SidearmSelectionOrder();
        if (_phase == GamePhase.Bet && index >= 0 && index < weapons.Length)
        {
            _selectedSidearmWeapon = weapons[index];
            SyncSelectedBetTotal();
        }
    }

    internal void UiCycleAgent()
    {
        if (_phase is GamePhase.Construct or GamePhase.Bet)
        {
            CycleSelectedAgent();
        }
    }

    internal void UiPurchaseAgentSkill()
    {
        if (_phase == GamePhase.Bet)
        {
            TryPurchaseAgentSkill();
        }
    }

    internal void UiConfirmPhase()
    {
        if (_phase == GamePhase.Construct)
        {
            _sideSwapConstructPending = false;
            BeginBetPhase();
        }
        else if (_phase == GamePhase.Bet)
        {
            StartRound();
        }
        else if (_phase == GamePhase.RoundResult)
        {
            if (_resultDestination == GamePhase.Bet)
            {
                BeginBetPhase();
            }
            else if (_resultDestination == GamePhase.Construct)
            {
                EnterSideSwapConstructPhase();
            }
            else
            {
                _phase = _resultDestination;
            }
        }
        else if (_phase is GamePhase.Victory or GamePhase.Defeat)
        {
            ResetCampaign();
        }
    }

    internal void UiRestartCampaign()
    {
        ResetCampaign();
        IsPaused = false;
    }

    private IReadOnlyList<GameUiChoice> BuildUiTools()
    {
        var tools = UiBuildToolOrder();
        return tools.Select((tool, index) => new GameUiChoice
        {
            Index = index,
            Code = index < 5 ? (index + 1).ToString() : "TAB",
            Name = BuildToolShortLabel(tool),
            Detail = BuildToolLabel(tool),
            Selected = tool == _selectedBuildTool,
            Enabled = _buildPoints >= MapEditApRules.ToolApCost(tool),
        }).ToArray();
    }

    private IReadOnlyList<GameUiChoice> BuildUiBosses()
    {
        return BossCandidateNames().Select((name, index) =>
        {
            var actor = FriendlyActorByName(name);
            var investment = GetFriendlyInvestment(name);
            return new GameUiChoice
            {
                Index = index,
                Code = (index + 1).ToString(),
                Name = name,
                Detail = $"{AgentCatalog.Get(actor?.Agent ?? AgentKind.Veil).Name} / {investment}c / 残り {BossSelectionsRemaining(name)}回 / {BossBuffSummary(investment)}",
                Selected = name == _selectedBossName,
                Enabled = CanSelectBoss(name),
            };
        }).ToArray();
    }

    private IReadOnlyList<GameUiChoice> BuildUiWeapons(IEnumerable<WeaponType> weaponTypes, WeaponType selected)
    {
        return weaponTypes.Select((type, index) =>
        {
            var stats = _weaponStats[type];
            return new GameUiChoice
            {
                Index = index,
                Code = stats.Code,
                Name = stats.Label,
                Detail = $"{stats.Category} / {stats.Cost}c / DMG {stats.Damage:0} / {stats.VisionClass}",
                Selected = type == selected,
                Enabled = stats.Cost <= _credits,
            };
        }).ToArray();
    }

    private IReadOnlyList<GameUiActor> BuildUiActors()
    {
        var actors = new List<GameUiActor>();
        AddUiActor(actors, _player, "ALLY");
        foreach (var ally in _allies)
        {
            AddUiActor(actors, ally, "ALLY");
        }
        foreach (var enemy in _enemies)
        {
            AddUiActor(actors, enemy, "ENEMY");
        }
        return actors;
    }

    private void AddUiActor(ICollection<GameUiActor> output, Actor actor, string team)
    {
        var width = Math.Max(1f, WorldBounds.Width);
        var height = Math.Max(1f, WorldBounds.Height);
        output.Add(new GameUiActor
        {
            Name = actor.Name,
            Team = team,
            HealthRatio = actor.MaxHealth <= 0f ? 0f : Math.Clamp(actor.Health / actor.MaxHealth, 0f, 1f),
            ShieldRatio = actor.MaxShield <= 0f ? 0f : Math.Clamp(actor.Shield / actor.MaxShield, 0f, 1f),
            MapX = Math.Clamp((actor.Position.X - WorldBounds.Left) / width, 0f, 1f),
            MapY = Math.Clamp((actor.Position.Y - WorldBounds.Top) / height, 0f, 1f),
            IsAlive = actor.IsAlive,
            IsBoss = actor.IsBoss,
        });
    }

    private IReadOnlyList<GameUiAbility> BuildUiAbilities()
    {
        return new[]
        {
            BuildUiAbility(AgentAbilitySlot.SkillOne, "1"),
            BuildUiAbility(AgentAbilitySlot.SkillTwo, "2"),
            BuildUiAbility(AgentAbilitySlot.Ultimate, "3"),
        };
    }

    private GameUiAbility BuildUiAbility(AgentAbilitySlot slot, string key)
    {
        return new GameUiAbility
        {
            Key = key,
            Name = AgentAbilityName(slot),
            Progress = Math.Clamp(AgentAbilityProgress(slot), 0f, 1f),
            Ready = AgentAbilityReady(slot) && (slot == AgentAbilitySlot.Ultimate || _agentSkillPurchased),
        };
    }

    private IReadOnlyList<string> BuildUiStatusEffects()
    {
        var effects = new List<string>();
        if (_playerDashTimer > 0f) effects.Add($"DASH {_playerDashTimer:0.0}s");
        if (_playerOverdriveTimer > 0f) effects.Add($"OVERDRIVE {_playerOverdriveTimer:0.0}s");
        if (_playerHealingTimer > 0f) effects.Add($"REGEN {_playerHealingTimer:0.0}s");
        if (_playerGhostTimer > 0f) effects.Add($"GHOST {_playerGhostTimer:0.0}s");
        if (IsPlayerBreathingExposed()) effects.Add("BREATH EXPOSED");
        return effects;
    }

    private string UiPhaseTitle()
    {
        return _phase switch
        {
            GamePhase.Construct => "構築フェーズ",
            GamePhase.Bet => "ボス選出 & ロードアウト",
            GamePhase.Hunt => IsPlayerTeamAttacking() ? "攻撃ラウンド" : "防衛ラウンド",
            GamePhase.RoundResult => "ラウンド精算",
            GamePhase.Victory => "マッチ勝利",
            _ => "作戦失敗",
        };
    }

    private string UiObjectiveTitle()
    {
        if (_phase == GamePhase.Hunt && _bombPlanted) return IsPlayerTeamAttacking() ? "爆破維持" : "ボム解除";
        if (_phase == GamePhase.Hunt) return IsPlayerTeamAttacking() ? "サイト侵攻" : "サイト防衛";
        if (_phase == GamePhase.Construct) return _sideSwapConstructPending ? "後半戦の再構築" : "防衛設備を配置";
        if (_phase == GamePhase.Bet) return "出撃準備";
        return _phase == GamePhase.RoundResult ? "精算" : "マッチ結果";
    }

    private string UiObjectiveBody()
    {
        return _phase switch
        {
            GamePhase.Construct => $"{BuildToolLabel(_selectedBuildTool)} / 残り {_buildPoints} AP",
            GamePhase.Bet => $"{PlayerRoleLabel()} / ボス {_selectedBossName} / 投資 {_selectedBet}c / {_weaponStats[_selectedWeapon].Code} + {_weaponStats[_selectedSidearmWeapon].Code}",
            GamePhase.Hunt when _bombPlanted => $"サイト {CurrentObjectiveSiteLabel()} / 残り {Math.Max(0f, _roundTimer):0.0}s",
            GamePhase.Hunt => $"敵残数 {LiveEnemyCount()}/{TeamSize} / 注力サイト {GetBombSite(_attackFocusSite).Label}",
            _ => _resultMessage,
        };
    }

    private string UiControls()
    {
        return _phase switch
        {
            GamePhase.Construct => "1-5 選択 / Tab 全設置物 / 左クリック 配置 / 右クリック 撤去 / Enter 確定",
            GamePhase.Bet => "1-4 ボス / A・D 投資 / Q・E 武器 / R メイン・サブ / Enter 出撃",
            GamePhase.Hunt => "WASD 移動 / マウス 照準 / 左クリック 射撃 / 1・2・3 スキル / F サイト操作",
            _ => "Enter 次へ / R 再挑戦",
        };
    }

    private static string FormatUiTime(float seconds)
    {
        var total = Math.Max(0, (int)Math.Ceiling(seconds));
        return $"{total / 60}:{total % 60:00}";
    }

    private static BuildToolKind[] UiBuildToolOrder()
    {
        return new[]
        {
            BuildToolKind.BlastDoor,
            BuildToolKind.HoneyTrap,
            BuildToolKind.StaticNest,
            BuildToolKind.ReconBeacon,
            BuildToolKind.ShieldRelay,
            BuildToolKind.PortableCover,
            BuildToolKind.VisorWall,
            BuildToolKind.HoloDecoy,
        };
    }
}
