using UnityEngine;
using System.Collections;

//自动销毁
public class AutoDestroy : MonoBehaviour {

    public float m_timer = 1.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        m_timer -= Time.deltaTime;
        if (m_timer <= 0) Destroy(this.gameObject);
	}
}
