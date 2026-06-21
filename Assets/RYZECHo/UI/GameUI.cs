using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityColor = UnityEngine.Color;
using UnityVector2 = UnityEngine.Vector2;

namespace RYZECHo.UI;

internal sealed class GameUI : IDisposable
{
    private const float MinimapSize = 176f;

    private readonly UIDocument _document;
    private readonly GameUIConfig _config;
    private readonly VisualElement _root;
    private readonly Label _phaseTitle;
    private readonly Label _score;
    private readonly Label _objectiveTitle;
    private readonly Label _objectiveBody;
    private readonly Label _timerLabel;
    private readonly Label _timerValue;
    private readonly Label _credits;
    private readonly Label _weapon;
    private readonly Label _agent;
    private readonly Label _controls;
    private readonly VisualElement _abilityRow;
    private readonly VisualElement _statusRow;
    private readonly VisualElement _roster;
    private readonly VisualElement _minimapDots;
    private readonly VisualElement _activityFeed;
    private readonly VisualElement _buildPanel;
    private Label _buildPoints = null!;
    private VisualElement _buildChoices = null!;
    private readonly VisualElement _loadoutPanel;
    private VisualElement _bossChoices = null!;
    private Label _investment = null!;
    private VisualElement _primaryChoices = null!;
    private VisualElement _sidearmChoices = null!;
    private Label _skillPurchase = null!;
    private readonly VisualElement _briefing;
    private readonly Label _briefingTitle;
    private readonly Label _briefingBody;
    private readonly VisualElement _resultOverlay;
    private readonly Label _resultTitle;
    private readonly Label _resultBody;
    private readonly VisualElement _pauseOverlay;

    internal event Action<int>? BuildToolSelected;
    internal event Action<int>? BossSelected;
    internal event Action<int>? PrimarySelected;
    internal event Action<int>? SidearmSelected;
    internal event Action<int>? InvestmentAdjusted;
    internal event Action? AgentCycleRequested;
    internal event Action? SkillPurchaseRequested;
    internal event Action? ConfirmRequested;
    internal event Action? ResumeRequested;
    internal event Action? RestartRequested;
    internal event Action? QuitRequested;

