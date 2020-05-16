using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// All arround game state manager
/// </summary>
public class GameManager : Singleton<GameManager>
{
    public enum GameMode
    {
        Gameplay,
        Cutscene,
    }

#if UNITY_EDITOR
    public bool editorFrameRateLock30 = true;
    [Space]
#endif
    public GameMode gameMode = GameMode.Gameplay;

    [Preview]
    public FactionTemplate playerFaction;
    public List<FactionTemplate> factions;


    public Platoon selectedPlatoon { get; private set; }
    private UnityEngine.Playables.PlayableDirector activeDirector;

    private bool showHealthbars;

    void Awake()
    {
        selectedPlatoon = GetComponent<Platoon>();
        Cursor.lockState = CursorLockMode.Confined;
#if UNITY_EDITOR
        if (editorFrameRateLock30)
        {
            Application.targetFrameRate = 30;//just to keep things "smooth" during presentations
        }
#endif
        if (playerFaction == null)
        {
            Debug.LogError("No player faction set", this);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (FactionTemplate faction in factions)
        {
            faction.units.Clear();
        }
    }

    public void IssueCommand(AICommand cmd, bool followUpCommand)
    {
        selectedPlatoon.ExecuteCommand(cmd, followUpCommand);
    }

    public int GetSelectionLength()
    {
        return selectedPlatoon.units.Count;
    }

    public Transform[] GetSelectionTransforms()
    {
        return selectedPlatoon.units.Select(x => x.transform).ToArray();
    }

    public List<Unit> GetSelectionUnits()
    {
        return selectedPlatoon.units;
    }

    public bool IsInsideSelection(Unit newSelectedUnit)
    {
        return selectedPlatoon.IncludesUnit(newSelectedUnit);
    }

    public void AddToSelection(IList<Unit> newSelectedUnits)
    {
        foreach (Unit newSelectedUnit in newSelectedUnits)
        {
            AddToSelection(newSelectedUnit);
        }
    }

    public bool AddToSelection(Unit newSelectedUnit)
    {
        if (selectedPlatoon.IncludesUnit(newSelectedUnit))
        {
            return false;
        }

        selectedPlatoon.AddUnit(newSelectedUnit);
        newSelectedUnit.SetSelected(true);
        UIManager.Instance.AddToSelection(newSelectedUnit);
        return true;
    }

    public void SetSelection(IList<Unit> newSelectedUnits)
    {
        ClearSelection();
        AddToSelection(newSelectedUnits);
    }

    public void SetSelection(Unit newSelectedUnit)
    {
        ClearSelection();
        AddToSelection(newSelectedUnit);
    }

    public void RemoveFromSelection(Unit unitToRemove)
    {
        selectedPlatoon.RemoveUnit(unitToRemove);
        unitToRemove.SetSelected(false);
        UIManager.Instance.RemoveFromSelection(unitToRemove);
    }

    public void ClearSelection()
    {
        foreach (Unit unit in selectedPlatoon.units)
        {
            unit.SetSelected(false);
        }

        selectedPlatoon.Clear();

        UIManager.Instance.ClearSelection();
    }

    public void MoveSelectedUnitsTo(Vector3 pos, bool followUpCommand)
    {
        AICommand newCommand = new AICommand(AICommand.CommandType.MoveTo, pos);
        IssueCommand(newCommand, followUpCommand);
    }

    public void AttackMoveSelectedUnitsTo(Vector3 pos, bool followUpCommand)
    {
        AICommand newCommand = new AICommand(AICommand.CommandType.AttackMoveTo, pos);
        IssueCommand(newCommand, followUpCommand);
    }

    public void AttackTarget(Unit tgtUnit, bool followUpCommand)
    {
        AICommand newCommand = new AICommand(AICommand.CommandType.AttackTarget, tgtUnit);
        IssueCommand(newCommand, followUpCommand);
    }

    public List<Unit> GetAllUnits()
    {
        return Unit.globalUnitsList;
    }

    public List<Unit> GetAllVisibleUnits()
    {
        return Unit.globalUnitsList.Where(unit => unit.visible).ToList();
    }

    //Called by the TimeMachine Clip (of type Pause)
    public void PauseTimeline(UnityEngine.Playables.PlayableDirector whichOne)
    {
        activeDirector = whichOne;
        activeDirector.Pause();
        gameMode = GameMode.Cutscene; //InputManager will be waiting for a spacebar to resume
    }

    //Called by the InputManager
    public void ResumeTimeline()
    {
        activeDirector.Resume();
        gameMode = GameMode.Gameplay;
    }

    public void QuitGame()
    {
        Debug.Log("quit game");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
