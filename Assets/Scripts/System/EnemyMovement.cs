using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class EnemyMovementSystem : SystemBase
{
    EntityQuery playerQuery;

    protected override void OnCreate()
    {
        var playerDesc = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Player), typeof(Translation) }
        };
        playerQuery = GetEntityQuery(playerDesc);
    }

    protected override void OnUpdate()
    {

        var entities = playerQuery.ToEntityArray(Allocator.TempJob);
        var positions = playerQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

        Dependency = Entities.ForEach((ref Velocity vel, in Enemy enemy, in Translation pos) => {
            Entity closestEntity = new Entity{ };
            float closestDist = float.MaxValue ;
            int closestIndex = -1;

            for(int i = 0; i < positions.Length; i++)
            {
                var dist = math.distance(positions[i].Value, pos.Value);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestEntity = entities[i];
                    closestIndex = i;
                }
            }
            if (closestIndex != -1)
            {
                if (closestDist < enemy.sightRange)
                {
                    if(closestDist < enemy.attackRange && closestDist > enemy.attackRange - enemy.leeWay)
                    {
                        vel.value = float2.zero;
                    }
                    else if(closestDist < enemy.attackRange - enemy.leeWay)
                    {
                        vel.value = (math.normalize(pos.Value - positions[closestIndex].Value)).xy;
                    }
                    else
                    {
                        vel.value = (math.normalize(positions[closestIndex].Value - pos.Value)).xy;
                    }
                }
                else
                {
                    vel.value = float2.zero;
                }
            }

        }).WithReadOnly(positions).ScheduleParallel(Dependency);

        Dependency.Complete();
        positions.Dispose();
        entities.Dispose();
    }

 
}
