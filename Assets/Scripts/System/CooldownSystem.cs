using Unity.Entities;


public class CooldownSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;

        Dependency = Entities.ForEach((ref Cooldown cd) => {
            if(cd.accu < cd.cd)
            {
                cd.accu += dt;
            }
        }).ScheduleParallel(Dependency);
    }
}