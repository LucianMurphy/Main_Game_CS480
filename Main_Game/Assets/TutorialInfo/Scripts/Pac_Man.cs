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
    private GameObject[] ghosts;

    [Header("Terrain Setup")]
    public LayerMask groundLayer;
    public float heightOffset = 0.5f;

    [Header("Animation")]
    [Tooltip("Drag Assets/#NVJOB Slender/Slender/Prefab/Slender.prefab here.")]
    public Object riggedVisualPrefab;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");

    private Rigidbody rb;
    private Animator locomotionAnimator;
    private Vector3 lastAnimPosition;
    private bool hasSpeedParameter;
  
   

    
        


    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        if (statusText != null) statusText.text = "<b><color=black>Objective:</color></b> <color=green>Hunt Pac-Man</color>";

        SetupLocomotionAnimation();
        lastAnimPosition = transform.position;
    }

    void Update()
    {
        if (!isStunned)
        {
            thinkTimer += Time.deltaTime;
            if (thinkTimer >= 0.15f)
            {
                ghosts = GameObject.FindGameObjectsWithTag("Ghost");
                currentDir = GetBestMove();
                thinkTimer = 0f;
            }

            MoveContinuous();
        }

        UpdateLocomotionAnimation();
    }

    public void Stun(float duration)
    {
        StartCoroutine(StunRoutine(duration));
    }

    private Vector3 GetTerrainPos(Vector3 targetPos)
    {
        Vector3 rayStart = new Vector3(targetPos.x, targetPos.y + 1f, targetPos.z);
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
        if(Physics.Raycast(transform.position, directionToGhost, out RaycastHit hit, distanceToGhost, wallLayer))
        {
            return false;
        }
        return true;
    }

    void SetupLocomotionAnimation()
    {
        Animation legacyAnimation = GetComponent<Animation>();
        if (legacyAnimation != null)
            Destroy(legacyAnimation);

        SkinnedMeshRenderer brokenRootSkin = GetComponent<SkinnedMeshRenderer>();
        if (brokenRootSkin != null)
            Destroy(brokenRootSkin);

        if (!HasValidSkinnedRig())
            TrySpawnRiggedVisual();

        locomotionAnimator = ResolveLocomotionAnimator();
        if (locomotionAnimator == null)
            return;

        locomotionAnimator.applyRootMotion = false;
        locomotionAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        hasSpeedParameter = HasAnimatorParameter(locomotionAnimator, SpeedParam, AnimatorControllerParameterType.Float);
        locomotionAnimator.SetFloat(SpeedParam, 0f);
    }

    void TrySpawnRiggedVisual()
    {
        if (riggedVisualPrefab == null)
            return;

        GameObject visual = SpawnPrefabAsGameObject(riggedVisualPrefab);
        if (visual == null)
            return;

        visual.name = riggedVisualPrefab.name.Replace(".prefab", "") + "_Visual";
        visual.transform.SetParent(transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        foreach (SlenderExample slenderExample in visual.GetComponentsInChildren<SlenderExample>())
            slenderExample.enabled = false;

        foreach (AudioSource childAudio in visual.GetComponentsInChildren<AudioSource>())
            childAudio.enabled = false;

        MeshRenderer staticMesh = GetComponent<MeshRenderer>();
        if (staticMesh != null)
            staticMesh.enabled = false;
    }

    static GameObject SpawnPrefabAsGameObject(Object prefabAsset)
    {
        if (prefabAsset == null)
            return null;

        Object instance = Object.Instantiate(prefabAsset);
        if (instance is GameObject gameObject)
            return gameObject;

        if (instance is Component component)
            return component.gameObject;

        if (instance != null)
            Object.Destroy(instance);

        Debug.LogError(
            $"Rigged Visual Prefab must be a GameObject prefab (use Assets/#NVJOB Slender/Slender/Prefab/Slender.prefab). Got: {prefabAsset.name} ({prefabAsset.GetType().Name})",
            prefabAsset);
        return null;
    }

    bool HasValidSkinnedRig()
    {
        foreach (SkinnedMeshRenderer skinnedMesh in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (!skinnedMesh.gameObject.activeInHierarchy)
                continue;

            if (skinnedMesh.bones != null && skinnedMesh.bones.Length > 0)
                return true;
        }
        return false;
    }

    Animator ResolveLocomotionAnimator()
    {
        Animator rootAnimator = GetComponent<Animator>();

        foreach (Animator candidate in GetComponentsInChildren<Animator>(true))
        {
            if (candidate.runtimeAnimatorController == null)
                continue;

            if (candidate.gameObject != gameObject)
            {
                if (rootAnimator != null)
                    rootAnimator.enabled = false;

                MeshRenderer staticMesh = GetComponent<MeshRenderer>();
                if (staticMesh != null)
                    staticMesh.enabled = false;

                return candidate;
            }
        }

        return rootAnimator != null && rootAnimator.runtimeAnimatorController != null ? rootAnimator : null;
    }

    void UpdateLocomotionAnimation()
    {
        if (locomotionAnimator == null)
            return;

        float locomotionSpeed = 0f;
        if (!isStunned)
        {
            float moved = (transform.position - lastAnimPosition).magnitude;
            locomotionSpeed = moved > 0.02f ? 1f : 0f;
        }

        lastAnimPosition = transform.position;

        if (hasSpeedParameter)
        {
            locomotionAnimator.SetFloat(SpeedParam, locomotionSpeed);
            return;
        }

        int targetState = locomotionSpeed > 0f
            ? Animator.StringToHash("Walk")
            : Animator.StringToHash("Idle");

        if (!locomotionAnimator.HasState(0, targetState))
            return;

        if (locomotionAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash != targetState)
            locomotionAnimator.CrossFade(targetState, 0.12f);
    }

    static bool HasAnimatorParameter(Animator animator, int hash, AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == hash && parameter.type == type)
                return true;
        }
        return false;
    }

    void MoveContinuous()
    {
        Vector3 rayStart = transform.position + (Vector3.up * 0.2f);

        if (!Physics.Raycast(rayStart, currentDir, 0.55f, wallLayer))
        {
            Vector3 nextPos = transform.position + currentDir * speed * Time.deltaTime;
            transform.position = GetTerrainPos(nextPos);
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
            Vector3 castStart = transform.position + (Vector3.up * 0.2f);
            if (Physics.SphereCast(castStart, 0.3f, dir, out RaycastHit hit, 0.6f, wallLayer))
            {
                continue;
            }

            float score = BetterEvaluationScore(transform.position + dir);

            if (Vector3.Dot(dir, currentDir) < -0.9f) score -= 20f;
            if (dir == currentDir) score += 0.1f; // Lowered momentum bias

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
        
        if (!isGhostScared && pacPanicing)
        {
            if (seeGhost)
            {
                score -= 50.0f / (nearestGhostPathDist + 1.0f);
            }
            if (nearestGhostPathDist <= 2f)
            {
                score -= 1000.0f / (nearestGhostPathDist + 0.1f);
            }
        }

        if (isGhostScared && nearestScaredGhostPathDist < 999f)
        {
            score += 500.0f / (nearestScaredGhostPathDist + 1.0f);
        }

        // ORIGINAL FOOD LOGIC WITH FALLBACK
        if (pellets.Length > 0)
        {
            float pelletDist = PacAStarToTag(pos, "Pellet");
            if (pelletDist >= 999f)
            {
                pelletDist = Vector3.Distance(pos, pellets[0].transform.position);
            }
            score += 40.0f / (pelletDist + 1.0f); 
        }

        // score -= 10f * pips.Length;
        if (pips.Length > 0 && !pacPanicing)
        {
            float foodDist = PacAStarToTag(pos, "Pip");
            if (foodDist >= 999f)
            {
                foodDist = Vector3.Distance(pos, pips[0].transform.position);
            }
            score += 1.0f / (foodDist + 1.0f); 
        }

        return score;
    }

    // ORIGINAL GHOST A* (Physics Based)
    float PacAStarToTarget(Vector3 startPos, Vector3 targetPos)
    {
        Vector2Int start = new Vector2Int(Mathf.RoundToInt(startPos.x), Mathf.RoundToInt(startPos.z));
        Vector2Int goal = new Vector2Int(Mathf.RoundToInt(targetPos.x), Mathf.RoundToInt(targetPos.z));
        
        var openSet = new PriorityQueue<Vector2Int, float>();
        var gScore = new Dictionary<Vector2Int, float>();
        
        openSet.Enqueue(start, 0);
        gScore[start] = 0;

        int limit = 0;
        while (openSet.Count > 0 && limit < 1000) 
        {
            limit++;
            Vector2Int current = openSet.Dequeue();

            if (current == goal) return gScore[current];

            foreach (Vector2Int d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighbor = current + d;
                Vector3 flatWorld = new Vector3(neighbor.x, transform.position.y, neighbor.y);

                if (!Physics.CheckSphere(flatWorld, 0.45f, wallLayer))
                {
                    float tentG = gScore[current] + 1;
                    if (!gScore.ContainsKey(neighbor) || tentG < gScore[neighbor])
                    {
                        gScore[neighbor] = tentG;
                        float h = Mathf.Abs(neighbor.x - goal.x) + Mathf.Abs(neighbor.y - goal.y);
                        openSet.Enqueue(neighbor, tentG + h);
                    }
                }
            }
        }
        return 999f;
    }

    // ORIGINAL FOOD A* (Physics Based)
    private Collider[] detectionBuffer = new Collider[10];
    float PacAStarToTag(Vector3 startPos, string tag)
    {
        Vector2Int start = new Vector2Int(Mathf.RoundToInt(startPos.x), Mathf.RoundToInt(startPos.z));
        var openSet = new PriorityQueue<Vector2Int, float>();
        var gScore = new Dictionary<Vector2Int, float>();
        openSet.Enqueue(start, 0);
        gScore[start] = 0;

        int limit = 0;
        while (openSet.Count > 0 && limit < 200) 
        {
            limit++;
            Vector2Int current = openSet.Dequeue();

            Vector3 flatCheckPos = new Vector3(current.x, transform.position.y, current.y);

            int numColliders = Physics.OverlapSphereNonAlloc(flatCheckPos, 0.45f, detectionBuffer);
            
            for (int i = 0; i < numColliders; i++)
            {
                if (detectionBuffer[i].CompareTag(tag))
                {
                    return gScore[current];
                }
            }

            foreach (Vector2Int d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighbor = current + d;
                Vector3 flatWorld = new Vector3(neighbor.x, transform.position.y, neighbor.y);
                
                if (!Physics.CheckSphere(flatWorld, 0.45f, wallLayer))
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