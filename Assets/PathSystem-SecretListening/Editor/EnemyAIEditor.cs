using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyAI))]
public class EnemyAIEditor : Editor
{
    void OnSceneGUI()
    {
        EnemyAI enemy = (EnemyAI)target;

        // Devriye noktalar� aras�nda kontrol noktalar�n� d�zenlenebilir yap
        if (enemy.patrolPoints.Length > 1)
        {
            for (int i = 0; i < enemy.patrolPoints.Length; i++)
            {
                // Kontrol noktalar�n� handles ile s�r�klenebilir yap
                if (enemy.curveControlPoints[i] != null)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPos = Handles.PositionHandle(enemy.curveControlPoints[i].position, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(enemy.curveControlPoints[i], "Move Control Point");
                        enemy.curveControlPoints[i].position = newPos;
                    }

                }
            }
        }
    }
}
