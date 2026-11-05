using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GameplayValidation
{
    // Run explicitly from batch mode; never modifies scenes on editor startup.
    public static void PrepareAndValidate()
    {
        foreach (string name in new[] { "Level1", "Level2", "Level3" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/" + name + ".unity");
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            Require(player != null, name + " player");
            var animator = player.GetComponentInChildren<Animator>();
            Require(animator != null && animator.runtimeAnimatorController != null, name + " animation controller");
            foreach (var boss in UnityEngine.Object.FindObjectsByType<BossHealth>(FindObjectsSortMode.None))
            {
                var hitbox = boss.GetComponent<BoxCollider2D>();
                var visual = boss.transform.Find("BossV").GetComponent<SpriteRenderer>();
                Bounds bounds = visual.bounds;
                Vector3 center = boss.transform.InverseTransformPoint(bounds.center);
                Vector3 size = boss.transform.InverseTransformVector(bounds.size);
                hitbox.offset = center;
                hitbox.size = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
                EditorUtility.SetDirty(hitbox);
                Require(hitbox.size.x > 0.1f && hitbox.size.y > 0.1f, "Boss hitbox size");
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("GAMEPLAY_VALIDATION: scene setup passed");
    }

    static void Require(bool condition, string label)
    {
        if (!condition) throw new Exception("Validation failed: " + label);
    }
}
