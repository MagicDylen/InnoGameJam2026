using UnityEngine;

public class Cooldown
{
    private readonly float _cooldownTime;
    private float _lastUseTime;

    public Cooldown(float cooldownTime)
    {
        _cooldownTime = cooldownTime;
        _lastUseTime = -cooldownTime;
    }

    public bool HasFinished()
    {
        return Time.time >= _lastUseTime + _cooldownTime;
    }

    public void StartCooldown()
    {
        _lastUseTime = Time.time;
    }
}