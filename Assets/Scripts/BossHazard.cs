using UnityEngine;

public sealed class BossHazard : MonoBehaviour
{
    private BossAgent owner;
    private Health player;
    private ArenaBounds bounds;
    private LineRenderer ring;
    private Vector2 velocity;
    private float radius,damage,armedAt,expires;
    private bool zone,resolved;
    public void Initialize(BossAgent boss,Health target,ArenaBounds arena)
    {
        owner=boss;player=target;bounds=arena;ring=gameObject.AddComponent<LineRenderer>();
    }
    public void ArmZone(Vector2 center,float size,float delay,float hitDamage)
    {
        transform.position=center;zone=true;resolved=false;radius=size;damage=hitDamage;armedAt=Time.time+delay;expires=armedAt+.25f;
        BossAgent.ConfigureRing(ring,radius,new Color(1,.32f,.16f,.75f));gameObject.SetActive(true);
    }
    public void Fire(Vector2 origin,Vector2 movement,float hitDamage)
    {
        transform.position=origin;zone=false;resolved=false;velocity=movement;damage=hitDamage;radius=.22f;expires=Time.time+5;
        BossAgent.ConfigureRing(ring,radius,new Color(1,.42f,.15f));gameObject.SetActive(true);
    }
    private void Update()
    {
        if(Time.deltaTime==0)return;
        if(owner==null || player.IsDead || Time.time>=expires){Release();return;}
        if(zone)
        {
            if(Time.time>=armedAt && !resolved)
            {
                resolved=true;ring.startColor=ring.endColor=Color.white;
                if(Vector2.Distance(transform.position,player.transform.position)<radius+.3f)owner.TryDamage(damage);
            }
        }
        else
        {
            var from=(Vector2)transform.position;var to=from+velocity*Time.deltaTime;
            var delta=to-from;
            var t=delta.sqrMagnitude>0 ? Mathf.Clamp01(Vector2.Dot((Vector2)player.transform.position-from,delta)/delta.sqrMagnitude):0;
            if(Vector2.Distance(from+delta*t,player.transform.position)<.55f){owner.TryDamage(damage);Release();return;}
            transform.position=to;
            if(!bounds.Contains(to,-1))Release();
        }
    }
    public void Release()=>gameObject.SetActive(false);
}
