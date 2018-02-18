using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.SceneManagement;

/* ========================================================================================================
 * This script will find all objects, that will complain about invalid MeshCollider mesh
 * within loaded scenes. It exactly replicates check of Transform component that Unity do inside (2018.1 and older)
 * 
 * */
public class CheckTransform : MonoBehaviour
{
    [MenuItem("UTools/Check Transform for MeshColliders")]
    static void CheckScene()
    {
        for (int temp = 0; temp < SceneManager.sceneCount; ++temp)
        {
            var scene = SceneManager.GetSceneAt(temp);
            var roots = scene.GetRootGameObjects();
            foreach(var root in roots)
                CheckGameObject(root);
        }
    }

    static void CheckGameObject(GameObject go)
    {
        var meshCollider = go.GetComponent<MeshCollider>();
        if ((meshCollider != null) && IsScaleBakingRequired(go))
        {
            string assetPath = AssetDatabase.GetAssetPath(meshCollider.sharedMesh.GetInstanceID());
            Debug.Log("Object: " + go.name + " with mesh " + assetPath, go);
        }

        for (int temp = 0; temp < go.transform.childCount; ++temp)
        {
            var child = go.transform.GetChild(temp);
            CheckGameObject(child.gameObject);
        }
    }

    static bool CompareApproximately(float f0, float f1, float epsilon = 0.000001F)
    {
        float dist = (f0 - f1);
        dist = Math.Abs(dist);
        return dist <= epsilon;
    }

    public static Matrix4x4 Rotate(Quaternion q)
    {
        // Precalculate coordinate products
        float x = q.x * 2.0F;
        float y = q.y * 2.0F;
        float z = q.z * 2.0F;
        float xx = q.x * x;
        float yy = q.y * y;
        float zz = q.z * z;
        float xy = q.x * y;
        float xz = q.x * z;
        float yz = q.y * z;
        float wx = q.w * x;
        float wy = q.w * y;
        float wz = q.w * z;

        // Calculate 3x3 matrix from orthonormal basis
        Matrix4x4 m;
        m.m00 = 1.0f - (yy + zz); m.m10 = xy + wz; m.m20 = xz - wy; m.m30 = 0.0F;
        m.m01 = xy - wz; m.m11 = 1.0f - (xx + zz); m.m21 = yz + wx; m.m31 = 0.0F;
        m.m02 = xz + wy; m.m12 = yz - wx; m.m22 = 1.0f - (xx + yy); m.m32 = 0.0F;
        m.m03 = 0.0F; m.m13 = 0.0F; m.m23 = 0.0F; m.m33 = 1.0F;
        return m;
    }

    public static Matrix4x4 CalculateGlobalRS(Transform transform)
    {
        Matrix4x4 globalRS = Rotate(transform.localRotation);
        globalRS = globalRS * Matrix4x4.Scale(transform.localScale);

        while (transform.parent)
        {
            Transform parent = transform.parent;
            Matrix4x4 parentRS = Rotate(parent.localRotation);
            parentRS = parentRS * Matrix4x4.Scale(parent.localScale);
            globalRS = parentRS * globalRS;

            transform = parent;
        }

        return globalRS;
    }

    public static Matrix4x4 CalculateGlobalScale(Transform transform)
    {
        Quaternion globalR = transform.rotation;
        Matrix4x4 grmInv = Rotate(Quaternion.Inverse(globalR));

        Matrix4x4 grsm = CalculateGlobalRS(transform);
        return grmInv * grsm;
    }

    public static bool HasValidNonUniformScale(Matrix4x4 matrix)
    {
        float epsilon = 0.01F;
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (x == y)
                {
                    if (matrix[y, x] < 0.0F)
                        return false;
                }
                else
                {
                    if (!CompareApproximately(matrix[y, x], 0.0F, epsilon))
                        return false;
                }
            }
        }
        return true;
    }

    public static bool IsNonUniformScaleTransform(Transform transform)
    {
        MethodInfo dynMethod = transform.GetType().GetMethod("IsNonUniformScaleTransform", BindingFlags.NonPublic | BindingFlags.Instance);
        return (bool)dynMethod.Invoke(transform, null);
    }

    public static bool IsScaleBakingRequired(GameObject go)
    {
        var transform = go.GetComponent<Transform>();
        return IsNonUniformScaleTransform(transform) && !HasValidNonUniformScale(CalculateGlobalScale(transform));
    }
}
