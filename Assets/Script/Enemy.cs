using UnityEngine;
using System.Collections;

//敌人控制器
public class Enemy: MonoBehaviour{

    //Transform组件
    Transform m_transform;

    //动画组件
    Animator m_ani;
    //主角
    GameObject m_player;
    Player m_playerSr;

    //寻路组件
    NavMeshAgent m_agent;

    //移动速度
    float m_moveSpeed = 1.0f;

    //旋转速度
    float m_rotSpeed = 120;

    //计时器 
    float m_timer = 2;

    //生命值
    int m_life = 10;

    //生成点
    EnemySpawn m_spawn;

    //初始化
    public void Init(EnemySpawn spawn)
    {
        m_spawn = spawn;
        m_spawn.m_enemyCount++;
    }

    void Start () {
        //获取组件
        m_transform = this.transform;
        //获取动画组件
        m_ani = this.GetComponent<Animator>();

        //获得主角
        m_player = GameObject.FindGameObjectWithTag("Player");
        m_playerSr = m_player.GetComponent<Player>();
        //获得寻路组件
        m_agent = GetComponent<NavMeshAgent>();
        //设置寻路目标
        //m_agent.SetDestination(m_player.transform.position);
    }

    void Update()
    {

        //如果主角死了，则什么都不做
        if (m_playerSr.m_life <= 0) return;

        //获取当前动画状态
        AnimatorStateInfo stateInfo = m_ani.GetCurrentAnimatorStateInfo(0);
       
        //如果处于待机状态
        if(stateInfo.fullPathHash == Animator.StringToHash("Base Layer.idle")
            && !m_ani.IsInTransition(0))
        {
            m_ani.SetBool("idle", false);

            //待机一定时间
            m_timer -= Time.deltaTime;
            if (m_timer > 0) return;

            //如果距离主角小于1.5米，进入攻击动画状态
            if(Vector3.Distance(m_transform.position,m_player.transform.position) < 1.5f)
            {
                m_ani.SetBool("attack", true);
            }
            else
            {
                //重置定时器
                m_timer = 1;

                //设置寻路目标点
                m_agent.SetDestination(m_player.transform.position);

                //进入跑步动画状态
                m_ani.SetBool("run", true);
            }
        }
        //如果处于跑步状态
        if(stateInfo.fullPathHash == Animator.StringToHash("Base Layer.run")
             && !m_ani.IsInTransition(0))
        {
            m_ani.SetBool("run", false);

            //每隔一秒重新定位主角的位置
            m_timer -= Time.deltaTime;
            if(m_timer < 0)
            {
                m_agent.SetDestination(m_player.transform.position);
                m_timer = 1;
            }
            //追向主角
            //MoveTo();
            //如果距离主角小于1.5米，攻击主角
            if (Vector3.Distance(m_transform.position, m_player.transform.position) < 1.5f)
            {
                //停止寻路
                m_agent.ResetPath();
                m_ani.SetBool("attack", true);
            }
        }
        //如果处于攻击状态
        if(stateInfo.fullPathHash == Animator.StringToHash("Base Layer.attack")
             && !m_ani.IsInTransition(0))
        {
            //面向主角
            RotateTo();
            m_ani.SetBool("attack", false);

            
            //如果动画播放完毕，重新进入待机状态
            if(stateInfo.normalizedTime >= 1.0f)
            {
                m_ani.SetBool("idle", true);
                //重置计时器
                m_timer = 2;

                //更新主角生命
                m_playerSr.OnDamage(1);
            }
        }
        //如果处于死亡状态
        if(stateInfo.fullPathHash == Animator.StringToHash("Base Layer.death")
            && !m_ani.IsInTransition(0))
        {
            //当播放死亡动画
            if(stateInfo.normalizedTime >= 1.0f)
            {
                OnDeath();
            }
        }


    }
    //寻路移动
    void MoveTo()
    {
        float speed = m_moveSpeed * Time.deltaTime;
        m_agent.Move(m_transform.TransformDirection(new Vector3(0, 0, speed)));
    }

    //转向目标点
    void RotateTo()
    {
        //当前角度
        Vector3 oldangle = m_transform.eulerAngles;

        //获得面向主角的角度
        m_transform.LookAt(m_player.transform);
        float target = m_transform.eulerAngles.y;

        //转向主角
        float speed = m_rotSpeed * Time.deltaTime;
        float angle = Mathf.MoveTowardsAngle(oldangle.y, target, speed);
        m_transform.eulerAngles = new Vector3(0, angle, 0);
    }
    //被打了
    public void OnDamage(int damage)
    {
        m_life -= damage;
        if (m_life <= 0)//进入死亡状态
        {
            m_ani.SetBool("death", true);
        }
    }
    //当被销毁时
    public void OnDeath()
    {
        //更新敌人数量
        m_spawn.m_enemyCount--;
        //加分
        GameManager.Instance.SetScore(100);
        //销毁自身
        Destroy(this.gameObject);
    }

}

