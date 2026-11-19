using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class LevelHealthTests
{
    static Component Find(string type) => (Component)UnityEngine.Object.FindFirstObjectByType(Type.GetType(type + ", Assembly-CSharp", true));
    static object Field(Component c,string key) => c.GetType().GetField(key,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).GetValue(c);

    [UnityTest]
    public IEnumerator ActualEnemyHitsUpdateTheVisibleHealthCounter()
    {
        for (int level=1;level<=3;level++)
        {
            yield return SceneManager.LoadSceneAsync("Level"+level);
            yield return null;
            var hp=Find("PlayerHealth"); var body=hp.GetComponent<Rigidbody2D>();
            var label=(Behaviour)Field(hp,"healthText");
            Assert.IsTrue(label.isActiveAndEnabled,"HUD in Level"+level);
            var text=label.GetType().GetProperty("text");
            Assert.AreEqual("HP: 3/3",text.GetValue(label));
            body.constraints=RigidbodyConstraints2D.FreezeAll;
            if(level==1)
                body.position=(Vector2)Find("ForestGuard").transform.position+Vector2.right*0.7f;
            else if(level==2)
                body.position=Find("EnemyPatrol").transform.position;
            else
            {
                var attack=Find("BossAttack");
                var prefab=(GameObject)Field(attack,"bossBulletPrefab");
                var shot=UnityEngine.Object.Instantiate(prefab,body.position+Vector2.left,Quaternion.identity);
                shot.SendMessage("SetDirection",Vector2.right);
            }
            Physics2D.SyncTransforms();
            float until=Time.time+2;
            while((int)Field(hp,"currentHealth")==3 && Time.time<until) yield return null;
            Assert.AreEqual(2,Field(hp,"currentHealth"),"Enemy hit in Level"+level);
            Assert.AreEqual("HP: 2/3",text.GetValue(label),"Visible HP in Level"+level);
            hp.GetType().GetMethod("Heal").Invoke(hp,new object[]{1});
            Assert.AreEqual("HP: 3/3",text.GetValue(label));
        }
    }
}
