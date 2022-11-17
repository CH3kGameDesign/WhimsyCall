using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using TMPro;

namespace Whimsy.Creatures
{
    public class Attack : MonoBehaviour
    {
        [HideInInspector]
        public AttackObject._attackObject attackInfo;
        // Start is called before the first frame update
        private List<GameObject> prevHits = new List<GameObject>();

        void Start()
            {

            }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateInfo (AttackObject._attackObject temp)
        {
            attackInfo = temp;
            GameObject.Destroy(gameObject, temp.lifetime);
        }

        public void OnTriggerEnter (Collider other)
        {
            if (attackInfo.hitSelf)
            {
                if (other.gameObject == attackInfo.self)
                {
                    DoDamage(other.gameObject);
                    return;
                }
            }
            string tag = other.tag;
            if (attackInfo.hitPlayerCreatures)
            {
                if (tag == "Creature" && other.gameObject != attackInfo.self)
                {
                    //Check If Player's
                    DoDamage(other.gameObject);
                    return;
                }
            }
            if (attackInfo.hitNonPlayerCreatures)
            {
                if (tag == "Creature" && other.gameObject != attackInfo.self)
                {
                    //Check If Not Player's
                    DoDamage(other.gameObject);
                    return;
                }
            }
            if (attackInfo.hitPlayer)
            {
                if (tag == "Player" && other.gameObject != attackInfo.self)
                {
                    DoDamage(other.gameObject);
                    return;
                }
            }
        }


        void DoDamage(GameObject hit)
        {
            if (!prevHits.Contains(hit))
            {
                GameObject GO = Instantiate(StaticScripts.objRef.damageCounter.prefab);

                GO.transform.position = transform.position;

                Vector3 forceDirection = Vector3.up * StaticScripts.objRef.damageCounter.upForce + Random.insideUnitSphere * StaticScripts.objRef.damageCounter.randomForce;
                GO.GetComponent<Rigidbody>().AddForce(forceDirection, ForceMode.Impulse);

                GO.transform.GetChild(0).GetComponent<TextMeshPro>().text = attackInfo.damage.ToString();

                if (attackInfo.breakOnHit)
                    GameObject.Destroy(gameObject);
                prevHits.Add(hit);
            }
        }
    }
}