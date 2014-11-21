﻿using System.Collections.Generic;
using System.Linq;
using Planets.Model;

namespace Planets.Controller.PhysicsRules
{
    class BlackHoleRule : AbstractGameRule
    {
        private double JoelConstante = 1.0;

        protected override void ExecuteRule(Playfield pf, double ms)
        {
            // Check for collisions with black hole

            var removed = new List<GameObject>();

            foreach (GameObject go in pf.GameObjects)
            {
                if (go is BlackHole)
                {
                    foreach (GameObject go2 in pf.GameObjects.Where(p => p.IsAffectedByBlackHole))
                    {

                        if (go != go2 && go.IntersectsWith(go2))
                        {
                            if (!(go2 is Player))
                                removed.Add(go2);
                        }
                    }
                }
            }
            removed.ForEach(g => pf.GameObjects.Remove(g));

            // Update speed to black hole
            foreach(GameObject g in pf.GameObjects){
                if (g is BlackHole){
                    foreach (GameObject g2 in pf.GameObjects.Where(p => p.IsAffectedByBlackHole && p.CanMove))
                    {
                        if (g != g2 && !(g2 is Player))
                        {
                            Vector V = g.Location - g2.Location;
                            double Fg = JoelConstante*((g2.mass*g.mass)/(V.Length()*V.Length()));
                            g2.DV += V.ScaleToLength(Fg*(ms/1000.0));
                        }
                    }
                }
            }
        }
    }
}