    internal GameUI(UIDocument document, GameUIConfig config)
    {
        _document = document;
        _config = config;
        _root = document.rootVisualElement;
        _root.Clear();
        ConfigureRoot(_root);

        var topBar = Row("top-bar");
        topBar.style.height = 48;
        topBar.style.paddingLeft = 18;
        topBar.style.paddingRight = 18;
        ApplyPanel(topBar, _config.Background, _config.Border);
        _phaseTitle = Text("RYZECHØ", 18, _config.Cyan, true);
        _phaseTitle.style.flexGrow = 1;
        _score = Text("0 - 0", 18, _config.Text, true);
        topBar.Add(_phaseTitle);
        topBar.Add(_score);
        _root.Add(topBar);

        var objectiveBar = Row("objective-bar");
        objectiveBar.style.height = 58;
        objectiveBar.style.paddingLeft = 18;
        objectiveBar.style.paddingRight = 18;
        objectiveBar.style.alignItems = Align.Center;
        objectiveBar.style.backgroundColor = _config.Panel;

        var objectiveCopy = Column();
        objectiveCopy.style.flexGrow = 1;
        _objectiveTitle = Text("OBJECTIVE", 11, _config.Gold, true);
        _objectiveBody = Text(string.Empty, 12, _config.Text);
        objectiveCopy.Add(_objectiveTitle);
        objectiveCopy.Add(_objectiveBody);
        objectiveBar.Add(objectiveCopy);

        var timer = Column();
        timer.style.width = 92;
        timer.style.alignItems = Align.Center;
        _timerLabel = Text("TIME", 9, _config.MutedText, true);
        _timerValue = Text("0:00", 20, _config.Text, true);
        timer.Add(_timerLabel);
        timer.Add(_timerValue);
        objectiveBar.Add(timer);

        _credits = Text("0c", 16, _config.Gold, true);
        _credits.style.width = 94;
        _credits.style.unityTextAlign = TextAnchor.MiddleRight;
        objectiveBar.Add(_credits);
        _root.Add(objectiveBar);

        var leftHud = Column("left-hud");
        leftHud.style.position = Position.Absolute;
        leftHud.style.left = 16;
        leftHud.style.top = 122;
        leftHud.style.width = 250;
        leftHud.style.paddingTop = 10;
        leftHud.style.paddingBottom = 10;
        leftHud.style.paddingLeft = 10;
        leftHud.style.paddingRight = 10;
        ApplyPanel(leftHud, _config.Panel, _config.Border);
        _weapon = Text("WEAPON", 13, _config.Text, true);
        _agent = Text("AGENT", 11, _config.Cyan);
        _abilityRow = Row();
        _statusRow = Row();
        _statusRow.style.flexWrap = Wrap.Wrap;
        leftHud.Add(_weapon);
        leftHud.Add(_agent);
        leftHud.Add(Spacer(6));
        leftHud.Add(_abilityRow);
        leftHud.Add(_statusRow);
        _root.Add(leftHud);

        _roster = Column("roster");
        _roster.style.position = Position.Absolute;
        _roster.style.left = 16;
        _roster.style.bottom = 116;
        _roster.style.width = 250;
        _root.Add(_roster);

        var minimap = new VisualElement { name = "minimap" };
        minimap.style.position = Position.Absolute;
        minimap.style.right = 16;
        minimap.style.bottom = 116;
        minimap.style.width = MinimapSize;
        minimap.style.height = MinimapSize;
        ApplyPanel(minimap, new UnityColor(0.025f, 0.04f, 0.055f, 0.92f), _config.Border);
        var mapGrid = Text("A          SIGNAL MAP          B", 9, _config.MutedText, true);
        mapGrid.style.position = Position.Absolute;
        mapGrid.style.left = 8;
        mapGrid.style.top = 8;
        minimap.Add(mapGrid);
        _minimapDots = new VisualElement();
        _minimapDots.style.position = Position.Absolute;
        _minimapDots.style.left = 0;
        _minimapDots.style.top = 0;
        _minimapDots.style.right = 0;
        _minimapDots.style.bottom = 0;
        minimap.Add(_minimapDots);
        _root.Add(minimap);

        _activityFeed = Column("activity-feed");
        _activityFeed.style.position = Position.Absolute;
        _activityFeed.style.right = 16;
        _activityFeed.style.top = 122;
        _activityFeed.style.width = 310;
        _activityFeed.style.paddingTop = 8;
        _activityFeed.style.paddingBottom = 8;
        _activityFeed.style.paddingLeft = 10;
        _activityFeed.style.paddingRight = 10;
        ApplyPanel(_activityFeed, new UnityColor(0.025f, 0.04f, 0.055f, 0.78f), _config.Border);
        _root.Add(_activityFeed);

        _controls = Text(string.Empty, 10, _config.MutedText);
        _controls.style.position = Position.Absolute;
        _controls.style.left = 16;
        _controls.style.right = 16;
        _controls.style.bottom = 10;
        _controls.style.height = 24;
        _controls.style.unityTextAlign = TextAnchor.MiddleCenter;
        _root.Add(_controls);

        _buildPanel = BuildConstructPanel();
        _root.Add(_buildPanel);

        _loadoutPanel = BuildLoadoutPanel();
        _root.Add(_loadoutPanel);

        _briefing = Column("briefing");
        _briefing.style.position = Position.Absolute;
        _briefing.style.left = new Length(50, LengthUnit.Percent);
        _briefing.style.top = 118;
        _briefing.style.width = 520;
        _briefing.style.translate = new Translate(new Length(-50, LengthUnit.Percent), 0);
        _briefing.style.paddingTop = 10;
        _briefing.style.paddingBottom = 10;
        _briefing.style.paddingLeft = 14;
        _briefing.style.paddingRight = 14;
        ApplyPanel(_briefing, new UnityColor(0.04f, 0.07f, 0.1f, 0.96f), _config.Cyan);
        _briefingTitle = Text(string.Empty, 12, _config.Cyan, true);
        _briefingBody = Text(string.Empty, 10, _config.Text);
        _briefingBody.style.whiteSpace = WhiteSpace.Normal;
        _briefing.Add(_briefingTitle);
        _briefing.Add(_briefingBody);
        _root.Add(_briefing);

        _resultOverlay = Overlay("result-overlay");
        var resultCard = Card(560);
        _resultTitle = Text("ROUND RESULT", 24, _config.Gold, true);
        _resultBody = Text(string.Empty, 13, _config.Text);
        _resultBody.style.whiteSpace = WhiteSpace.Normal;
        var resultContinue = UiButton("続行  [Enter]", _config.Cyan, () => ConfirmRequested?.Invoke());
        resultCard.Add(_resultTitle);
        resultCard.Add(Spacer(12));
        resultCard.Add(_resultBody);
        resultCard.Add(Spacer(16));
        resultCard.Add(resultContinue);
        _resultOverlay.Add(resultCard);
        _root.Add(_resultOverlay);

        _pauseOverlay = Overlay("pause-overlay");
        var pauseCard = Card(420);
        var pauseTitle = Text("PAUSED", 30, _config.Text, true);
        pauseTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
        pauseCard.Add(pauseTitle);
        pauseCard.Add(Spacer(18));
        pauseCard.Add(UiButton("ゲームに戻る", _config.Cyan, () => ResumeRequested?.Invoke()));
        pauseCard.Add(Spacer(8));
        pauseCard.Add(UiButton("最初からやり直す", _config.Gold, () => RestartRequested?.Invoke()));
        pauseCard.Add(Spacer(8));
        pauseCard.Add(UiButton("終了", _config.Red, () => QuitRequested?.Invoke()));
        _pauseOverlay.Add(pauseCard);
        _root.Add(_pauseOverlay);
    }

