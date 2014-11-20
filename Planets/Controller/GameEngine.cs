﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
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

        // Model Data
        private Playfield field;

        // Mouse Variables
        private int MouseX;
        private int MouseY;

        // Variables
        private bool running;
        private Thread GameThread;

        public GameEngine(MainEngine HostEngine, PlanetsForm HostForm)
        {
            this.HostEngine = HostEngine;
            this.HostForm = HostForm;
            this.field = new Playfield();

            this.HostForm.Click += Form_Click;
            this.HostForm.MouseDown += Form_MouseDown;

            this.GameView = new GameView(this.field);

            running = false;
            GameThread = new Thread(GameLoop);
            GameThread.Start();
        }

        private void Form_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Form Clicked = " + MouseX + " / " + MouseY);
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            MouseX = e.X;
            MouseY = e.Y;
        }

        public void Start()
        {
            this.running = true;
        }

        public void GameLoop()
        {
            DateTime LoopBegin = DateTime.Now;
            TimeSpan DeltaT = new TimeSpan(1000/60);

            int loopcount = 0;

            while (true)
            {
                while (running)
                {

                    if(loopcount > 0)
                    {
                        DeltaT = DateTime.Now - LoopBegin;
                    }

                    LoopBegin = DateTime.Now;

                    // MOCHT GAMELOOP SNELLER ZIJN DAN +- 17MS -> DAN WACHTEN MET UPDATEN TOT 17MS is bereikt! ANDERS MEER DAN 60 FPS!!
                    if (DeltaT.Milliseconds >= 1000 / 60) 
                    {
                        Thread.Sleep(1);	
                    }

                    // PLAATS GAMELOOP HIER, voor allereerste loop is DELTA T niet beschikbaar! Bedenk dus een vaste waarde voor eerste loop!?

                    // Update shizzle hier.
                    GameView.Invalidate();
                }
                loopcount = 0;
                Thread.Sleep(50);
            }
        }
    }
}
