using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    public float thrust;
    public float knockTime;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Player"))
        {
            Rigidbody2D hit = other.GetComponent<Rigidbody2D>();
            if (hit != null)
            {
                Vector2 difference = hit.transform.position - transform.position;
                difference = difference.normalized * thrust;
                hit.AddForce(new Vector3(difference.x, 0, 0), ForceMode2D.Impulse);
                
                if (other.gameObject.CompareTag("Enemy"))
                {
                    other.gameObject.GetComponent<Enemy>().currentState = EnemyState.Stagger;
                    other.gameObject.GetComponent<Enemy>().Knock(hit, knockTime);
                }
                else if (other.gameObject.CompareTag("Player"))
                {
                    other.gameObject.GetComponent<PlayerController>().currentState = PlayerState.Stagger;
                    other.gameObject.GetComponent<PlayerController>().Knock(hit, knockTime);
                }
            }
        }
    }

    
}
