using Sandbox.Physics;

namespace AtmokineticAssets;

[Title( "Atmokinetic Assets - Character Controller Utility" )]
[Category( "Atmokinetic Assets" )]
[Icon( "badge" )]


public sealed partial class CharacterControllerUtility : Component, Component.INetworkSpawn
{
    /// <summary>
    /// THIS IS PRIMARILY USED FOR THE MODELPHYISICS TO GENERATE WATER SIMULATIONS/RIPPLES
    /// WITHOUT COLLIDING WITH THE CHARACTER CONTROLLER'S BOX/CAPSULE COLLIDER. 
    /// IT USES THE STEAMID AS AN IDENTIFIER.
    /// </summary>

    private string _appliedTag;
    private ModelPhysics _modelPhysics;
    private int _lastTaggedBodyCount = -1;

    public void OnNetworkSpawn( Connection owner )
    {
        EnsurePhysics();
        ApplyTag( owner );
    }

    protected override void OnStart()
    {
        EnsurePhysics();

        // Fallback for non-networked scenes: tag with the local connection
        ApplyTag( Network.Owner ?? Connection.Local );
    }

    // Create a ModelPhysics from the player's SkinnedModelRenderer if one is
    // missing, so the collision bodies this component tags actually exist.
    private void EnsurePhysics()
    {
        if ( GameObject.GetComponentInChildren<ModelPhysics>( true ).IsValid() )
            return;

        var renderer = GameObject.GetComponentInChildren<SkinnedModelRenderer>( true );
        if ( !renderer.IsValid() || renderer.Model is null )
            return;

        var physics = renderer.GameObject.AddComponent<ModelPhysics>();
        physics.Renderer = renderer;
        physics.Model = renderer.Model;
        physics.MotionEnabled = false;
    }

    protected override void OnUpdate()
    {
        // ModelPhysics creates its ragdoll bone objects lazily (and rebuilds them on
        // model reload). Instead of walking every child collider each tick, watch the
        // body count - it only changes when the ragdoll is (re)built.
        if ( string.IsNullOrEmpty( _appliedTag ) )
            return;

        if ( !_modelPhysics.IsValid() )
            _modelPhysics = GameObject.GetComponentInChildren<ModelPhysics>( true );

        int bodyCount = _modelPhysics.IsValid() ? _modelPhysics.Bodies.Count : 0;
        if ( bodyCount == _lastTaggedBodyCount )
            return;

        _lastTaggedBodyCount = bodyCount;
        TagColliders( _appliedTag );
    }

    private void ApplyTag( Connection connection )
    {
        if ( connection is null )
            return;

        var tag = connection.SteamId.ToString();

        if ( _appliedTag == tag )
            return;

        // Clean up a previously applied id (e.g. after ownership transfer)
        if ( !string.IsNullOrEmpty( _appliedTag ) )
            GameObject.Tags.Remove( _appliedTag );

        // Another component may have already stamped this exact Steam ID tag (e.g. a
        // duplicate tagger, or this one re-running after a hot reload) - don't add it
        // or the collision-ignore rule twice, but still make sure the ragdoll bones
        // (which that other component may not know about) are covered.
        bool alreadyTagged = GameObject.Tags.Has( tag );

        if ( !alreadyTagged )
        {
            GameObject.Tags.Add( tag );
            IgnoreSelfCollision( tag );
        }

        TagColliders( tag );

        _appliedTag = tag;
    }

    // Add a rule so any two shapes both carrying this exact Steam ID tag don't
    // collide. Only this player's own colliders carry it, so its ragdoll ignores
    // its own body while still colliding with other players (whose tag differs).
    private void IgnoreSelfCollision( string tag )
    {
        var rules = Scene.PhysicsWorld?.CollisionRules;
        if ( rules is null )
            return;

        rules.Pairs[new CollisionRules.Pair( tag, tag )] = CollisionRules.Result.Ignore;
    }

    // Stamp the Steam ID tag onto every collider under the player. Shapes take
    // their tags from their GameObject, and ragdoll bone objects don't inherit
    // the root's tags, so each one has to be tagged directly.
    private void TagColliders( string tag )
    {
        foreach ( var collider in GameObject.GetComponentsInChildren<Collider>( true ) )
        {
            if ( !collider.GameObject.Tags.Has( tag ) )
                collider.GameObject.Tags.Add( tag );
        }
    }

}
