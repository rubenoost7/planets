﻿using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Planets.Controller.PhysicsRules;
using Planets.Model;

namespace PlanetsTest.Controller.PhysicsRules
{
    [TestClass]
    public class CollidewithSmallerTest
    {
        [TestMethod]
        public void UnitTest_CollidewithSmaller_Change()
        {
            var cws = new CollidewithSmaller();

            var go1 = new Player(new Vector(100, 100), new Vector(10, 10), 300);
            var go2 = new GameObject(new Vector(110, 110), new Vector(-5, -5), 300);
            var pf = new Playfield(300, 300);

            pf.BOT.Add(go1);
            pf.BOT.Add(go2);

            var Old_go1Mass = go1.Mass;
            var Old_go2Mass = go2.Mass;

            cws.change(go1, go2, pf);

            Assert.AreEqual(Old_go1Mass + 300.00, go1.Mass, "Collided right");
        }
    }
}
