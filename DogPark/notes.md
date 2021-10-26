List of FMOD events sent by the game:

## BackgroundLoops

For background music/loops. This event is emitted once at the start of the game. Parameter values are set based on which 'zone' the player is in (look at Park AudioRegions in the unity project to see the different zones).

### Params
- Zone1 Volume
- Zone2 Volume
- Zone3 Volume
- Forest Volume
- Mountaintop Volume

## Water

For water sounds. This event is emitted once at the start of the game. The location of the emitter is set to the closest point on the river to the player.

## PlayerFeet

Emitted when a character foot hits the ground.

### Params
- GroundMaterial
  - The material of the surface the foot is hitting.

## PlayerHighlight

Emitted when an object is highlighted (can be picked up).

## PlayerJump

Emitted when a character player jumps.

## PlayerMovement

### Params

- Velocity (0 to 1)
- YPos
  - Y position (height) of the player (0 to 20?)
- OnGround
  - Is the player on the ground? (0 = no, 1 = yes) 
- IsStrong
  - Is the player holding the shift key?

## PlayerPickup

Emitted when the player picks up an object.

## PlayerDrop

Emitted when the player releases an object.

## PetFeet

Emitted when a pet foot hits the ground.

### Params
- GroundMaterial
  - The material of the surface the foot is hitting.

## PetSounds

General pet sounds.

### Params
- PetInterest
  - How interested is the pet in whatever object it is chasing? (0 to 1)

## BallHit

Emitted when a ball collides with something.

### Params
- BallType
  - What kind of ball (Red, Blue, Christmas, Orange, Rainbow, Green)
- ImpactVelociy
  - Velocity of the impact (0 to 1)
- GroundMaterial
  - The material of the surface the ball is hitting.

## BallMovement

- BallType
  - What kind of ball (Red, Blue, Christmas, Orange, Rainbow, Green)
- BallVelocity (0 to 1)
- YPos
  - Y position (height) of the ball (0 to 20?)
- OnGround
  - Is the ball on the ground? (0 = no, 1 = yes) 

## Grass

?

## Hills

?

## NestedAmbience

?

