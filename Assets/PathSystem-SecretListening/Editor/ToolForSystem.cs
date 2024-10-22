using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;

public class ToolForSystem : EditorWindow
{
    private GameObject Enemy;
    private GameObject PatrolListParent;
    GameObject _surface;
    private float viewRadius = 10f;
    private float viewAngle = 120f;
    private float chaseDistance = 15f;
    private float reloadDistance = 1.1f;
    private float patrolSpeed = 3f;
    private float chaseSpeed = 6f;
    private float bezierTolerance = 1f;

    private LayerMask layer;

    public Transform[] patrolPoints; 
    private ReorderableList patrolPointsList;

    private Vector3 SpawnPos;

    [MenuItem("Tools/System")]
    public static void ShowWindow()
    {
        GetWindow<ToolForSystem>("System");
    }

    private void OnEnable()
    {
        
        if (patrolPoints == null)
        {
            patrolPoints = new Transform[0];
        }

       
        patrolPointsList = new ReorderableList(patrolPoints, typeof(Transform), true, true, true, true);

        patrolPointsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            patrolPoints[index] = (Transform)EditorGUI.ObjectField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                $"Point {index + 1}",
                patrolPoints[index],
                typeof(Transform),
                true
            );
        };

        patrolPointsList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Patrol Points");
        };

        patrolPointsList.onAddCallback = (ReorderableList list) =>
        {
            ArrayUtility.Add(ref patrolPoints, null);
            patrolPointsList.list = patrolPoints;
        };

        patrolPointsList.onRemoveCallback = (ReorderableList list) =>
        {
            ArrayUtility.RemoveAt(ref patrolPoints, list.index);
            patrolPointsList.list = patrolPoints;
        };
    }

    private void OnGUI()
    {
        GUILayout.Label("System Menu", EditorStyles.boldLabel);

        Enemy = (GameObject)EditorGUILayout.ObjectField("Enemy", Enemy, typeof(GameObject), true);
        SpawnPos = EditorGUILayout.Vector3Field("Spawn Pos", SpawnPos);
        viewRadius = EditorGUILayout.FloatField("View Radius", viewRadius);
        viewAngle = EditorGUILayout.FloatField("View Angle", viewAngle);
        chaseDistance = EditorGUILayout.FloatField("Chase Distance", chaseDistance);
        reloadDistance = EditorGUILayout.FloatField("for Reload Scene Distance", reloadDistance);
        bezierTolerance = EditorGUILayout.FloatField("Path Tolerance", bezierTolerance);
        patrolSpeed = EditorGUILayout.FloatField("Patrol Speed", patrolSpeed);
        chaseSpeed = EditorGUILayout.FloatField("Chase Speed", chaseSpeed);

        layer = LayerMaskField("Select Layers", layer);

        patrolPointsList.DoLayoutList();
        PatrolListParent = (GameObject)EditorGUILayout.ObjectField("Curve List Parent Object", PatrolListParent, typeof(GameObject), true);

        if (GUILayout.Button("Set Everything") && patrolPoints.Length > 1 && Enemy != null && PatrolListParent != null)
        {
            _Set();
        }
        else
        {
            Debug.LogError("You have missing object in -> ToolForSystem");
        }
    }

    void _Set()
    {
        GameObject newEnemy = Instantiate(Enemy, SpawnPos, Quaternion.identity);

        newEnemy.AddComponent<EnemyAI>();
        newEnemy.AddComponent<NavMeshAgent>();

        #region SetScriptValues
        EnemyAI enemyAI = newEnemy.GetComponent<EnemyAI>();
        enemyAI.viewRadius = viewRadius;
        enemyAI.viewAngle = viewAngle;
        enemyAI.chaseDistance = chaseDistance;
        enemyAI.reloadDistance = reloadDistance;
        enemyAI.patrolSpeed = patrolSpeed;
        enemyAI.chaseSpeed = chaseSpeed;
        enemyAI.patrolPoints = patrolPoints;
        enemyAI.parentCurve = PatrolListParent;
        enemyAI.bezierTolerance = bezierTolerance;
        enemyAI.targetLayer = layer;

        enemyAI.CreateControlPoints();
        #endregion

        GameObject player = FindObjectOfType<PlayerHideController>().gameObject;
        if(player != null)
            enemyAI.ChaseObject = player.transform;

        #region Build NavMashSurface
        NavMeshSurface surface = FindObjectOfType<NavMeshSurface>();

        if(surface == null)
        {
           _surface  = new GameObject("NavMeshSurface");
           _surface.AddComponent<NavMeshSurface>();
           surface = _surface.GetComponent<NavMeshSurface>();
            surface.BuildNavMesh();
        }
        #endregion

    }

    private LayerMask LayerMaskField(string label, LayerMask layerMask)
    {
        var layers = InternalEditorUtility.layers;
        var layerNumbers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            layerNumbers[i] = LayerMask.NameToLayer(layers[i]);
        }

        int maskWithoutEmpty = 0;
        for (int i = 0; i < layerNumbers.Length; i++)
        {
            if (((1 << layerNumbers[i]) & layerMask.value) > 0)
            {
                maskWithoutEmpty |= (1 << i);
            }
        }

        maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

        int mask = 0;
        for (int i = 0; i < layerNumbers.Length; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) > 0)
            {
                mask |= (1 << layerNumbers[i]);
            }
        }

        layerMask.value = mask;
        return layerMask;
    }
}
