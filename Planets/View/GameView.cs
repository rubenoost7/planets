﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Planets.Controller.Subcontrollers;
using Planets.Model;
using Planets.Properties;
using Planets.View.Imaging;

namespace Planets.View
{
    public partial class GameView : UserControl
    {
        #region Properties

        private float _propZoom = 2.0f;
        public float Zoom
        {
            get { return _propZoom; }
            set
            {
                if (value >= 1.0f)
                    _propZoom = value;
            }
        }

        #endregion

        Playfield field;

        private SpritePool sp = new SpritePool();

        private static readonly double MaxArrowSize = 150;
        private static readonly double MinArrowSize = 50;

        // Aiming Settings
        /// <summary>
        /// If true, a vector will be drawn to show the current trajectory
        /// </summary>
        public bool IsAiming;
        public Vector AimPoint;

        // Aiming pen buffer
        private Pen CurVecPen = new Pen(Color.Red, 5);
        private Pen NextVecPen = new Pen(Color.Green, 5);
        private Pen AimVecPen = new Pen(Color.White, 5);
        private Pen BorderPen = new Pen(new TextureBrush(Resources.Texture), 10.0f);

        // Wordt gebruikt voor bewegende achtergrond
        private int _blackHoleAngle = 0;

        public GameView(Playfield field)
        {
            InitializeComponent();
            DoubleBuffered = true;
            this.field = field;
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
            CurVecPen.CustomEndCap = bigArrow;
            NextVecPen.CustomEndCap = bigArrow;
            AimVecPen.DashPattern = new float[] { 10 };
            AimVecPen.DashStyle = DashStyle.Dash;
            AimVecPen.CustomEndCap = bigArrow;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Draw static back layer
            DrawBackLayers(g);

            // Draw top layer
            DrawBorder(g);
            lock (field.BOT)
            {
                field.BOT.Iterate(obj => DrawGameObject(g, obj));
                DrawAimVectors(g);
                DrawDemo(g);
                DrawDebug(g);
            }
        }

        #region Draw Functions

        private void DrawBackLayers(Graphics g)
        {
            // Draw background
            Rectangle target;

            target = GameToScreen(new Rectangle(new Point(0, 0), ClientSize), 0.25f);
            g.DrawImageUnscaled(sp.GetSprite(Sprite.Background1, target.Width, target.Height), target);

            target = GameToScreen(new Rectangle(new Point(0, 0), ClientSize), 0.50f);
            g.DrawImageUnscaled(sp.GetSprite(Sprite.Background2, target.Width, target.Height), target);

            target = GameToScreen(new Rectangle(new Point(0, 0), ClientSize), 0.75f);
            g.DrawImageUnscaled(sp.GetSprite(Sprite.Background3, target.Width, target.Height), target);

        }

        private void DrawBorder(Graphics g)
        {
            Rectangle rp = GameToScreen(new Rectangle(new Point(), field.Size));
            g.DrawRectangle(BorderPen, rp.X - BorderPen.Width / 2, rp.Y - BorderPen.Width / 2, rp.Width + BorderPen.Width, rp.Height + BorderPen.Width);
        }

        private void DrawAimVectors(Graphics g)
        {
            GameObject obj = field.CurrentPlayer;
            if (IsAiming)
            {
                Vector CursorPosition = Cursor.Position;
                AimPoint = obj.Location - CursorPosition;

                Vector CurVec = obj.Location + obj.DV.ScaleToLength(obj.Radius + Math.Min(MaxArrowSize, Math.Max(obj.DV.Length(), MinArrowSize)));
                // Draw current direction vector
                g.DrawLine(CurVecPen, GameToScreen(obj.Location + obj.DV.ScaleToLength(obj.Radius + 1)), GameToScreen(CurVec));

                // Draw aim direction vector
                Vector AimVec = obj.Location + AimPoint.ScaleToLength(obj.Radius + Math.Min(MaxArrowSize, Math.Max(obj.DV.Length(), MinArrowSize)));
                g.DrawLine(AimVecPen, GameToScreen(obj.Location + AimPoint.ScaleToLength(obj.Radius + 1)), GameToScreen(AimVec));

                // Draw next direction vector
                Vector NextVec = ShootProjectileController.CalcNewDV(obj, new GameObject(new Vector(0, 0), new Vector(0, 0), 0.05 * obj.Mass), Cursor.Position);
                g.DrawLine(NextVecPen, GameToScreen(obj.Location + NextVec.ScaleToLength(obj.Radius + 1.0)), GameToScreen(obj.Location + NextVec.ScaleToLength(obj.Radius + Math.Min(MaxArrowSize, Math.Max(obj.DV.Length(), MinArrowSize)))));
            }
        }

        private void DrawGameObject(Graphics g, GameObject obj)
        {
            float radius = (float)obj.Radius;
            int length = (int)(radius * 2);

            // Calculate player
            /*if (obj.DV.Length() > 1.0)
            {
                int angleO = 0;
                angleO = (int)(Math.Atan2(obj.DV.X, obj.DV.Y) / Math.PI * 180.0);
                // Retrieve sprites
                Sprite cometSprite = sp.GetSprite(Sprite.CometTail, length * 4, length * 4, angleO + 180);
                g.DrawImageUnscaled(cometSprite, (int)(obj.Location.X - cometSprite.Width / 2), (int)(obj.Location.Y - cometSprite.Height / 2));
            }*/

            // Get sprite
            int spriteID;
            int objAngle = 0;

            if (obj == field.CurrentPlayer)
            {
                spriteID = Sprite.Player;
            }
            else if (obj is BlackHole)
            {
                spriteID = Sprite.BlackHole;
                objAngle = _blackHoleAngle;
                _blackHoleAngle++;
            }
            else if (obj is Stasis)
            {
                spriteID = Sprite.Stasis;
            }
            else if (obj is Antigravity)
            {
                spriteID = Sprite.BlackHole;
            }
            else if (obj is AntiMatter)
            {
                spriteID = Sprite.BlackHole;
            }
            else
            {
                spriteID = Sprite.Player;
            }

            // Draw object
            Rectangle target = GameToScreen(obj.BoundingBox);
            Sprite s = sp.GetSprite(spriteID, target.Width, target.Height, objAngle);
            g.DrawImageUnscaled(s, target);
        }

