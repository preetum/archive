using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Omron.Actors;
using Omron.Framework;
using Omron;


namespace Omron.AI
{
    public class TroopAI : BaseTroopAI
    {
        public TroopAI(FatherUnit actor, float maxSpeed)
            : base(actor, maxSpeed)
        {
        }
        public override void SetTarget(Vector2 target)
        {
            base.SetTarget(target);

            resetSpaz();
        }
        public override void SetTargetAggressive(Vector2 target)
        {
            base.SetTargetAggressive(target);

            resetSpaz();
        }
        /// <summary>
        /// uses only the extention as the buffer zone. thus, it is more spacious than mod2. do not set feelMult = 0 here. 
        /// </summary>
        /// <param name="feelMult"></param>
        /// <returns></returns>
        Vector2 calcExternForces(float feelMult)
        {
            Vector2 fSum = Vector2.Zero;

            var arad = actor.MaxRadius;
            var feelExtend = arad * feelMult;

            var collideSet = queryRadiusAll(arad + feelExtend);

            foreach (var otherActor in collideSet)
            {
                var relPos = (otherActor.Position - actor.Position);
                var disp = (arad + feelExtend) + otherActor.MaxRadius - relPos.Length();
                if (disp > 0.0)
                {
                    var bump = -(disp / feelExtend) * Vector2.Normalize(relPos);
                    fSum += bump;
                }
            }

            return fSum;
        }
        /// <summary>
        /// uses softer displacement bumping
        /// </summary>
        /// <param name="feelMult"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        Vector2 calcExternForcesMOD2(float feelMult, Predicate<Actor> selector)
        {
            Vector2 fSum = Vector2.Zero;

            var arad = actor.MaxRadius;
            var feelExtend = arad * feelMult;
            var totRad = arad + feelExtend;

            var collideSet = queryRadiusAll(arad + feelExtend).Where(a => selector(a));

            foreach (var otherActor in collideSet)
            {
                var relPos = (otherActor.Position - actor.Position);
                var disp = (totRad) + otherActor.MaxRadius - relPos.Length();
                if (disp > 0.0)
                {
                    var bump = -(disp / totRad) * Vector2.Normalize(relPos);
                    fSum += bump;
                }
            }

            return fSum;
        }
        Vector2 calcExternForcesMOD2(float feelMult)
        {
            return calcExternForcesMOD2(feelMult, a => true);
        }
        /// <summary>
        /// eliminates the penetrating component of unit forces (gives building forces ultimate priority)
        /// </summary>
        /// <param name="feelMult"></param>
        /// <returns></returns>
        Vector2 calcExternForcesMOD(float feelMult)
        {
            Vector2 bSum = Vector2.Zero;
            Vector2 uSum = Vector2.Zero;

            var arad = actor.MaxRadius;
            var feelExtend = arad * feelMult;
            var totRad = arad + feelExtend;

            var collideSet = queryRadiusAll(arad + feelExtend);

            foreach (var otherActor in collideSet)
            {
                Vector2 bump;
                var relPos = (otherActor.Position - actor.Position);
                var disp = (arad + feelExtend) + otherActor.MaxRadius - relPos.Length();
                if (disp > 0.0)
                {
                    bump = -(disp / feelExtend) * Vector2.Normalize(relPos);

                    if (otherActor is DynamicActor)
                        uSum += bump;
                    else
                        bSum += bump;
                }
            }

            //performs correction: if unitforces points opposite to building (static) forces, eliminate the penetrating component of the unit forces. (effectivly give priority to buildings)
            if (bSum.Length() > 0)
            {
                uSum = clampToDirection(uSum, bSum);
            }

            return uSum + bSum;
        }

