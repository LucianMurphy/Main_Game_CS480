using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private Vector3 currentDir = Vector3.forward;
    private float thinkTimer = 0f;
    private bool isStunned = false;

    private float lastSeenGhost = 0f;
    private float pelletWeight = 0f;
    private GameObject[] ghosts;

    [Header("Terrain Setup")]
    public LayerMask groundLayer;
    public float heightOffset = 0.5f;

    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        if (statusText != null) statusText.text = "<b><color=black>Objective:</color></b> <color=green>Hunt Pac-Man</color>";
    }

    void Update()
    {
        if (isStunned) return;

        thinkTimer += Time.deltaTime;
        if (thinkTimer >= 0.1f)
        {
            ghosts = GameObject.FindGameObjectsWithTag("Ghost");
            currentDir = GetBestMove();
            thinkTimer = 0f;
        }

        MoveContinuous();
    }

    public void Stun(float duration)
    {
        StartCoroutine(StunRoutine(duration));
    }

    private Vector3 GetTerrainPos(Vector3 targetPos)
    {
        Vector3 rayStart = new Vector3(targetPos.x, targetPos.y + 5f, targetPos.z);
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 10f, groundLayer))
        {
            return new Vector3(targetPos.x, hit.point.y + heightOffset, targetPos.z);
        }
        return targetPos;
    }
    private System.Collections.IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    bool CanSeeGhost(GameObject Ghost)
    {
        Vector3 directionToGhost = Ghost.transform.position - transform.position;
        float distanceToGhost = directionToGhost.magnitude;
        //checks to see if there is a wall in the way
        if(Physics.Raycast(transform.position, directionToGhost, 
                            out RaycastHit hit, distanceToGhost, wallLayer))
        {
            return false;
        }
        return true;

    }
    void MoveContinuous()
    {
        Vector3 rayStart = transform.position + (Vector3.up * 0.2f);

        if (!Physics.Raycast(rayStart, currentDir, 0.55f, wallLayer))
        {
            Vector3 targetFlatPos = transform.position + currentDir * speed * Time.deltaTime;
            
            Vector3 targetTerrainPos = GetTerrainPos(targetFlatPos);

            Vector3 nextPos = new Vector3(targetFlatPos.x, targetTerrainPos.y, targetFlatPos.z);
            
            float verticalClimbSpeed = speed * 2.5f;
            nextPos.y = Mathf.MoveTowards(transform.position.y, targetTerrainPos.y, verticalClimbSpeed * Time.deltaTime);

            transform.position = nextPos;
            
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
            // hopefully wont run into walls with this code
            Vector3 castStart = transform.position + (Vector3.up * 0.2f);
            if (Physics.SphereCast(castStart, 0.3f, dir, out RaycastHit hit, 0.6f, wallLayer))
            {
                continue;
            }

            float score = BetterEvaluationScore(transform.position + dir);

            // stops a bug where he just dances intead of moving 
            if (Vector3.Dot(dir, currentDir) < -0.9f) score -= 20f;
            // changed this to .1 instead of 1 to help looping when not finding food 
            if (dir == currentDir) score += 0.1f;

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
        GameObject[] pellets = GameObject.FindGameObjectsWithTag("Pellet");

        bool seeGhost = false;
        float nearestGhostPathDist = 999f;
        float nearestScaredGhostPathDist = 999f;

        foreach (GameObject g in ghosts)
        {
            if (g == null) continue;
            
            // This prevents Pac-Man from staring at a ghost through a wall
            float pathDist = PacAStarToTarget(pos, g.transform.position);

            if (!isGhostScared)
            {
                if(CanSeeGhost(g))
                {
                    seeGhost = true;
                    lastSeenGhost = Time.time;
                    if (pathDist < nearestGhostPathDist) 
                    {
                        nearestGhostPathDist = pathDist;
                    }
                } 
            }
            else
            {
                if (pathDist < nearestScaredGhostPathDist) 
                {
                    nearestScaredGhostPathDist = pathDist;
                }
            }
        }
        bool pacPanicing = (Time.time - lastSeenGhost) <= 4.0f;
        // run from ghost when its not scared 
        if (!isGhostScared && pacPanicing)
        {
                if (seeGhost)
                {
                    score -= 10.0f / (nearestGhostPathDist + 1.0f);
                }
                if (nearestGhostPathDist < 4f)
                {
                    return float.NegativeInfinity;
                }
                score -= 10.0f / (nearestGhostPathDist + 1.0f);

        }
        // if (!isGhostScared && nearestGhostPathDist < 4f) 
        // {
        //     return float.NegativeInfinity;
        // }
        // // This is commented out just in case 
        // if (!isGhostScared) 
        // {
        //     score -= 10.0f / (nearestGhostPathDist + 1.0f);
        // }
        // chase ghost when it is scared 
        if (isGhostScared && nearestScaredGhostPathDist < 999f)
        {
            score += 500.0f / (nearestScaredGhostPathDist + 1.0f);
        }

        // get pellet
        if (pellets.Length > 0)
        {
            float pelletDist = PacAStarToTag(pos, "Pellet");
            score += 40.0f / (pelletDist + 1.0f); 
        }
        //broken code need to fix after playtest 
        // get food 
        score -= 10f * pips.Length;
        if (pips.Length > 0)
        {
            float foodDist = PacAStarToTag(pos, "Pip");
            score += 1.0f / (foodDist + 1.0f); 
        }

        return score;
    }

    // A* that looks for ghosts 
    float PacAStarToTarget(Vector3 startPos, Vector3 targetPos)
    {
        Vector3Int start = new Vector3Int(Mathf.RoundToInt(startPos.x), Mathf.RoundToInt(startPos.z), Mathf.RoundToInt(startPos.z));
        Vector3Int goal = new Vector3Int(Mathf.RoundToInt(targetPos.x), Mathf.RoundToInt(targetPos.z), Mathf.RoundToInt(targetPos.z));
        
        var openSet = new PriorityQueue<Vector3Int, float>();
        var gScore = new Dictionary<Vector3Int, float>();
        
        openSet.Enqueue(start, 0);
        gScore[start] = 0;

        int limit = 0;
        Vector3Int[] directions = { Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right};
        while (openSet.Count > 0 && limit < 300) 
        {
            limit++;
            Vector3Int current = openSet.Dequeue();

            if (Vector3Int.Distance(current, goal) < 1) return gScore[current];

            foreach (Vector3Int d in directions)
            {
                Vector3Int flatNeighbor = current + d;

                Vector3 rayStart = new Vector3(flatNeighbor.x, current.y + 1.5f, flatNeighbor.z);

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit groundHit, 3.0f, groundLayer))
                {
                    Vector3Int neighbor = new Vector3Int(flatNeighbor.x,Mathf.RoundToInt(groundHit.point.y), flatNeighbor.z);
                    float heightDiff = Mathf.Abs(neighbor.y - current.y);
                    if (heightDiff <= 2.0f)
                    {
                        Vector3 wallCheckPos = new Vector3(neighbor.x, groundHit.point.y + heightOffset, neighbor.z);
                        if(!Physics.CheckSphere(wallCheckPos, 0.45f, wallLayer))
                        {
                            float tentG = gScore[current] + 1;
                            if (!gScore.ContainsKey(neighbor) || tentG < gScore[neighbor])
                            {
                                gScore[neighbor] = tentG;
                                
                                // Upgraded to a 3D Manhattan Heuristic
                                float h = Mathf.Abs(neighbor.x - goal.x) + 
                                            Mathf.Abs(neighbor.y - goal.y) + 
                                            Mathf.Abs(neighbor.z - goal.z);
                                            
                                openSet.Enqueue(neighbor, tentG + h);
                            }
                        }
                    }
                }
            }
        }
        return 999f;
    }

    // A* that looks fore pellets and pips 
    private Collider[] detectionBuffer = new Collider[10];
    float PacAStarToTag(Vector3 startPos, string tag)
    {
        Vector3Int start = new Vector3Int(Mathf.RoundToInt(startPos.x), Mathf.RoundToInt(startPos.y), Mathf.RoundToInt(startPos.z));
        var openSet = new PriorityQueue<Vector3Int, float>();
        var gScore = new Dictionary<Vector3Int, float>();
        
        openSet.Enqueue(start, 0);
        gScore[start] = 0;

        int limit = 0;
        Vector3Int[] directions = { Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right };

        while (openSet.Count > 0 && limit < 1000) 
        {
            limit++;
            Vector3Int current = openSet.Dequeue();

            Vector3 checkPos = new Vector3(current.x, current.y + heightOffset, current.z);
            int numColliders = Physics.OverlapSphereNonAlloc(checkPos, 0.45f, detectionBuffer);
            
            for (int i = 0; i < numColliders; i++)
            {
                if (detectionBuffer[i].CompareTag(tag))
                {
                    return gScore[current];
                }
            }

            foreach (Vector3Int d in directions)
            {
                Vector3Int flatNeighbor = current + d;
                Vector3 rayStart = new Vector3(flatNeighbor.x, current.y + 1.5f, flatNeighbor.z);

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit groundHit, 3.0f, groundLayer))
                {
                    Vector3Int neighbor = new Vector3Int(flatNeighbor.x, Mathf.RoundToInt(groundHit.point.y), flatNeighbor.z);
                    float heightDiff = Mathf.Abs(neighbor.y - current.y);
                    
                    if (heightDiff <= 2.0f)
                    {
                        Vector3 wallCheckPos = new Vector3(neighbor.x, groundHit.point.y + heightOffset, neighbor.z);
                        if (!Physics.CheckSphere(wallCheckPos, 0.45f, wallLayer))
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
            }
        }
        return 999f;
    }
    
    //if eat pellet chase pacman
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
                if (wall != null)
                {
                    wall.tag = "escape_walls";
                }
            }

        }
        else if (other.CompareTag("Ghost"))
        {
            // Check if the player has a shield or invincibility active.
            PlayerStatus ps = other.GetComponent<PlayerStatus>();
            if (ps != null && ps.TryAbsorbHit()) return;

            GameManager gm = FindObjectOfType<GameManager>();
            if (gm == null) return;

            if (isGhostScared)
                gm.Lose();
            else
                gm.Win();
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