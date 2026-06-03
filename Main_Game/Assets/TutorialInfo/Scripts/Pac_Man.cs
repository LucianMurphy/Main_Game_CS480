using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using TMPro;

public class PacAI : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 5.0f;
    public LayerMask wallLayer;
    public bool isGhostScared = false;

    [Header("Escape Wall Setup")]
    public GameObject[] specificEscapeWalls;
    public bool escape_walls = false;

    [Header("UI")]
    public TMP_Text statusText;

    [Header("AI Settings")]
    public float visionRange = 10f;
    public float fleeSearchRadius = 15f;
    public float replanInterval = 0.3f;

    private enum AIState { SeekingPellet, FleeingPlayer, HuntingPlayer }
    private AIState state = AIState.SeekingPellet;

    private NavMeshAgent agent;
    private bool isStunned = false;
    private float replanTimer = 0f;
    private bool wasGhostScared = false;
    private GameObject[] players;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        players = GameObject.FindGameObjectsWithTag("Ghost");
        wasGhostScared = isGhostScared;
        if (statusText != null) statusText.text = "<b><color=black>Objective:</color></b> <color=green>Hunt Pac-Man</color>";
        StartCoroutine(DelayedStart());
    }

    void Update()
    {
        if (isStunned) return;

        replanTimer += Time.deltaTime;

        bool scaredChanged = isGhostScared != wasGhostScared;

        if (state == AIState.FleeingPlayer)
        {
            bool playerGone = GetVisiblePlayer() == null;
            bool reachedFleePoint = agent.remainingDistance < 0.5f && !agent.pathPending;
            if (scaredChanged || playerGone || reachedFleePoint)
            {
                wasGhostScared = isGhostScared;
                replanTimer = 0f;
                Replan();
            }
            return;
        }

        if (scaredChanged || replanTimer >= replanInterval)
        {
            wasGhostScared = isGhostScared;
            replanTimer = 0f;
            Replan();
        }

        // Keep hunting destination current since the player moves every frame
        if (state == AIState.HuntingPlayer)
        {
            GameObject nearest = GetNearestPlayer();
            if (nearest != null)
                agent.SetDestination(nearest.transform.position);
        }
    }

    // ── Decision Making ───────────────────────────────────────────────────────

    void Replan()
    {
        if (!agent.isOnNavMesh) return;

        players = GameObject.FindGameObjectsWithTag("Ghost");
        Debug.Log($"[PacAI] Players found: {players.Length}");
        foreach (var p in players)
            Debug.Log($"[PacAI] Player: {p.name} dist:{Vector3.Distance(transform.position, p.transform.position):F1} visionRange:{visionRange}");

        if (isGhostScared)
        {
            state = AIState.HuntingPlayer;
            GameObject nearest = GetNearestPlayer();
            if (nearest != null)
                agent.SetDestination(nearest.transform.position);
            return;
        }

        GameObject visiblePlayer = GetVisiblePlayer();
        if (visiblePlayer != null)
        {
            state = AIState.FleeingPlayer;
            agent.SetDestination(FindFleeDestination(visiblePlayer.transform.position));
            return;
        }

        state = AIState.SeekingPellet;
        GameObject pellet = FindNearestTagged("Pellet");
        GameObject pip = FindNearestTagged("Pip");
        GameObject target = pellet ?? pip;
        if (target != null)
            agent.SetDestination(target.transform.position);
    }

    // ── Flee Destination ──────────────────────────────────────────────────────

    Vector3 FindFleeDestination(Vector3 threatPos)
    {
        Vector3 bestPoint = transform.position;
        float bestScore = -1f;

        // Sample 8 directions around the AI and pick the NavMesh point farthest from the threat
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            Vector3 candidate = transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * fleeSearchRadius;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, fleeSearchRadius, NavMesh.AllAreas))
            {
                float score = Vector3.Distance(hit.position, threatPos);
                if (score > bestScore) { bestScore = score; bestPoint = hit.position; }
            }
        }

        return bestPoint;
    }

    // ── Detection ─────────────────────────────────────────────────────────────

    GameObject GetVisiblePlayer()
    {
        foreach (GameObject p in players)
        {
            if (p == null) continue;
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist > visionRange) continue;
            return p;
        }
        return null;
    }

    GameObject GetNearestPlayer()
    {
        GameObject nearest = null;
        float nearestDist = float.MaxValue;
        foreach (GameObject p in players)
        {
            if (p == null) continue;
            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d < nearestDist) { nearestDist = d; nearest = p; }
        }
        return nearest;
    }

    GameObject FindNearestTagged(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        if (objects.Length == 0) return null;
        GameObject nearest = null;
        float nearestDist = float.MaxValue;
        foreach (GameObject obj in objects)
        {
            float d = Vector3.Distance(transform.position, obj.transform.position);
            if (d < nearestDist) { nearestDist = d; nearest = obj; }
        }
        return nearest;
    }

    // ── Stun ──────────────────────────────────────────────────────────────────

    private IEnumerator DelayedStart()
    {
        yield return null;
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            agent.Warp(hit.position);
        else
            agent.Warp(transform.position);
        yield return null;
        Replan();
    }

    public void Stun(float duration) => StartCoroutine(StunRoutine(duration));

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
        agent.isStopped = false;
    }

    // ── Triggers ──────────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pip"))
        {
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Pellet"))
        {
            Destroy(other.gameObject);
            isGhostScared = true;
            escape_walls = true;
            if (statusText != null) statusText.text = "<b><color=black>Objective:</color></b> <color=red>Escape or Stay and Collect</color>";
            foreach (GameObject wall in specificEscapeWalls)
            {
                if (wall != null) wall.tag = "escape_walls";
            }
        }
        else if (other.CompareTag("Ghost"))
        {
            PlayerStatus ps = other.GetComponent<PlayerStatus>();
            if (ps != null && ps.TryAbsorbHit()) return;

            GameManager gm = FindObjectOfType<GameManager>();
            if (gm == null) return;

            if (isGhostScared) gm.Lose();
            else gm.Win();
        }
    }
}
