# DataOrientedAssignment_SpaceShooter

##How to play the game:
WASD to move.
Space to shoot.

##"How I made the systems in a data oriented fashion"

By assuming "if there's one, there's more" and by trying to generalize the behavior of an entity by composition.
For example, regardless of entity, anything with velocity will want to integrate the velocity into the translation.

###Collision Detection

For collision detection I use a fairly overkill and costly GJK algorithm for narrowphase, there is no real broadphase system at the moment, only a fairly naive aabb intersection test before doing GJK test.

The collision detection system generates CollisionResults which is stored as a buffer for each entity.
The collision results buffers are cleared at the start of each collision system update, the system also lacks caching the collisions so it is currently not possible to detect when the collision was entered/exited.

###Collision Response

The collision response system iterates through the collision results to resolve collisions. To interact with other systems the idea is to add/remove buffers/components to communicate. For example a bullet damaging an enemy will append a DamageEvent buffer element. Other system then iterate those buffers to evaluate what to do.

###Damage System

Simply iterating all current damage events for each buffer and decrementing the health component.
Afterwards, check if health is less than or equal to 0, if so destroy the entity at the end of the simulation.

###Player Systems

The player systems are implemented as if there are multiple players, but currently it only supports one player.
It would be easy to increase the number of players supported, the only missing part is implementing different input sources and spawning multiple players entities.

The player can be modified through a scriptable object called Player settings.
I choose this approach over the ConvertAndDestroy method since I was aiming for a pure dots implementation.

The players' bullets can also be modified/added to through two scriptable objects. BulletPrefab and BulletPrefabList.
The bullets list is stored as a BlobAsset, with a BlobAssetReference on an entity in the world. This means that any system could theoretically get a hold of this data by querying for the entity.

###Enemy Systems

Assumes there are multiple players they can move towards. Enemy types can be modified/added to by using the scriptable objects EnemyPrefab and EnemySettings.

###Cooldown System

Simply update the cooldown accumulator.

 
