using UnityEngine;

public sealed class BossAgent : MonoBehaviour
{
    private BossEncounter encounter;
    private Health player;
    private ArenaBounds bounds;
    private Rigidbody2D body;
    private readonly BossHazard[] hazards = new BossHazard[48];
    private LineRenderer aim;
    private Vector2 lockedDirection;
    private float nextAttack, strikeAt, dashUntil, nextDamage;
    private int attackIndex;
    private bool preparing, defeated;
    public bool IsFinal { get; private set; }
    public Health Health { get; private set; }
    public bool IsPreparing => preparing;
    public void Initialize(BossEncounter owner, Health target, ArenaBounds arena, bool final, float maximum)
    {
        encounter=owner;player=target;bounds=arena;IsFinal=final;body=GetComponent<Rigidbody2D>();
        Health=GetComponent<Health>();GetComponent<EnemyController>().SpawnBoss(this,maximum,Color.white);
        var aimRoot=new GameObject("Attack Telegraph");aimRoot.transform.SetParent(transform,false);
        aim=aimRoot.AddComponent<LineRenderer>();aim.sharedMaterial=ArenaPresentation.TrailMaterial;aim.useWorldSpace=true;
        aim.startWidth=aim.endWidth=.14f;aim.startColor=aim.endColor=new Color(1f,.3f,.16f,.8f);aim.sortingOrder=10;aim.positionCount=2;aim.enabled=false;
        for(var i=0;i<hazards.Length;i++)
        {
            var root=new GameObject("Boss Hazard "+i);root.SetActive(false);root.transform.SetParent(encounter.transform);
            hazards[i]=root.AddComponent<BossHazard>();hazards[i].Initialize(this,player,bounds);
        }
        nextAttack=Time.time+1.2f;
    }
    private void FixedUpdate()
    {
        if(player==null || player.IsDead || Health.IsDead || defeated) return;
        var position=body.position;
        if(Vector2.Distance(position,player.transform.position)<(IsFinal?1.55f:1.25f))TryDamage(IsFinal?34:24);
        if(Time.time<dashUntil)
        {
            body.MovePosition(bounds.ClampPoint(position+lockedDirection*(IsFinal?15:11)*Time.fixedDeltaTime,1.4f));return;
        }
        if(preparing)
        {
            if(Time.time<strikeAt)return;
            preparing=false;aim.enabled=false;
            if(attackIndex%3==0)FireVolley();
            if(attackIndex%3==2)dashUntil=Time.time+.65f;
            attackIndex++;nextAttack=Time.time+(IsFinal?.75f:1.1f);return;
        }
        if(Time.time>=nextAttack){PrepareAttack();return;}
        var direction=((Vector2)player.transform.position-position).normalized;
        body.MovePosition(bounds.ClampPoint(position+direction*(IsFinal?3.4f:2.3f)*Time.fixedDeltaTime,1.4f));
    }
    private void PrepareAttack()
    {
        preparing=true;
        var enraged=IsFinal && Health.Current<Health.Max*.5f;
        var windup=enraged?.65f:IsFinal?.85f:1.05f;
        strikeAt=Time.time+windup;
        var velocity=player.GetComponent<Rigidbody2D>().linearVelocity;
        var target=(Vector2)player.transform.position+Vector2.ClampMagnitude(velocity,9)*.45f;
        lockedDirection=(target-body.position).normalized;
        if(attackIndex%3==1)
        {
            Zone(bounds.ClampPoint(target,.5f),IsFinal?3f:2.3f,windup);
            if(IsFinal)
            {
                Zone(bounds.ClampPoint(target+Vector2.Perpendicular(lockedDirection)*4,.5f),2.4f,windup+.25f);
                Zone(bounds.ClampPoint(target-Vector2.Perpendicular(lockedDirection)*4,.5f),2.4f,windup+.25f);
            }
        }
        else
        {
            aim.enabled=true;aim.SetPosition(0,transform.position);aim.SetPosition(1,body.position+lockedDirection*(attackIndex%3==2?10:12));
        }
        CombatFeedback.Instance?.PlayEnemyCharge(transform.position,new Color(1,.4f,.2f));
    }
    private BossHazard Acquire()
    {
        foreach(var hazard in hazards)if(!hazard.gameObject.activeSelf)return hazard;
        return null;
    }
    private void Zone(Vector2 center,float radius,float delay)=>Acquire()?.ArmZone(center,radius,delay,IsFinal?38:25);
    private void FireVolley()
    {
        var count=IsFinal?7:3;
        for(var i=0;i<count;i++)
        {
            var direction=(Vector2)(Quaternion.Euler(0,0,(i-(count-1)*.5f)*12)*lockedDirection);
            Acquire()?.Fire(body.position+direction*1.4f,direction*(IsFinal?9:7),IsFinal?28:18);
        }
        if(IsFinal && Health.Current<Health.Max*.5f)
            for(var i=0;i<8;i++)
            {
                var direction=new Vector2(Mathf.Cos(i*Mathf.PI/4),Mathf.Sin(i*Mathf.PI/4));
                Acquire()?.Fire(body.position+direction*1.4f,direction*6,22);
            }
    }
    public void TryDamage(float damage)
    {
        // A single volley cannot stack seven contacts into one unavoidable instant kill.
        if(defeated || player.IsDead || Time.time<nextDamage)return;
        nextDamage=Time.time+.3f;player.TakeDamage(damage);CombatFeedback.Instance?.PlayPlayerHit(player.transform.position);
    }
    public void Defeated()
    {
        if(defeated)return;defeated=true;
        foreach(var hazard in hazards)if(hazard!=null)hazard.Release();
        encounter.BossDefeated(this);gameObject.SetActive(false);Destroy(gameObject);
    }
    private void OnDestroy(){foreach(var hazard in hazards)if(hazard!=null)Destroy(hazard.gameObject);}
    public static void ConfigureRing(LineRenderer line,float radius,Color color)
    {
        line.sharedMaterial=ArenaPresentation.TrailMaterial;line.useWorldSpace=false;line.loop=true;line.positionCount=64;
        line.startWidth=line.endWidth=.09f;line.startColor=line.endColor=color;line.sortingOrder=9;
        for(var i=0;i<64;i++){var a=i*Mathf.PI*2/64;line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0));}
    }
}
