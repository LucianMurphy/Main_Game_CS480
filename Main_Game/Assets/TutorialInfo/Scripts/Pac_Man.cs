using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PacAI : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 5.0f;
    public LayerMask wallLayer;
    public bool isGhostScared = false;

    private Vector3 currentDir = Vector3.forward;
    private float thinkTimer = 0f;
    private GameObject[] ghosts;

    void Start()
    {
        ghosts = GameObject.FindGameObjectsWithTag("Ghost");
    }

    void Update()
    {
        thinkTimer += Time.deltaTime;
        if (thinkTimer >= 0.1f)
        {
            ghosts = GameObject.FindGameObjectsWithTag("Ghost");
            currentDir = GetBestMove();
            thinkTimer = 0f;
        }

        MoveContinuous();
    }

    void MoveContinuous()
    {
        // Thin ray stop-check
        if (!Physics.Raycast(transform.position, currentDir, 0.55f, wallLayer))
        {
            transform.position += currentDir * speed * Time.deltaTime;
        }
        else 
        {
            currentDir = GetBestMove();
        }

        if (currentDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(currentDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.deltaTime);
        }
    }

    Vector3 GetBestMove()
    {
        Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        Vector3 bestDir = currentDir;
        float bestScore = float.NegativeInfinity;

        foreach (Vector3 dir in dirs)
        {
            // Physical clearance check
            if (Physics.SphereCast(transform.position, 0.3f, dir, out RaycastHit hit, 0.6f, wallLayer))
            {
                continue;
            }

            float score = BetterEvaluationScore(transform.position + dir);

            // Penalize 180s heavily to stop the jitter/dancing
            if (Vector3.Dot(dir, currentDir) < -0.9f) score -= 20f;
            if (dir == currentDir) score += 1f;

            if (score > bestScore)
            {
                bestScore = score;
                bestDir = dir;
            }
        }
        return bestDir;
    }

    float BetterEvaluationScore(Vector3 pos)
    {
        float score = 0;
        GameObject[] pips = GameObject.FindGameObjectsWithTag("Pip");
        
        float nearestGhostPathDist = 999f;
        float nearestScaredGhostPathDist = 999f;

        foreach (GameObject g in ghosts)
        {
            if (g == null) continue;
            
            // CRITICAL FIX: Use A* Path distance for ghosts, not straight lines
            // This prevents Pac-Man from staring at a ghost through a wall
            float pathDist = PacAStarToTarget(pos, g.transform.position);

            if (!isGhostScared)
            {
                if (pathDist < nearestGhostPathDist) nearestGhostPathDist = pathDist;
            }
            else
            {
                if (pathDist < nearestScaredGhostPathDist) nearestScaredGhostPathDist = pathDist;
            }
        }

        // 1. GHOST DANGER (Path-aware)
        if (!isGhostScared && nearestGhostPathDist < 2f) return float.NegativeInfinity;
        if (!isGhostScared) score -= 10.0f / (nearestGhostPathDist + 1.0f);

        // 2. GHOST HUNTING (Path-aware)
        // If the ghost is scared, this is the #1 priority
        if (isGhostScared && nearestScaredGhostPathDist < 999f)
        {
            score += 500.0f / (nearestScaredGhostPathDist + 1.0f);
        }

        // 3. FOOD (Path-aware)
        score -= 10f * pips.Length;
        if (pips.Length > 0)
        {
            float foodDist = PacAStarToTag(pos, "Pip");
            score += 20.0f / (foodDist + 1.0f); 
        }

        return score;
    }

    // A* that looks for a specific world position (for ghosts)
    float PacAStarToTarget(Vector3 startPos, Vector3 targetPos)
    {
        Vector2Int start = new Vector2Int(Mathf.RoundToInt(startPos.x), Mathf.RoundToInt(startPos.z));
        Vector2Int goal = new Vector2Int(Mathf.RoundToInt(targetPos.x), Mathf.RoundToInt(targetPos.z));
        
        var openSet = new PriorityQueue<Vector2Int, float>();
        var gScore = new Dictionary<Vector2Int, float>();
        
        openSet.Enqueue(start, 0);
        gScore[start] = 0;

        int limit = 0;
        while (openSet.Count > 0 && limit < 200) 
        {
            limit++;
            Vector2Int current = openSet.Dequeue();

            if (current == goal) return gScore[current];

            foreach (Vector2Int d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighbor = current + d;
                Vector3 nWorld = new Vector3(neighbor.x, transform.position.y, neighbor.y);

                if (!Physics.CheckSphere(nWorld, 0.45f, wallLayer))
                {
                    float tentG = gScore[current] + 1;
                    if (!gScore.ContainsKey(neighbor) || tentG < gScore[neighbor])
                    {
                        gScore[neighbor] = tentG;
                        // Manhattan heuristic for A*
                        float h = Mathf.Abs(neighbor.x - goal.x) + Mathf.Abs(neighbor.y - goal.y);
                        openSet.Enqueue(neighbor, tentG + h);
                    }
                }
            }
        }
        return 999f;
    }

    // A* that looks for a tag (for food)
    float PacAStarToTag(Vector3 startPos, string tag)
    {
        Vector2Int start = new Vector2Int(Mathf.RoundToInt(startPos.x), Mathf.RoundToInt(startPos.z));
        var openSet = new PriorityQueue<Vector2Int, float>();
        var gScore = new Dictionary<Vector2Int, float>();
        openSet.Enqueue(start, 0);
        gScore[start] = 0;

        int limit = 0;
        while (openSet.Count > 0 && limit < 150) 
        {
            limit++;
            Vector2Int current = openSet.Dequeue();
            Vector3 checkPos = new Vector3(current.x, transform.position.y, current.y);

            if (Physics.OverlapSphere(checkPos, 0.45f).Any(h => h.CompareTag(tag)))
                return gScore[current];

            foreach (Vector2Int d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighbor = current + d;
                Vector3 nWorld = new Vector3(neighbor.x, transform.position.y, neighbor.y);
                if (!Physics.CheckSphere(nWorld, 0.45f, wallLayer))
                {
                    float tentG = gScore[current] + 1;
                    if (!gScore.ContainsKey(neighbor) || tentG < gScore[neighbor])
                    {
                        gScore[neighbor] = tentG;
                        openSet.Enqueue(neighbor, tentG);
                    }
                }
            }
        }
        return 999f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pip")) Destroy(other.gameObject);
        else if (other.CompareTag("Pellet"))
        {
            Destroy(other.gameObject);
            isGhostScared = true;
        }
    }

    
}

public class PriorityQueue<TElement, TPriority> where TPriority : System.IComparable<TPriority>
{
    private List<(TElement Element, TPriority Priority)> elements = new List<(TElement, TPriority)>();
    public int Count => elements.Count;
    public void Enqueue(TElement element, TPriority priority)
    {
        elements.Add((element, priority));
        elements.Sort((x, y) => x.Priority.CompareTo(y.Priority));
    }
    public TElement Dequeue() { var item = elements[0].Element; elements.RemoveAt(0); return item; }
}