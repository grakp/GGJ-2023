using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AiController : MonoBehaviour // MonoBehaviourPun, IPunInstantiateMagicCallback
{
    // Components
    private Rigidbody2D rb;
    private BaseUnit self;

    // Enemy
    public float walkSpeed = 4f;
    public float internalClockSec = 2f;
    public float chanceToNotMove = 0.3f;

    // Drag and Drop the targets
    public AiTriggerStateController aggroStateController;
    public AiTriggerStateController combatStateController;

    // The existing state
    private AiState _current;
    private Coroutine _coroutine;

    public enum AiState {
        MOVE,
        AGGRESSIVE,
        COMBAT,
    }

    // State Machine
    // MOVE <--> AGGRESSIVE <--> COMBAT

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        self = GetComponent<BaseUnit>();
        _current = AiState.MOVE;
        _coroutine = StartCoroutine(MoveState());
    }

    void Update() {
        if(_current == AiState.MOVE) {
            GameObject potentialAggro = aggroStateController.GetClosestTargetInRange();
            if(potentialAggro != null) {
                EnterAggressiveState(potentialAggro);
            }
        } else if(_current == AiState.AGGRESSIVE || _current == AiState.COMBAT) {
            GameObject potentialCombat = combatStateController.GetClosestTargetInRange();
            GameObject potentialAggro = aggroStateController.GetClosestTargetInRange();
            if(potentialCombat != null) {
                EnterCombatState(potentialCombat);
            } else if(potentialAggro != null) {
                EnterAggressiveState(potentialAggro);
            } else {
                EnterMoveState();
            }
        }
    }

    // Look in a random direction
    IEnumerator MoveState() {
        while(true) {
            if(Random.Range(0f, 1f) < chanceToNotMove) {
                self.SetDirection(new Vector2(0, 0));
            }
            else {
                self.SetDirection(new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f)).normalized);
            }
            rb.velocity = self.dir * walkSpeed;
            yield return new WaitForSeconds(internalClockSec);
        }
    }

    // Keep chasing a target
    IEnumerator AggressiveState(GameObject target) {
        while(true) {
            self.SetDirection((target.transform.position - transform.position).normalized);
            rb.velocity = self.dir * walkSpeed;
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator CombatState(GameObject target) {
        while(true) {
            self.SetDirection((target.transform.position - transform.position).normalized);
            self.UseAbility("Swing");            
            yield return new WaitForFixedUpdate();
        }
    }

    // Called when there are no more targets in the aggressive state
    public void EnterMoveState() {
        // Stop the existing coroutine
        StopCoroutine(_coroutine);
        rb.velocity = Vector2.zero;
        _coroutine = StartCoroutine(MoveState());
        _current = AiState.MOVE;
    }

    // Called when there are one or more targets in the aggressive state
    public void EnterAggressiveState(GameObject target) {
        // Stop the existing coroutine
        StopCoroutine(_coroutine);
        rb.velocity = Vector2.zero;
        _coroutine = StartCoroutine(AggressiveState(target));
        _current = AiState.AGGRESSIVE;
    }

    // Called when there are targets within attaxk range
    public void EnterCombatState(GameObject target) {
        // Stop the existing coroutine
        StopCoroutine(_coroutine);
        rb.velocity = Vector2.zero;
        _coroutine = StartCoroutine(CombatState(target));
        _current = AiState.COMBAT;
    }
}
