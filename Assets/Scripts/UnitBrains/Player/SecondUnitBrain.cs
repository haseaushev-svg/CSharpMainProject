using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        private List<Vector2Int> _farTargets = new List<Vector2Int>();

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            BaseProjectile projectile = CreateProjectile(forTarget);
            AddProjectileToList(projectile, intoList);
        }

        public override Vector2Int GetNextStep()
        {
            List<Vector2Int> targets = SelectTargets();

            // если целей нет или цель уже в зоне атаки — стоим на месте
            if (targets.Count == 0 || IsTargetInRange(targets[0]) == true)
            {
                return unit.Pos;
            }

            Vector2Int target = targets[0];
            Vector2Int delta = target - unit.Pos;
            Vector2Int step = unit.Pos;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                step.x = step.x + System.Math.Sign(delta.x);
            }
            else
            {
                step.y = step.y + System.Math.Sign(delta.y);
            }

            return step;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            _farTargets.Clear();

            List<Vector2Int> all = new List<Vector2Int>();

            foreach (Vector2Int t in GetAllTargets())
            {
                all.Add(t);
            }

            if (all.Count == 0)
            {
                return result;
            }

            Vector2Int ownBase = runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];

            float minDist = float.MaxValue;
            Vector2Int closest = Vector2Int.zero;

            foreach (Vector2Int t in all)
            {
                float d = (t - ownBase).sqrMagnitude;

                if (d < minDist)
                {
                    minDist = d;
                    closest = t;
                }
            }

            if (IsTargetInRange(closest) == true)
            {
                result.Add(closest);
            }
            else
            {
                _farTargets.Add(closest);
                result.Add(closest);
            }

            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated == true)
            {
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown / 10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);

                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if (_overheated == true)
            {
                return (int)OverheatTemperature;
            }

            return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;

            if (_temperature >= OverheatTemperature)
            {
                _overheated = true;
            }
        }
    }
}