        private void DrawDemo(Graphics g)
        {
            // Drawing the autodemo
            double f = (DateTime.Now - field.LastAutoClickMoment).TotalMilliseconds;
            if (f < 1000)
            {
                int r = 20 + (int)(f / 10);
                Rectangle autoDemoEffectTarget = new Rectangle(field.LastAutoClickGameLocation.X - r / 2, field.LastAutoClickGameLocation.Y - r / 2, r, r);
                g.FillEllipse(new SolidBrush(Color.FromArgb((int)(255 - f / 1000 * 255), 255, 0, 0)), autoDemoEffectTarget);
                Point cursorPixelPoint = field.LastAutoClickGameLocation;
                g.DrawImageUnscaled(sp.GetSprite(Sprite.Cursor, 100, 100), cursorPixelPoint.X - 4, cursorPixelPoint.Y - 10);
            }
        }

        private void DrawDebug(Graphics g)
        {
            if (Debug.Enabled)
            {
                using (Pen p = new Pen(Color.OrangeRed, 2.0f))
                {
                    field.BOT.DoCollisions((go1, go2, ms) => g.DrawLine(p, go1.Location, go2.Location), 0);
                }

                int d = field.BOT.Count;
                int d2 = (d - 1) * d / 2;

                using (Brush b = new SolidBrush(Color.Magenta))
                {
                    Font f = new Font(FontFamily.GenericSansSerif, 16.0f, FontStyle.Bold);
                    g.DrawString("Regular Collision Detection: " + d2, f, b, 100, 300);
                    g.DrawString("Binary Tree Collision Detection: " + (field.BOT.ColCount), f, b, 100, 320);
                    g.DrawString("Collision Detection improvement: " + (d2 - field.BOT.ColCount) * 100 / d2 + "%", f, b, 100, 340);
                }
            }
        }

        #endregion

        #region Game / Screen conversions

        public Vector GameToScreen(Vector v, float ParallaxDepth = 1.0f)
        {
            return v;
        }

        public Vector ScreenToGame(Vector v, float ParallaxDepth = 1.0f)
        {
            return v;
        }

        public Rectangle GameToScreen(Rectangle gameRectangle, float ParallaxDepth = 1.0f)
        {
            // The game size associated with each layer
            Vector layerGameSize = new Vector(field.Size.Width, field.Size.Height);

            //=================================== [ Game Center ] =================================

            // The center of the game, if no parallax is present
            Vector noPrlxViewCenter = layerGameSize / 2;

            // The center of the game if parallax is 1.0
            Vector onePrlxViewCenter = field.CurrentPlayer.Location;

            // The corrected center of the game, with any parallaxdepth
            Vector viewCenterGame = onePrlxViewCenter * ParallaxDepth + (1.0f - ParallaxDepth) * noPrlxViewCenter;

            //=================================== [ Game View Size ] ==============================

            // View size with no parallax present
            Vector noPrlxViewSize = layerGameSize;

            // View size if parallax is 1.0
            Vector onePrlxViewSize = layerGameSize / Zoom;

            // Corrected view size with any parallaxdepth
            Vector viewSizeGame = onePrlxViewSize * ParallaxDepth + (1.0f - ParallaxDepth) * noPrlxViewSize;

            //=================================== [ Correct viewing rectangle ] ====================

            viewCenterGame = new Vector(Math.Max(viewSizeGame.X / 2, viewCenterGame.X), Math.Max(viewSizeGame.Y / 2, viewCenterGame.Y));
            viewCenterGame = new Vector(Math.Min(viewCenterGame.X, layerGameSize.X - viewSizeGame.X / 2), Math.Min(viewCenterGame.Y, layerGameSize.Y - viewSizeGame.Y / 2));

            //=================================== [ Scale to pixels ] =============================

            double scaleX = ClientSize.Width / viewSizeGame.X;
            double scaleY = ClientSize.Height / viewSizeGame.Y;

            Vector viewCenterPixel = new Vector(ClientSize.Width / 2, ClientSize.Height / 2);

            Vector rectangleSizeGame = new Vector(gameRectangle.Width, gameRectangle.Height);
            Vector rectangleCenterGame = gameRectangle.Location + rectangleSizeGame / 2;

            Vector rectangleCenterGameRelativeToCenter = rectangleCenterGame - viewCenterGame;
            Vector rectangleCenterPixelRelativeToCenter = new Vector(rectangleCenterGameRelativeToCenter.X * scaleX, rectangleCenterGameRelativeToCenter.Y * scaleY);
            Vector rectangleCenterPixel = viewCenterPixel + rectangleCenterPixelRelativeToCenter;

            Vector rectangleSizePixel = new Vector(rectangleSizeGame.X * scaleX, rectangleSizeGame.Y * scaleY);

            return new Rectangle(rectangleCenterPixel - rectangleSizePixel / 2, new Size((int)rectangleSizePixel.X, (int)rectangleSizePixel.Y));
        }

        #endregion
    }
}