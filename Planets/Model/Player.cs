﻿using System.Drawing;

namespace Planets.Model
{
    class Player : GameObject
    {
        public Player(double x, double y, double dx, double dy, double mass) : base(x, y, dx, dy, mass, false)
        {

        }

        public Player(Vector location, Vector velocity, double mass) : base(location, velocity, mass, false)
        {

        }

        public void ShootBall(Direction dir)
        {

            Vector BallDV = new Vector(0, 0);

            double scale = this.mass / 5.0;

            switch (dir)
            {
                case Direction.up:
                    BallDV = new Vector(0 / scale, 10 / scale);
                    break;
                case Direction.down:
                    BallDV = new Vector(0 / scale, -10 / scale);
                    break;
                case Direction.left:
                    BallDV = new Vector(10 / scale, 0 / scale);
                    break;
                case Direction.right:
                    BallDV = new Vector(-10 / scale, 0 / scale);
                    break;
            }

            GameObject BallShot = new GameObject((this.Location.X + (Utils.CalcRadius(this.mass - 5) / 2)) - (Utils.CalcRadius(scale) / 2), this.Location.Y + (Utils.CalcRadius(this.mass - 5) / 2) - (Utils.CalcRadius(scale) / 2), BallDV.X, BallDV.Y, scale, true);

            this.mass -= 5;

            if(this.DV.X < 10 && this.DV.Y < 10)
                this.DV += BallDV;
        }
    }
}
