﻿using System;
using Planets.Controller.GameRules.Abstract;
using Planets.Model;
using Planets.Model.GameObjects;

namespace Planets.Controller.GameRules.Impl
{
    class DynamicEatRule : AbstractCollisionRule
    {
        protected override void DoCollision(Playfield pf, ScoreBoard sb, GameObject go1, GameObject go2, double ms)
        {
            if (!go1.Is(Rule.EATABLE) && !go2.Is(Rule.EATABLE)) return;
            if (!go1.Is(Rule.DYNAMIC_RADIUS) && !go2.Is(Rule.DYNAMIC_RADIUS)) return;
            if (go1 is BlackHole || go2 is BlackHole) return;

            // Check distance
            double L = (go1.Location - go2.Location).Length();

            // Check for distance too large
            if (go1.Radius + go2.Radius <= L) return;

            // Determine largest and smallest
            GameObject gL, gS;
            if (go1.Radius > go2.Radius)
            {
                gL = go1;
                gS = go2;
            }
            else
            {
                gL = go2;
                gS = go1;
            }

            if (gS is AntiMatter && !(gL is AntiMatter) && gL.Is(Rule.EATABLE))
            {
                // gL moet zoveel kleiner worden als dat gS is. Hierna moet er worden gecheckt of
                double LostMass = gS.Mass * 30;

                // Check for mass of large gameobject
                if (LostMass >= gL.Mass)
                {
                    if (gL != pf.CurrentPlayer)
                        pf.BOT.Remove(gL);
                }
                else
                    gL.Mass -= LostMass;

                if (gL is Player || gS is Player)
                {
                    pf.sb.AddScore(new Score(-50, DateTime.Now, gS.Location, true));
                }


                // Remove antimatter
                pf.BOT.Remove(gS);
            }
            else
            {
                // Check for eat flags
                if (!gS.Is(Rule.EATABLE)) return;
                if (!gL.Is(gS is Player ? Rule.EAT_PLAYER : Rule.EATS)) return;

                // Check for too close
                double T = gL.Mass + gS.Mass;
                if (Math.Sqrt(T / Math.PI) > L)
                {
                    gL.Mass = T;
                    gS.Mass = 1.0;

                    // Bereken score? Animeer score!
                    if (!(gS is Player) && (gL is Player))
                    {
                        sb.AddScore(new Score(50, DateTime.Now, gS.Location, (gL == pf.CurrentPlayer)));
                    }

                    pf.BOT.Remove(gS);
                    return;
                }

                // Do magic
                double B = T;
                double temp1 = Math.PI * L * L - T;

                double C = temp1 * temp1;
                double D = Math.Sqrt(B * B - C);

                double glM = (B + D) / 2;

                // Set new velocity
                gL.DV = (gL.DV * gL.Mass + gS.DV * (glM - gL.Mass)) / glM;

                // Set new masses
                gL.Mass = glM;
                gS.Mass = (B - D) / 2;
            }
        }
    }
}
