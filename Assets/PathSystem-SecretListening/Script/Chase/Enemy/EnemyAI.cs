
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public Transform[] curveControlPoints; // Her devriye noktasý çifti için bir kontrol noktasý
    public float viewRadius = 10f;
    public float viewAngle = 120f;
    public float chaseDistance = 15f;
    public float reloadDistance = 3f;
    public float patrolSpeed = 3f;
    public float chaseSpeed = 6f;
    public float bezierTolerance = 1f; // Bezier yolundaki bir noktaya ulaþýldýðýnda, bir sonraki noktaya geçiþ için tolerans

    private int currentPoint = 0;
    [SerializeField]private bool isChasing = false;
    private NavMeshAgent agent;
    private List<Vector3> bezierPath = new List<Vector3>(); // Bezier noktalarýný tutan liste
    public Transform ChaseObject;

    public LayerMask targetLayer;

    public GameObject parentCurve;
    private PlayerHideController hide;
    void Start()
    {
        if(ChaseObject == null)
        {
            Debug.LogError("Set Chase Object or target Layer in this object :" + gameObject.name);
        }

        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        // Bezier yolunu hesapla ve listeye kaydet
        CalculateBezierPath();
        hide = ChaseObject.GetComponent<PlayerHideController>();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, ChaseObject.position);

        if (CanSeePlayer() && distanceToPlayer < viewRadius && !hide.isHide)
        {
            StartChasing();
        }

        else if (isChasing && distanceToPlayer > chaseDistance)
        {
            StopChasing();
        }

        if (distanceToPlayer < reloadDistance)
        {
            ReloadScene();
        }

        if (!isChasing)
        {
            Patrol();
        }
    }

    void OnValidate()
    {
        // Sadece patrolPoints'ler deðiþtiðinde kontrol noktalarýný oluþtur
        CreateControlPoints();
        // Her seferinde bezier yolunu yeniden hesapla
        CalculateBezierPath();
    }

    public void CreateControlPoints()
    {
        // Eðer devriye noktalarý varsa, her nokta çifti arasýnda bir kontrol noktasý oluþtur
        if (patrolPoints.Length > 1)
        {
            if (curveControlPoints == null || curveControlPoints.Length != patrolPoints.Length)
            {
                curveControlPoints = new Transform[patrolPoints.Length];
            }
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                // Kontrol noktasý zaten var mý kontrol et
                if (curveControlPoints[i] == null)
                {
                    GameObject controlPointObj = new GameObject("CurveControlPoint_" + i);
                    parentCurve.transform.position = Vector3.zero;
                    controlPointObj.transform.SetParent(parentCurve.transform);
                    controlPointObj.transform.position = (patrolPoints[i].position + patrolPoints[(i + 1) % patrolPoints.Length].position) / 2;
                    curveControlPoints[i] = controlPointObj.transform;
                }
            }
        }
    }

    void Patrol()
    {
        // Eðer bezier yolunu henüz oluþturmadýysak veya boþsa geri dön
        if (bezierPath == null || bezierPath.Count == 0) return;

        if (Vector3.Distance(transform.position, bezierPath[currentPoint]) < bezierTolerance)
        {
            currentPoint = (currentPoint + 1) % bezierPath.Count; // Dairesel devriye
        }

        agent.SetDestination(bezierPath[currentPoint]);
    }

    void CalculateBezierPath()
    {
        bezierPath.Clear();

        if (patrolPoints.Length > 1)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Vector3 startPoint = patrolPoints[i].position;
                Vector3 endPoint = patrolPoints[(i + 1) % patrolPoints.Length].position;
                Vector3 controlPoint = curveControlPoints[i].position;

                int curveDetail = 20; // Eðriyi kaç parçaya böleceðimizi belirler
                for (int j = 0; j <= curveDetail; j++)
                {
                    float t = j / (float)curveDetail;
                    Vector3 pointOnCurve = CalculateQuadraticBezierPoint(t, startPoint, controlPoint, endPoint);

                    // Bezier noktalarýný listeye ekle
                    bezierPath.Add(pointOnCurve);
                }
            }
        }
    }

    void StartChasing()
    {
        isChasing = true;
        agent.speed = chaseSpeed;
        agent.SetDestination(ChaseObject.position); 
    }

    void StopChasing()
    {
        isChasing = false;
        agent.speed = patrolSpeed;
    }

    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (ChaseObject.position - transform.position).normalized;
        float angleBetweenEnemyAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);

        if (angleBetweenEnemyAndPlayer < viewAngle / 2)
        {
            if (RaycastHitPlayer(dirToPlayer))
            {
                return true;
            }

        }
        return false;
    }

    private bool RaycastHitPlayer(Vector3 dirPlayer)
    {
        Ray ray = new Ray(transform.position, dirPlayer);
        RaycastHit hit;

        Debug.DrawRay(transform.position, dirPlayer * viewRadius, Color.red);

        if (Physics.Raycast(ray, out hit ,viewRadius, targetLayer))
        {
            if (hit.collider.CompareTag("Player"))
                return true;
        }
        return false;
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // Bezier eðrisini hesaplayan fonksiyon
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 point = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        return point;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

        if (patrolPoints.Length > 1)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Vector3 startPoint = patrolPoints[i].position;
                Vector3 endPoint = patrolPoints[(i + 1) % patrolPoints.Length].position;
                Vector3 controlPoint = curveControlPoints[i].position;

                int curveDetail = 20;
                Vector3 previousPoint = startPoint;

                for (int j = 1; j <= curveDetail; j++)
                {
                    float t = j / (float)curveDetail;
                    Vector3 pointOnCurve = CalculateQuadraticBezierPoint(t, startPoint, controlPoint, endPoint);

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(previousPoint, pointOnCurve);

                    previousPoint = pointOnCurve;
                }

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(controlPoint, 0.2f);
            }
        }
    }
}