    internal void Render(GameUiState state)
    {
        _phaseTitle.text = $"RYZECHØ  //  {state.PhaseTitle}";
        _score.text = $"{state.PlayerWins}  -  {state.EnemyWins}";
        _objectiveTitle.text = state.ObjectiveTitle;
        _objectiveBody.text = state.ObjectiveBody;
        _timerLabel.text = state.TimerLabel;
        _timerValue.text = state.TimerValue;
        _credits.text = $"{state.Credits}c";
        _weapon.text = $"{state.WeaponCode}  //  {state.WeaponDetail}";
        _agent.text = state.AgentDetail;
        _controls.text = state.Controls;
        _buildPoints.text = $"AVAILABLE AP  {state.BuildPoints}";
        _investment.text = $"選択中の投資  {state.SelectedInvestment}c";
        _skillPurchase.text = state.AgentSkillPurchased ? "エージェントスキル購入済み" : "エージェントスキル 400c";
        _briefingTitle.text = state.PhaseTitle;
        _briefingBody.text = state.ResultMessage;
        _resultTitle.text = state.Phase is GamePhase.Victory ? "VICTORY" : state.Phase is GamePhase.Defeat ? "DEFEAT" : "ROUND RESULT";
        _resultBody.text = state.ResultMessage;

        RebuildAbilities(state.Abilities);
        RebuildStatuses(state.StatusEffects);
        RebuildRoster(state.Actors);
        RebuildMinimap(state.Actors, state.SiteLabel);
        RebuildActivityFeed(state.ActivityFeed);
        RebuildChoices(_buildChoices, state.BuildTools, BuildToolSelected);
        RebuildChoices(_bossChoices, state.Bosses, BossSelected);
        RebuildChoices(_primaryChoices, state.PrimaryWeapons, PrimarySelected, compact: true);
        RebuildChoices(_sidearmChoices, state.Sidearms, SidearmSelected, compact: true);

        SetVisible(_buildPanel, state.Phase == GamePhase.Construct);
        SetVisible(_loadoutPanel, state.Phase == GamePhase.Bet);
        SetVisible(_briefing, state.ShowBriefing && state.Phase is GamePhase.Construct or GamePhase.Bet);
        SetVisible(_resultOverlay, state.Phase is GamePhase.RoundResult or GamePhase.Victory or GamePhase.Defeat);
        SetVisible(_pauseOverlay, state.IsPaused);
        _pauseOverlay.BringToFront();
    }

