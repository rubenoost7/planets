﻿using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Planets.Controller.PhysicsRules;
using Planets.Controller.Subcontrollers;
using Planets.View;
using Planets.Model;

namespace Planets.Controller
{
    class GameEngine
    {
        // Hosts
        private MainEngine HostEngine;
        private PlanetsForm HostForm;

        // Views
        private GameView GameView;

        // Controllers
        private ShootProjectileController spc;

        // Model Data
        private Playfield field;

        // Game rules
        private AbstractGameRule[] _gameRules =
        {
            // Movement Rules (will change location of GameObjects)
            new MoveRule(),

            // Collision Rules (can change location of GameObjects)
            new BlackHoleRule(),
            

            // Do not touch the next rules, these should have the final word about movement
            new ElasticCollisionRule(),

            // Effect rules (cannot change location of GameObjects)
            new StayInFieldRule(), 

            // Reset rule
            new ResetRule(), 
        };

        // Variables
        private bool running;
        private Thread GameThread;

        private Autodemo ad;
        private Thread adthread;
        
        public GameEngine(MainEngine HostEngine, PlanetsForm HostForm)
        {
            this.HostEngine = HostEngine;
            this.HostForm = HostForm;
            this.field = new Playfield(1920, 1080);
            this.field.CurrentPlayer = new Player(200, 200, 0, 0, Utils.StartMass);

            GameView = new GameView(this.field);

            // Create new ShootProjectileController
            spc = new ShootProjectileController(field, GameView);

            this.HostEngine.SetView(GameView);

            // Adjust playfield
            field.Size = GameView.Size;

            this.ad = new Autodemo(this.field, this.spc);
            this.adthread = new Thread(ad.Run);

            GameView.KeyDown += delegate(object sender, KeyEventArgs args) { if (args.KeyData == Keys.R) field.CurrentPlayer.mass = 1.0; };
            GameView.KeyUp += delegate(object sender, KeyEventArgs args)
            {
                if (args.KeyData == Keys.K && ad.Kpressed == false)
                {
                    this.adthread = new Thread(ad.Run);
                    ad.Start();
                    adthread.Start();
                }
            };
            GameView.KeyUp += delegate(object sender, KeyEventArgs args)
            {
                if (args.KeyData == Keys.L && ad.Kpressed == true)
                {
                    ad.Stop();
                }
            };

            running = false;
            GameThread = new Thread(GameLoop);
            GameThread.Start();
        }

        public void Start()
        {
            this.running = true;
        }

        public void GameLoop()
        {
            DateTime LoopBegin = DateTime.Now;
            TimeSpan DeltaT;

            int loopcount = 0;

            while (true)
            {
                while (running)
                {
                    // Bereken DeltaT
                    DeltaT = DateTime.Now - LoopBegin;
                    LoopBegin = DateTime.Now;

                    // Converteer DeltaT naar ms
                    double dt = DeltaT.TotalMilliseconds;

                    // MOCHT GAMELOOP SNELLER ZIJN DAN +- 17MS -> DAN WACHTEN MET UPDATEN TOT 17MS is bereikt! ANDERS MEER DAN 60 FPS!!
                    double temp1 = dt * 60 / 1000;
                    double temp2 = temp1 - (int)temp1;
                    double temp3 = temp2 * 1000 / 60;
                    Thread.Sleep((int)temp3);

                    // Lock GameObjects
                    lock (field.GameObjects)
                    {
                        // ExecuteRule game rules
                        foreach (AbstractGameRule agr in _gameRules)
                        {
                            if (agr.Activated) agr.Execute(field, DeltaT.TotalMilliseconds);
                        }
                    }

                    // Update shizzle hier.
                    GameView.Invalidate();
                }
            }
        }
    }
}
