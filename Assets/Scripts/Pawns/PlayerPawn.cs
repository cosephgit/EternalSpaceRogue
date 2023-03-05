using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player Pawn
// DO NOT DELETE (carries the camera as a child)
// called by the StageManager when it is the player's turn and takes player input, these inputs are then passed to the parent PawnControllerBase to move the pawn etc

public class PlayerPawn : PawnControllerBase
{
    [SerializeField]private float moveSensitivity = 0.1f; // how much input is needed to accept a move request
    List<SquareIndicator> indicators;
    bool enteredActionState = false;
    int experienceLevel = 10;
    int experience = 0;
    bool stageComplete = false;

    void Start()
    {
        UpdateHealthBar();
        UpdateActionBar();
        UpdateXPBar();
        enteredActionState = false;
        indicators = new List<SquareIndicator>();
    }

    void UpdateHealthBar()
    {
        UIManager.instance.healthBar.UpdateHealth(health, healthMax);
    }

    void UpdateActionBar()
    {
        UIManager.instance.actionBar.UpdateHealth(movePoints, movePointsMax);
    }

    void UpdateXPBar()
    {
        UIManager.instance.xpBar.UpdateHealth(experience, experienceLevel);
    }

    public override void RoundPrep()
    {
        base.RoundPrep();
        enteredActionState = false;
        stageComplete = false;
        UpdateActionBar();
        PlaceMoveIndicators();
        UIManager.instance.instructions.ShowInstruction(Instruction.Move);
    }

    // player chooses to spend their action moving again
    void ActionRun()
    {
        movePoints = movePointsMax;
        UpdateActionBar();
        PlaceMoveIndicators();
        UIManager.instance.instructions.ShowInstruction(Instruction.Run);
    }

    // clears all currently displayed indicators
    void ClearIndicators()
    {
        for (int i = indicators.Count - 1; i >= 0; i--)
        {
            Destroy(indicators[i].gameObject);
        }
        indicators.Clear();
    }

    // this takes the current number of movement points and places movement indicators on all accessible spaces in range
    void PlaceMoveIndicators()
    {
        int totalPoints = movePoints + (moveActionDone ? 0 : movePointsMax);
        Collider2D[] nodes = Physics2D.OverlapCircleAll(transform.position, totalPoints, Global.LayerNav());

        foreach (Collider2D node in nodes)
        {
            Vector3 offset = (transform.position - node.transform.position);
            float dist = Mathf.Abs(offset.x) + Mathf.Abs(offset.y);

            // TODO add pathfinding check here
            if (dist <= totalPoints && dist > 0.1f)
            {
                SquareIndicator indicatorNew = Instantiate(PrefabProvider.inst.indicator, node.transform.position, Quaternion.identity);
                if (dist <= movePoints && !moveActionDone) // only show blue on the first move, always appear yellow on the second move
                    indicatorNew.InitIndicator(IndicatorType.Move);
                else // second move appears yellow to remind that you're not going to get an attack
                    indicatorNew.InitIndicator(IndicatorType.Run);
                indicators.Add(indicatorNew);
            }
        }
    }

    // places attack area indicators for currently selected weapon at currently selected range
    // TODO currently just does range 1
    void PlaceAttackIndicators()
    {
        if (attackFacing.magnitude > 0)
        {
            SquareIndicator indicatorAttack = Instantiate(PrefabProvider.inst.indicator, transform.position + attackFacing, Quaternion.identity);
            indicatorAttack.InitIndicator(IndicatorType.Attack);
            indicators.Add(indicatorAttack);
        }
    }

    void FlashAttackIndicators()
    {
        for (int i = indicators.Count - 1; i >= 0; i--)
        {
            // this call tells the indicator to destroy itself, so the reference must be removed right away
            indicators[i].Flash();
        }
    }

    protected override void PreAttack()
    {
        base.PreAttack();
        FlashAttackIndicators();
    }
    protected override void PostAttack()
    {
        base.PostAttack();
        ClearIndicators();
    }

    public override bool PawnUpdate()
    {
        if (moving) return false; // in the middle of a move, take no inputs

        if (movePoints == 0 && moveActionDone)
        {
            ClearIndicators();
            UIManager.instance.instructions.HideInstructions();
            return true;
        }

        if (movePoints > 0)
        {
            Vector3 move = new Vector3();

            if (Input.GetAxis("Horizontal") > moveSensitivity && CanMove(Vector3.right))
            {
                move.x = 1f;
            }
            else if (Input.GetAxis("Horizontal") < -moveSensitivity && CanMove(Vector3.left))
            {
                move.x = -1f;
            }
            else if (Input.GetAxis("Vertical") > moveSensitivity && CanMove(Vector3.up))
            {
                move.y = 1f;
            }
            else if (Input.GetAxis("Vertical") < -moveSensitivity && CanMove(Vector3.down))
            {
                move.y = -1f;
            }
            else if (Input.GetButtonDown("Confirm"))
            {
                // cancel remaining movement
                movePoints = 0;
                UpdateActionBar();
                ClearIndicators();
                return false;
            }
            else
            {
                // no move input
                // TODO check for cancel movement option
                return false;
            }

            movePoints--;
            UpdateActionBar();

            // some sort of valid move input has been received, start moving
            StartCoroutine(MovePosition(move));
        }
        else if (!enteredActionState)
        {
            enteredActionState = true;
            // place the player's current facing as the default attack direction
            ClearIndicators();
            PlaceAttackIndicators();
        }
        else
        {
            Vector3 tryAttack = Vector3.zero;
            UIManager.instance.instructions.ShowInstruction(Instruction.Attack);

            // handle the player action
            // attack
            if (Input.GetAxis("Horizontal") > moveSensitivity)
            {
                tryAttack.x = 1f;
            }
            else if (Input.GetAxis("Horizontal") < -moveSensitivity)
            {
                tryAttack.x = -1f;
            }
            else if (Input.GetAxis("Vertical") > moveSensitivity)
            {
                tryAttack.y = 1f;
            }
            else if (Input.GetAxis("Vertical") < -moveSensitivity)
            {
                tryAttack.y = -1f;
            }
            else if (Input.GetButtonDown("Confirm"))
            {
                // player wants to attack!
                moveActionDone = true;
                StartCoroutine(Attack());
                return false;
            }
            // TODO take inputs and move indicator
            // run
            else if (Input.GetButtonDown("Fire1"))
            {
                moveActionDone = true;
                ClearIndicators();
                ActionRun();
                return false;
            }

            if (tryAttack.magnitude > 0)
            {
                // player has entered an attack direction preference!
                if (attackFacing == tryAttack)
                {
                    // player has selected the same direction as currently facing
                    // later this will allow you to adjust range
                }
                else
                {
                    // change the current facing to the entered direction
                    ClearIndicators();
                    attackFacing = tryAttack;
                    PlaceAttackIndicators();
                }
            }
        }


        return false;
    }

    protected override void PostMove()
    {
        ClearIndicators();
        PlaceMoveIndicators();
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        UpdateHealthBar();
    }

    public void AddXP(int amount)
    {
        experience += amount;
        while (experience >= experienceLevel)
        {
            Debug.Log("LEVEL UP!");
            experience -= experienceLevel;
        }
        UpdateXPBar();
    }

    protected override void Death()
    {
        base.Death();
        Debug.Log("YOU DIED!");
    }

    public void ObjectiveReached(int xp)
    {
        AddXP(xp);
        stageComplete = true;
        Debug.Log("STAGE COMPLETE!");
    }
}
