//--- New Code ---
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitActionPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject panelRoot;

    [SerializeField]
    private TMP_Text unitInfoText;

    [SerializeField]
    private Button clearForestButton;

    [SerializeField]
    private Button buildColonyButton;

    [SerializeField]
    private Button enterColonyButton;

    [SerializeField]
    private Button endTurnButton;

    [SerializeField]
    private Button closeTownButton;

    private GameManager gameMgr;
    private UIManager uiMgr;

    void Start()
    {
        gameMgr = GameManager.instance;
        uiMgr = UIManager.instance;

        clearForestButton.onClick.AddListener(ClearForest);
        buildColonyButton.onClick.AddListener(BuildColony);
        enterColonyButton.onClick.AddListener(EnterColony);
        endTurnButton.onClick.AddListener(EndTurn);
        closeTownButton.onClick.AddListener(CloseTown);
    }

    void Update()
    {
        RefreshPanel();
    }

    private LandUnit GetCurLandUnit()
    {
        Unit unit = gameMgr.CurUnit;

        if (unit == null || unit.UnitType != UnitType.Land)
            return null;

        return (LandUnit)unit;
    }

    private bool CanClearForest(LandUnit unit)
    {
        if (unit == null || unit.UnitStatus != UnitStatus.None)
            return false;

        Hex hex = unit.CurHex;

        if (!hex.HasForest || hex.HexType == HexType.Ocean
            || hex.HexType == HexType.Hills || hex.HexType == HexType.Mountains)
            return false;

        return unit.ToolsNum >= 20;
    }

    private bool CanBuildColony(LandUnit unit)
    {
        if (unit == null || unit.UnitStatus != UnitStatus.None)
            return false;

        Hex hex = unit.CurHex;

        if (hex.HasTown || hex.HasForest || hex.HexType == HexType.Ocean || hex.HexType == HexType.Mountains)
            return false;

        return unit.ToolsNum >= 20;
    }

    private bool CanEnterColony()
    {
        Unit unit = gameMgr.CurUnit;

        if (unit == null || !unit.CurHex.HasTown || unit.CurHex.Town == null)
            return false;

        return unit.CurHex.Town.Faction == gameMgr.PlayerFaction;
    }

    private string GetHint(LandUnit unit)
    {
        if (unit == null)
            return "Ship: right-click an adjacent hex to sail";

        switch (unit.UnitStatus)
        {
            case UnitStatus.OnBoard:
                return "On board: right-click land to go ashore";
            case UnitStatus.Clearing:
                return "Clearing forest... done at End Turn";
            case UnitStatus.Building:
                return "Building colony... done at End Turn";
            case UnitStatus.WorkInField:
                return "Working in colony field";
        }

        if (unit.ToolsNum < 20)
            return "Needs 20 tools to clear / build";
        if (unit.CurHex.HasForest)
            return "Clear the forest before building";

        return "";
    }

    private void RefreshPanel()
    {
        Unit unit = gameMgr.CurUnit;
        LandUnit landUnit = GetCurLandUnit();
        bool canAct = gameMgr.PlayerTurn && !uiMgr.TownPanel.activeSelf;
        panelRoot.SetActive(!uiMgr.TownPanel.activeSelf);

        if (unit != null)
        {
            string tools = landUnit != null ? $"   Tools: {landUnit.ToolsNum}" : "";
            unitInfoText.text = $"Turn {gameMgr.GameTurn}\n"
                + $"<b>{unit.UnitName}</b>  [{unit.CurHex.HexName}]\n"
                + $"Moves: {unit.MovePoint}/{unit.MovePointMax}{tools}\n"
                + $"<size=80%>{GetHint(landUnit)}</size>";
        }
        else
            unitInfoText.text = $"Turn {gameMgr.GameTurn}";

        if (!gameMgr.PlayerTurn)
            unitInfoText.text += "\n<size=80%>Other nations are moving...</size>";

        clearForestButton.interactable = canAct && CanClearForest(landUnit);
        buildColonyButton.interactable = canAct && CanBuildColony(landUnit);
        enterColonyButton.interactable = canAct && CanEnterColony();
        endTurnButton.interactable = canAct;
    }

    private void ClearForest()
    {
        LandUnit unit = GetCurLandUnit();

        if (unit != null)
            unit.ClearingLand();
    }

    private void BuildColony()
    {
        LandUnit unit = GetCurLandUnit();

        if (unit != null)
            unit.BuildSettlement();
    }

    private void EnterColony()
    {
        if (CanEnterColony())
            gameMgr.SetupCurrentTown(gameMgr.CurUnit.CurHex.Town);
    }

    private void EndTurn()
    {
        gameMgr.Endturn();
    }

    private void CloseTown()
    {
        uiMgr.ToggleTownPanel(false);
    }
}
