using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public static PlayerControl instance;

    public enum ControlState { Player0_Leads, Player1_Leads }
    public ControlState controlState = ControlState.Player0_Leads;

    public PlayerMovementsControl player0;
    public PlayerMovementsControl player1;

    void Awake()
    {
        instance = this;
        SetControlState(controlState);
    }

    void Update()
    {
        if (controlState == ControlState.Player0_Leads)
        {
            player0.MoveFromInput();
            player1.MirrorMove(player0.GetLastInput());
        }
        else
        {
            player1.MoveFromInput();
            player0.MirrorMove(player1.GetLastInput());
        }
    }

    public void SwitchControl()
    {
        controlState = (controlState == ControlState.Player0_Leads)
            ? ControlState.Player1_Leads
            : ControlState.Player0_Leads;

        SetControlState(controlState);
    }

    private void SetControlState(ControlState state)
    {
        player0.isControlled = (state == ControlState.Player0_Leads);
        player1.isControlled = (state == ControlState.Player1_Leads);
    }
}
