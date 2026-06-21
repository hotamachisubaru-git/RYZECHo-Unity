using System;
using UnityEngine;
using UnityVector2 = UnityEngine.Vector2;

namespace RYZECHo.UI;

internal sealed class GameUIManager : IDisposable
{
    private readonly GameModel _model;
    private readonly GameUI _view;

    internal GameUIManager(GameModel model, GameUI view)
    {
        _model = model;
        _view = view;
        _view.BuildToolSelected += _model.UiSelectBuildTool;
        _view.BossSelected += _model.UiSelectBoss;
        _view.PrimarySelected += _model.UiSelectPrimaryWeapon;
        _view.SidearmSelected += _model.UiSelectSidearm;
        _view.InvestmentAdjusted += _model.UiAdjustInvestment;
        _view.AgentCycleRequested += _model.UiCycleAgent;
        _view.SkillPurchaseRequested += _model.UiPurchaseAgentSkill;
        _view.ConfirmRequested += _model.UiConfirmPhase;
        _view.ResumeRequested += Resume;
        _view.RestartRequested += _model.UiRestartCampaign;
        _view.QuitRequested += Quit;
    }

    internal void Update()
    {
        _view.Render(_model.GetUiState());
    }

    internal bool IsPointerOverInteractiveElement(UnityVector2 screenPosition)
    {
        return _view.IsPointerOverInteractiveElement(screenPosition);
    }

    public void Dispose()
    {
        _view.BuildToolSelected -= _model.UiSelectBuildTool;
        _view.BossSelected -= _model.UiSelectBoss;
        _view.PrimarySelected -= _model.UiSelectPrimaryWeapon;
        _view.SidearmSelected -= _model.UiSelectSidearm;
        _view.InvestmentAdjusted -= _model.UiAdjustInvestment;
        _view.AgentCycleRequested -= _model.UiCycleAgent;
        _view.SkillPurchaseRequested -= _model.UiPurchaseAgentSkill;
        _view.ConfirmRequested -= _model.UiConfirmPhase;
        _view.ResumeRequested -= Resume;
        _view.RestartRequested -= _model.UiRestartCampaign;
        _view.QuitRequested -= Quit;
        _view.Dispose();
    }

    private void Resume()
    {
        _model.IsPaused = false;
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
