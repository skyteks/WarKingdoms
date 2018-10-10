using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    public enum GameMode
    {
        Gameplay,
        Cutscene,
    }

    public GameMode gameMode = GameMode.Gameplay;
    public UnitTemplate.Faction faction;

    private Platoon selectedPlatoon;
    private PlayableDirector activeDirector;

    public UnityEvent updatedPlatoon;

    private void Awake()
    {
        selectedPlatoon = GetComponent<Platoon>();
        Cursor.lockState = CursorLockMode.Confined;
#if UNITY_EDITOR
        Application.targetFrameRate = 30; //just to keep things "smooth" during presentations
#endif
    }

    public void IssueCommand(AICommand cmd)
    {
        selectedPlatoon.ExecuteCommand(cmd);
    }

    public int GetSelectionLength()
    {
        return selectedPlatoon.units.Count;
    }

    public Transform[] GetSelectionTransforms()
    {
        return selectedPlatoon.units.Select(x => x.transform).ToArray();
    }

    public Unit[] GetSelectionUnits()
    {
        return selectedPlatoon.units.ToArray();
    }

    public void AddToSelection(Unit[] newSelectedUnits)
    {
        selectedPlatoon.AddUnits(newSelectedUnits);
        for (int i = 0; i < newSelectedUnits.Length; i++)
        {
            newSelectedUnits[i].SetSelected(true);
        }

        updatedPlatoon.Invoke();
    }

    public void AddToSelection(Unit newSelectedUnit)
    {
        selectedPlatoon.AddUnit(newSelectedUnit);
        newSelectedUnit.SetSelected(true);

        updatedPlatoon.Invoke();
    }

    public void RemoveFromSelection(Unit u)
    {
        selectedPlatoon.RemoveUnit(u);
        u.SetSelected(false);

        updatedPlatoon.Invoke();
    }

    public void ClearSelection()
    {
        for (int i = 0; i < selectedPlatoon.units.Count; i++)
        {
            selectedPlatoon.units[i].SetSelected(false);
        }

        selectedPlatoon.Clear();

        updatedPlatoon.Invoke();
    }

    public void SentSelectedUnitsTo(Vector3 pos)
    {
        AICommand newCommand = new AICommand(AICommand.CommandType.GoToAndGuard, pos);
        IssueCommand(newCommand);
    }

    public void AttackTarget(Unit tgtUnit)
    {
        AICommand newCommand = new AICommand(AICommand.CommandType.AttackTarget, tgtUnit);
        IssueCommand(newCommand);
    }

    public Unit[] GetAllSelectableUnits()
    {
        return FindObjectsOfType<Unit>().Where(unit => unit.template.faction == faction).ToArray();//GameObject.FindGameObjectsWithTag("Locals").Select(x => x.GetComponent<Unit>()).ToArray();
    }

    //Called by the TimeMachine Clip (of type Pause)
    public void PauseTimeline(PlayableDirector whichOne)
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
}
