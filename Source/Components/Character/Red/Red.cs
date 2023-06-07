using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace EternitySaga.Components;
/// <summary>EternitySaga Character class base on
/// <see langword="Nez.Component"/>.
/// <inheritdoc/>
/// </summary>
public class Red : Component, IUpdatable
{
    private Utils.Pathfinder _pathfinder;
    private List<Vector2> _path;
    private List<Vector2> _tempPath;
    private int _currentWaypoint = 0;
    private Mover _mover;
    private SubpixelVector2 _subpixelV2 = new();

    /// <inheritdoc cref="Red" path="/summary"/>
    public Red() { }
    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        var texture = Entity.Scene.Content.LoadTexture("Sprite/Red/Front Movement");
        var sprites = Sprite.SpritesFromAtlas(texture, 64, 64);
        var animator = Entity.AddComponent(new SpriteAnimator());
        _pathfinder = Entity.AddComponent(new Utils.Pathfinder());
        _tempPath = new();
        RegisterAnimation(sprites, animator);
        _mover = Entity.AddComponent(new Mover());
    }

    private static void RegisterAnimation(List<Sprite> sprites, SpriteAnimator animator)
    {
        animator.AddAnimation("idle", 10f, sprites[0], sprites[1], sprites[2], sprites[3], sprites[4], sprites[5]);
        animator.AddAnimation("walk-front", 10f, sprites[6], sprites[7], sprites[8], sprites[9], sprites[10], sprites[11]);
        animator.Play("idle");
    }

    void IUpdatable.Update()
    {
        if (Input.RightMouseButtonPressed)
        {
            var start = Entity.Position;
            var end = Entity.Scene.Camera.MouseToWorldPoint();
            var newPath = _pathfinder.SearchPath(start, end);
            if (newPath != null && !newPath.SequenceEqual(_tempPath))
            {
                UpdatePath(newPath);
            }
        }
        var pathCount = _path?.Count ?? 0;
        if (pathCount > 0 && _currentWaypoint < pathCount) Move();
    }

    private void UpdatePath(List<Vector2> newPath)
    {
        if (newPath.Count == 1) return;
        _currentWaypoint = 0;
        _path = newPath;
        _tempPath = newPath;
    }

    private void Move()
    {
        var nextWayPoint = _path[_currentWaypoint];
        var direction = CalculateDirection(Entity.Position, nextWayPoint);
        var speed = 25;
        var waypointThreshold = 1f;
        var movement = direction * speed * Time.DeltaTime;

        _mover.CalculateMovement(ref movement, out var _);
        _subpixelV2.Update(ref movement);
        _mover.ApplyMovement(movement);
        if (Vector2.Distance(Entity.Position, nextWayPoint) < waypointThreshold)
        {
            _currentWaypoint++;
        }
    }

    ///<summary>Calculate direction from start point to end point</summary>
    /// <param name="start">Departure point.</param>
    /// <param name="end">Destination point.</param>
    /// <returns>Normalized Vector2</returns>
    private static Vector2 CalculateDirection(Vector2 start, Vector2 end)
    {
        var direction = end - start;
        return direction.Length() < 0.000001f ?
            Vector2.Zero : Vector2.Normalize(end - start);
    }
}