        /// <summary>
        /// eliminates the component of a vector which points in a direction directly opposite to a direction
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dirAxis"></param>
        /// <returns></returns>
        Vector2 clampToDirection(Vector2 source, Vector2 dirAxis)
        {
            var mod = Vector2.Dot(dirAxis, source) / dirAxis.Length();
            if (mod < 0)
            {
                var corr = (-mod) * Vector2.Normalize(dirAxis);
                return (source + corr);
            }
            else
                return source;
        }
        protected override void hold(Vector2 targ)
        {
            var del = targ - actor.Position;
            var fGoal = MathHelper.SafeNormalize(del);
            if (withinThresh(targ)) fGoal = Vector2.Zero;

            var fExt_build = calcExternForcesMOD2(0.5f, a => a is FatherBuilding);
            var fExt_unitsIdle = calcExternForcesMOD2(0.2f, a => a is FatherUnit && (a as FatherUnit).AI.State == UnitAIState.Holding);
            var fExt_unitsHot = calcExternForcesMOD2(2.0f, a => a is FatherUnit && ((a as FatherUnit).AI.State == UnitAIState.Pursuing || (a as FatherUnit).AI.State == UnitAIState.Tracking));

            var fSum = 3 * fExt_unitsHot + fExt_build + fExt_unitsIdle + fGoal;

            if (fExt_build.Length() > 0)
                fSum = clampToDirection(fSum, fExt_build);

            if (fSum.Length() > 1) //clamp to maxSpeed
                fSum *= 1 / fSum.Length();

            actor.DesiredVelocity = fSum * maxSpeed;
        }
        protected override void track(Vector2 targ)
        {
            var del = targ - actor.Position;
            var fGoal = MathHelper.SafeNormalize(del);

            var fExt_build = calcExternForcesMOD2(0.2f, a => a is FatherBuilding);
            var fExt_units = calcExternForcesMOD2(0.0f, a => a is FatherUnit);

            var fSum = fExt_build + fExt_units + fGoal;

            if (fExt_build.Length() > 0)
                fSum = clampToDirection(fSum, fExt_build);

            fSum = MathHelper.SafeNormalize(fSum);

            actor.DesiredVelocity = fSum * maxSpeed;
        }
        protected override void surround(Vector2 targ, float rad)
        {
            var fExt_build = calcExternForcesMOD2(0.0f, a => a is FatherBuilding);
            var fExt_units = calcExternForcesMOD2(0.5f, a => a is FatherUnit && a.Faction == actor.Faction); //only repel from same-faction units

            var disp = targ - actor.Position;
            var del = disp.Length() - rad;

            Vector2 fGoal = del * MathHelper.SafeNormalize(disp); //proportional response


            Vector2 fSum = 3f * fGoal + 1f * fExt_units;
            if (fExt_build.Length() > 0)
                fSum = clampToDirection(fSum, fExt_build);

            fSum += 1 * fExt_build;

            if (fSum.Length() > 1) //clamp to maxSpeed
                fSum *= 1 / fSum.Length();

            actor.DesiredVelocity = fSum * maxSpeed;
        }

        bool IsSpazzing = false;
        Vector2 preVel = Vector2.Zero;
        Vector2 avgDel = Vector2.Zero;
        const int bufLen = 6;
        int i = 0;
        Vector2[] posBuf = new Vector2[bufLen];
        void updateSpaz(float dt)
        {

            posBuf[i++] = actor.Position;
            i %= bufLen;


            if (firstRun)
            {
                if (i != 0) return;
                else firstRun = false;
            }

            var min = posBuf.Aggregate((a, b) => Vector2.Min(a, b));
            var max = posBuf.Aggregate((a, b) => Vector2.Max(a, b));

            float diam = (max - min).Length(); //approx diameter of bounding circle

            IsSpazzing = diam < ((float)bufLen) * 0.5 * maxSpeed * dt;

            //var avgVel = vels.Aggregate((a, b) => a + b);
            //avgVel /= (float)bufLen;
            //IsSpazzing = (actor.ActualVelocity.Length() > 0.00f * maxSpeed && avgVel.Length() < 0.01f * maxSpeed);        
        }
        bool firstRun = true;
        void resetSpaz()
        {
            firstRun = true;
            i = 0;
            IsSpazzing = false;
        }

        void updateRot(GameTime gameTime)
        {
            var targRot = MathHelper.GetAngle(actor.ActualVelocity);


            /*float del;
            if (Math.Sign(targRot) != Math.Sign(actor.Rotation))
            {
                var cwDel = targRot - actor.Rotation;
                var ccwDel = (float)Math.PI + targRot - actor.Rotation;

                del = Math.Abs(cwDel) < Math.Abs(ccwDel) ? cwDel : ccwDel;
            }
            else
            {
                del = targRot - actor.Rotation;
            }

            float a = 0.05f;
            var rot = actor.Rotation + a * del;*/

            var rot = targRot;

            //if (actor.ActualVelocity.Length() > 0.5f * maxSpeed) //only rotate if speed is beyond thresh, to prevent rot jitter
            //actor.DesiredRotation = rot - (float)Math.PI / 2f;  
            actor.DesiredRotation = rot;
        }
        public override void UpdateMotionFast(GameTime gameTime)
        {
            base.UpdateMotionFast(gameTime);

            if (State == UnitAIState.Holding || State == UnitAIState.Pursuing || State == UnitAIState.Tracking)
            {
                updateSpaz((float)gameTime.ElapsedGameTime.TotalSeconds);
                if (IsSpazzing)
                {
                    postarget = actor.Position;
                }
            }

            if (actor.IsRotatable)
            {
                if (engagement != null)
                    actor.DesiredRotation = MathHelper.GetAngle((engagement.Position - actor.Position));
                else
                    actor.DesiredRotation = MathHelper.GetAngle(actor.ActualVelocity);
            }
        }
    }
}
