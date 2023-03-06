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
    bool weaponReady = false;

    void Start()
    {
        if (weaponEquipped) weaponReady = true;
        UIManager.instance.weaponManager.UpdateFist(weaponUnarmed.title);
        UIManager.instance.weaponManager.ReadyFist(false);
        UpdateHealthBar();
        UpdateActionBar();
        UpdateXPBar();
        UpdateAmmoBar();
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

    void UpdateAmmoBar()
    {
        int ammoNow = 0, ammoMax = 1;
        if (weaponEquipped)
        {
            if (weaponEquipped.ammoMax > 0)
            {
                ammoNow = weaponEquipped.ammo;
                ammoMax = weaponEquipped.ammoMax;
            }

            UIManager.instance.weaponManager.UpdateWeapon(weaponEquipped.title);
        }
        else
        {
            if (weaponReady)
                weaponReady = false;
            UIManager.instance.weaponManager.UpdateWeapon("");
        }
        UIManager.instance.weaponManager.ReadyFist(!weaponReady);
        UIManager.instance.weaponManager.UpdateAmmo(ammoNow, ammoMax);
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
        int totalPoints = movePoints; // + (moveActionDone ? 0 : movePointsMax); // cut these to reduce path finding frame time
        Collider2D[] nodes = Physics2D.OverlapCircleAll(transform.position, totalPoints, Global.LayerNav());

        for (int i = 0; i < nodes.Length; i++)
        {
            int dist = Global.OrthogonalDist(transform.position, nodes[i].transform.position);
            bool clear = !Physics2D.OverlapPoint(nodes[i].transform.position, Global.LayerPawn());
            if (clear && dist > 0)
            {
                // only do expensive path finding if it's a possible point to reach
                dist = StageManager.instance.Pathfind(transform.position, nodes[i].transform.position, false, totalPoints).Count;

                // TODO add pathfinding check here
                if (dist <= totalPoints && dist > 0)
                {
                    SquareIndicator indicatorNew = Instantiate(PrefabProvider.inst.indicator, nodes[i].transform.position, Quaternion.identity);
                    if (dist <= movePoints && !moveActionDone) // only show blue on the first move, always appear yellow on the second move
                        indicatorNew.InitIndicator(IndicatorType.Move);
                    else // second move appears yellow to remind that you're not going to get an attack
                        indicatorNew.InitIndicator(IndicatorType.Run);
                    indicators.Add(indicatorNew);
                }
            }
        }
    }

    // places attack area indicators for currently selected weapon at currently selected range
    // TODO currently just does range 1
    void PlaceAttackIndicators()
    {
        if (attackFacing.magnitude > 0)
        {
            Vector3[] indicatorPoints;

            if (weaponReady) indicatorPoints = weaponEquipped.GetHitLocations(transform.position, attackFacing, attackRange);
            else indicatorPoints = weaponUnarmed.GetHitLocations(transform.position, attackFacing, 1);

            for (int i = 0; i < indicatorPoints.Length; i++)
            {
                SquareIndicator indicatorAttack = Instantiate(PrefabProvider.inst.indicator, indicatorPoints[i], Quaternion.identity);
                indicatorAttack.InitIndicator(IndicatorType.Attack);
                indicators.Add(indicatorAttack);
            }
        }
    }

    void FlashAttackIndicators()
    {
        for (int i = indicators.Count - 1; i >= 0; i--)
        {
            indicators[i].Flash();
        }
    }

    protected override WeaponBase WeaponSelected()
    {
        if (weaponReady)
            return weaponEquipped;
        return weaponUnarmed;
    }

    protected override void PreAttack()
    {
        base.PreAttack();
        FlashAttackIndicators();
        if (weaponReady) UpdateAmmoBar(); // no need if using fist
    }
    protected override void PostAttack()
    {
        base.PostAttack();
        ClearIndicators();
    }


    bool PlayerMove()
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
        else if (Input.GetButtonDown("Fire3"))
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
            return false;
        }

        // check for powerups in the target space
        Vector3 posCheck = transform.position + move;
        Collider2D[] collisions = Physics2D.OverlapPointAll(posCheck, Global.LayerPower());
        bool clear = true;
        if (collisions.Length > 0)
        {
            for (int i = 0; i < collisions.Length; i++)
            {
                PowerupCrate powerCollision = collisions[i].GetComponent<PowerupCrate>();
                if (powerCollision)
                {
                    // check for crates first as this could stop movement
                    if (!powerCollision.TouchPowerup(this))
                    {
                        // an enemy has spawned in the target space!
                        clear = false;
                    }
                }
            }
            if (clear)
            {
                // space is clear, now activate all powerups in the space
                for (int i = 0; i < collisions.Length; i++)
                {
                    PowerUpBase powerCollision = collisions[i].GetComponent<PowerUpBase>();
                    if (powerCollision)
                    {
                        powerCollision.TouchPowerup(this);
                    }
                }
            }
        }

        // always spend a movement point even if the space is blocked
        movePoints--;
        UpdateActionBar();

        if (clear)
        {
            // some sort of valid move input has been received, start moving
            StartCoroutine(MovePosition(move));
        }
        else
        {
            StartCoroutine(AimDelay());
            ClearIndicators();
            PlaceMoveIndicators();
        }
        return false;
    }

    bool PlayerAttack()
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
        // switch between fist and weapon
        else if (Input.GetButtonDown("Fire1"))
        {
            // only switch if either weapon is ready OR weapon is not ready and there is a weapon to ready
            if (weaponReady || (!weaponReady && weaponEquipped))
            {
                weaponReady = !weaponReady;
                attackRange = 1;
                ClearIndicators();
                StartCoroutine(AimDelay());
                PlaceAttackIndicators();
                UpdateAmmoBar();
            }
            return false;
        }
        else if (Input.GetButtonDown("Fire2"))
        {
            if (weaponEquipped)
            {
                weaponEquipped.DiscardWeapon();
                weaponReady = false;
            }
            #if UNITY_EDITOR
            else
            {
                // cheat!
                weaponEquipped = Instantiate(weaponOptions.Select());
                weaponEquipped.EquipWeapon(this);
                weaponReady = true;
            }
            #endif
            ClearIndicators();
            UpdateAmmoBar();
            PlaceAttackIndicators();
        }
        // cancel attack and run instead
        else if (Input.GetButtonDown("Fire3"))
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
                if (weaponEquipped && attackRange < weaponEquipped.rangeMax)
                {
                    Collider2D obstacle = Physics2D.OverlapPoint(transform.position + (attackRange * attackFacing), Global.LayerObstacle());
                    if (obstacle)
                    {
                        // if the currently targeted space is blocked, that means all spaces behind the target are also blocked
                        return false;
                    }
                    ClearIndicators();
                    attackRange++;
                    PlaceAttackIndicators();
                    StartCoroutine(AimDelay());
                }
            }
            else
            {
                if (attackFacing == -tryAttack && weaponEquipped && attackRange > weaponEquipped.rangeMin)
                {
                    // the desired facing is opposite to the current facing - instead of changing direction, reduce the range
                    ClearIndicators();
                    attackRange--;
                    PlaceAttackIndicators();
                    StartCoroutine(AimDelay());
                }
                else
                {
                    // change the current facing to the entered direction
                    ClearIndicators();
                    attackFacing = tryAttack;
                    attackRange = 1;
                    PlaceAttackIndicators();
                    StartCoroutine(AimDelay());
                }
            }
        }
        return false;
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
            return PlayerMove();
        }
        else if (!enteredActionState)
        {
            enteredActionState = true;
            ClearIndicators();
            PlaceAttackIndicators();
        }
        else
        {
            return PlayerAttack();
        }


        return false;
    }

    // this is a simple coroutine to slow down aim adjustments 
    IEnumerator AimDelay()
    {
        moving = true;
        yield return new WaitForSeconds(0.1f);
        moving = false;
    }

    protected override void PostMove()
    {
        ClearIndicators();
        PlaceMoveIndicators();
    }

    public bool Heal(int amount)
    {
        if (health < healthMax)
        {
            health = Mathf.Min(health + amount, healthMax);
            UpdateHealthBar();
            return true;
        }
        return false;
    }

    public bool Reload()
    {
        if (weaponEquipped)
        {
            if (weaponEquipped.ammoMax > 0 && weaponEquipped.ammo < weaponEquipped.ammoMax)
            {
                weaponEquipped.Reload();
                UpdateAmmoBar();
                return true;
            }
        }
        return false;
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
            Debug.Log("<color=blue>INFO</color> LEVEL UP!");
            experience -= experienceLevel;
        }
        UpdateXPBar();
    }

    public bool HasWeapon()
    {
        return weaponEquipped;
    }

    public bool LowHealth()
    {
        if (health < healthMax * 0.5f)
        {
            return true;
        }
        return false;
    }

    protected override void Death()
    {
        base.Death();
        Debug.Log("<color=blue>INFO</color> YOU DIED!");
        StageManager.instance.PlayerDefeated();
    }

    // this is called when a player picks up a weapon
    public bool PickupWeapon(WeaponBase weaponPick)
    {
        if (weaponEquipped)
        {
            return false;
        }
        else
        {
            weaponEquipped = weaponPick;
            weaponEquipped.EquipWeapon(this);
            weaponReady = true;
            UpdateAmmoBar();
            return true;
        }
    }

    // this is called when a player unequips a weapon (either by choice or by the weapon requesting to be unequipped after running out of ammo)
    public override void UnequipWeapon(WeaponBase weaponUnequip)
    {
        if (weaponEquipped == weaponUnequip)
        {
            base.UnequipWeapon(weaponUnequip);
            weaponReady = false;
            UpdateAmmoBar();
        }
    }
}