    internal bool IsPointerOverInteractiveElement(UnityVector2 screenPosition)
    {
        if (_root.panel == null)
        {
            return false;
        }

        var panelPosition = RuntimePanelUtils.ScreenToPanel(_root.panel, screenPosition);
        var current = _root.panel.Pick(panelPosition);
        while (current != null)
        {
            if (current.ClassListContains("blocks-world") || current is Button)
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }

    public void Dispose()
    {
        _root.Clear();
    }

    private VisualElement BuildConstructPanel()
    {
        var panel = Column("build-panel");
        panel.AddToClassList("blocks-world");
        panel.style.position = Position.Absolute;
        panel.style.left = 282;
        panel.style.right = 208;
        panel.style.bottom = 42;
        panel.style.minHeight = 102;
        panel.style.paddingTop = 10;
        panel.style.paddingBottom = 10;
        panel.style.paddingLeft = 12;
        panel.style.paddingRight = 12;
        ApplyPanel(panel, _config.Background, _config.Cyan);

        var header = Row();
        var title = Text("CONSTRUCTION LOADOUT", 12, _config.Cyan, true);
        title.style.flexGrow = 1;
        _buildPoints = Text("AVAILABLE AP", 12, _config.Gold, true);
        header.Add(title);
        header.Add(_buildPoints);
        panel.Add(header);

        _buildChoices = Row();
        _buildChoices.style.flexWrap = Wrap.Wrap;
        panel.Add(_buildChoices);
        panel.Add(UiButton("配置を確定して投資へ  [Enter]", _config.Green, () => ConfirmRequested?.Invoke()));
        return panel;
    }

    private VisualElement BuildLoadoutPanel()
    {
        var panel = Column("loadout-panel");
        panel.AddToClassList("blocks-world");
        panel.style.position = Position.Absolute;
        panel.style.left = 282;
        panel.style.right = 208;
        panel.style.top = 122;
        panel.style.bottom = 42;
        panel.style.paddingTop = 12;
        panel.style.paddingBottom = 12;
        panel.style.paddingLeft = 14;
        panel.style.paddingRight = 14;
        ApplyPanel(panel, _config.Background, _config.Gold);

        var scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.flexGrow = 1;
        panel.Add(scroll);
        scroll.Add(Text("BOSS SELECT / INVESTMENT", 14, _config.Gold, true));
        _bossChoices = Column();
        scroll.Add(_bossChoices);

        var investRow = Row();
        _investment = Text("選択中の投資", 12, _config.Text, true);
        _investment.style.flexGrow = 1;
        investRow.Add(_investment);
        investRow.Add(UiButton("-25", _config.Red, () => InvestmentAdjusted?.Invoke(-25), 72));
        investRow.Add(UiButton("+25", _config.Green, () => InvestmentAdjusted?.Invoke(25), 72));
        scroll.Add(investRow);

        scroll.Add(Spacer(10));
        scroll.Add(Text("PRIMARY WEAPON", 13, _config.Cyan, true));
        _primaryChoices = Row();
        _primaryChoices.style.flexWrap = Wrap.Wrap;
        scroll.Add(_primaryChoices);

        scroll.Add(Spacer(8));
        scroll.Add(Text("SIDEARM", 13, _config.Cyan, true));
        _sidearmChoices = Row();
        _sidearmChoices.style.flexWrap = Wrap.Wrap;
        scroll.Add(_sidearmChoices);

        scroll.Add(Spacer(8));
        var agentRow = Row();
        _skillPurchase = Text("エージェントスキル 400c", 11, _config.Text, true);
        _skillPurchase.style.flexGrow = 1;
        agentRow.Add(UiButton("エージェント切替  [6]", _config.Purple, () => AgentCycleRequested?.Invoke(), 190));
        agentRow.Add(_skillPurchase);
        agentRow.Add(UiButton("スキル購入  [5]", _config.Green, () => SkillPurchaseRequested?.Invoke(), 150));
        scroll.Add(agentRow);

        panel.Add(UiButton("ロードアウトを確定して出撃  [Enter]", _config.Gold, () => ConfirmRequested?.Invoke()));
        return panel;
    }

    private void RebuildChoices(VisualElement root, IReadOnlyList<GameUiChoice> choices, Action<int>? selected, bool compact = false)
    {
        root.Clear();
        foreach (var choice in choices)
        {
            var button = UiButton($"[{choice.Code}]  {choice.Name}\n{choice.Detail}", choice.Selected ? _config.Cyan : _config.Border,
                () => selected?.Invoke(choice.Index), compact ? 168 : 210);
            button.style.height = compact ? 52 : 58;
            button.style.marginRight = 6;
            button.style.marginBottom = 6;
            button.style.opacity = choice.Enabled ? 1f : 0.5f;
            if (choice.Selected)
            {
                button.style.backgroundColor = new UnityColor(0.08f, 0.2f, 0.25f, 1f);
            }
            root.Add(button);
        }
    }

    private void RebuildAbilities(IReadOnlyList<GameUiAbility> abilities)
    {
        _abilityRow.Clear();
        foreach (var ability in abilities)
        {
            var slot = Column();
            slot.style.width = 70;
            slot.style.height = 48;
            slot.style.marginRight = 6;
            slot.style.paddingTop = 4;
            slot.style.paddingLeft = 5;
            ApplyPanel(slot, _config.PanelRaised, ability.Ready ? _config.Green : _config.Border);
            slot.Add(Text($"[{ability.Key}] {ability.Name}", 8, ability.Ready ? _config.Green : _config.MutedText, true));
            var bar = new VisualElement();
            bar.style.height = 4;
            bar.style.marginTop = 5;
            bar.style.backgroundColor = _config.Border;
            var fill = new VisualElement();
            fill.style.height = 4;
            fill.style.width = new Length(ability.Progress * 100f, LengthUnit.Percent);
            fill.style.backgroundColor = ability.Ready ? _config.Green : _config.Cyan;
            bar.Add(fill);
            slot.Add(bar);
            _abilityRow.Add(slot);
        }
    }

    private void RebuildStatuses(IReadOnlyList<string> statuses)
    {
        _statusRow.Clear();
        foreach (var status in statuses)
        {
            var label = Text(status, 8, status.Contains("EXPOSED") ? _config.Red : _config.Gold, true);
            label.style.marginTop = 5;
            label.style.marginRight = 6;
            _statusRow.Add(label);
        }
    }

    private void RebuildRoster(IReadOnlyList<GameUiActor> actors)
    {
        _roster.Clear();
        _roster.Add(Text("TEAM STATUS", 10, _config.MutedText, true));
        foreach (var actor in actors)
        {
            if (actor.Team == "ENEMY" && actor.IsAlive)
            {
                continue;
            }
            if (actor.Team == "ENEMY")
            {
                continue;
            }

            var row = Row();
            row.style.height = 28;
            row.style.marginTop = 4;
            row.style.paddingLeft = 7;
            row.style.paddingRight = 7;
            ApplyPanel(row, _config.Panel, actor.IsAlive ? _config.Green : _config.Border);
            var name = Text(actor.IsBoss ? $"◆ {actor.Name}" : actor.Name, 9, actor.IsAlive ? _config.Text : _config.MutedText, actor.IsBoss);
            name.style.flexGrow = 1;
            row.Add(name);
            row.Add(Text($"HP {actor.HealthRatio * 100f:0}%  SH {actor.ShieldRatio * 100f:0}%", 8, _config.MutedText));
            _roster.Add(row);
        }
    }

    private void RebuildMinimap(IReadOnlyList<GameUiActor> actors, string siteLabel)
    {
        _minimapDots.Clear();
        var site = Text(siteLabel, 10, _config.Gold, true);
        site.style.position = Position.Absolute;
        site.style.left = 70;
        site.style.top = 78;
        _minimapDots.Add(site);

        foreach (var actor in actors)
        {
            if (!actor.IsAlive)
            {
                continue;
            }
            var dot = new VisualElement();
            var size = actor.IsBoss ? 10f : 7f;
            dot.style.position = Position.Absolute;
            dot.style.left = 6 + actor.MapX * (MinimapSize - 18);
            dot.style.top = 16 + actor.MapY * (MinimapSize - 28);
            dot.style.width = size;
            dot.style.height = size;
            dot.style.backgroundColor = actor.Team == "ENEMY" ? _config.Red : _config.Green;
            dot.style.borderTopLeftRadius = size;
            dot.style.borderTopRightRadius = size;
            dot.style.borderBottomLeftRadius = size;
            dot.style.borderBottomRightRadius = size;
            _minimapDots.Add(dot);
        }
    }

    private void RebuildActivityFeed(IReadOnlyList<string> messages)
    {
        _activityFeed.Clear();
        _activityFeed.Add(Text("ACTIVITY", 10, _config.Cyan, true));
        foreach (var message in messages)
        {
            var label = Text($"> {message}", 9, _config.Text);
            label.style.marginTop = 4;
            label.style.whiteSpace = WhiteSpace.Normal;
            _activityFeed.Add(label);
        }
    }

    private void ConfigureRoot(VisualElement root)
    {
        root.style.position = Position.Absolute;
        root.style.left = 0;
        root.style.top = 0;
        root.style.right = 0;
        root.style.bottom = 0;
        root.style.fontSize = 12 * _config.UiScale;
        root.pickingMode = PickingMode.Ignore;
    }

    private VisualElement Overlay(string name)
    {
        var overlay = new VisualElement { name = name };
        overlay.AddToClassList("blocks-world");
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0;
        overlay.style.top = 0;
        overlay.style.right = 0;
        overlay.style.bottom = 0;
        overlay.style.alignItems = Align.Center;
        overlay.style.justifyContent = Justify.Center;
        overlay.style.backgroundColor = new UnityColor(0f, 0.01f, 0.02f, 0.84f);
        return overlay;
    }

    private VisualElement Card(float width)
    {
        var card = Column();
        card.style.width = width;
        card.style.maxWidth = new Length(88, LengthUnit.Percent);
        card.style.paddingTop = 24;
        card.style.paddingBottom = 24;
        card.style.paddingLeft = 24;
        card.style.paddingRight = 24;
        ApplyPanel(card, _config.Background, _config.Cyan);
        return card;
    }

    private Button UiButton(string text, UnityColor accent, Action action, float minWidth = 0)
    {
        var button = new Button(action) { text = text };
        button.AddToClassList("blocks-world");
        button.style.minHeight = 36;
        if (minWidth > 0) button.style.width = minWidth;
        button.style.paddingLeft = 10;
        button.style.paddingRight = 10;
        button.style.whiteSpace = WhiteSpace.Normal;
        button.style.unityTextAlign = TextAnchor.MiddleCenter;
        button.style.unityFontStyleAndWeight = FontStyle.Bold;
        button.style.color = _config.Text;
        ApplyPanel(button, _config.PanelRaised, accent);
        return button;
    }

    private static VisualElement Row(string? name = null)
    {
        var element = new VisualElement { name = name };
        element.style.flexDirection = FlexDirection.Row;
        element.style.alignItems = Align.Center;
        return element;
    }

    private static VisualElement Column(string? name = null)
    {
        var element = new VisualElement { name = name };
        element.style.flexDirection = FlexDirection.Column;
        return element;
    }

    private static Label Text(string text, float size, UnityColor color, bool bold = false)
    {
        var label = new Label(text);
        label.style.fontSize = size;
        label.style.color = color;
        if (bold) label.style.unityFontStyleAndWeight = FontStyle.Bold;
        return label;
    }

    private static VisualElement Spacer(float height)
    {
        var spacer = new VisualElement();
        spacer.style.height = height;
        return spacer;
    }

    private static void ApplyPanel(VisualElement element, UnityColor background, UnityColor border)
    {
        element.style.backgroundColor = background;
        element.style.borderTopWidth = 1;
        element.style.borderRightWidth = 1;
        element.style.borderBottomWidth = 1;
        element.style.borderLeftWidth = 1;
        element.style.borderTopColor = border;
        element.style.borderRightColor = border;
        element.style.borderBottomColor = border;
        element.style.borderLeftColor = border;
        element.style.borderTopLeftRadius = 4;
        element.style.borderTopRightRadius = 4;
        element.style.borderBottomLeftRadius = 4;
        element.style.borderBottomRightRadius = 4;
    }

    private static void SetVisible(VisualElement element, bool visible)
    {
        element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
