using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Maniaa
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D line;
        Texture2D ballTexture;
        SpriteFont spriteFont;

        List<Ball> spokeBalls;
        List<Shot> incomingShots;
        Queue<Interval> intervalQueue;
        Level currentLevel;

        double rotation;
        int startTimeOfInterval;
        Interval currentInterval;

        bool shotFired;

        bool gameover;
        int levelsCompleted;

        KeyboardState current, previous;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            graphics.PreferredBackBufferHeight = 500;
            graphics.PreferredBackBufferWidth = 400;
            rotation = 0;
            shotFired = false;
            gameover = false;
            levelsCompleted = 0;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            line = new Texture2D(GraphicsDevice, 1, 1);
            line.SetData( new Color[] { Color.White } );
            ballTexture = Content.Load<Texture2D>("circle");
            spriteFont = Content.Load<SpriteFont>("font");

            NewGame();

            previous = (current = Keyboard.GetState());
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            current = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            rotation += ToRad(currentInterval.Speed);

            if(current.IsKeyDown(Keys.Space) && previous.IsKeyUp(Keys.Space))
            {
                shotFired = true;
            }

            if (current.IsKeyDown(Keys.Enter) && previous.IsKeyUp(Keys.Enter) && gameover)
            {
                LoadLevel(currentLevel);
                gameover = false;
            }

            if (shotFired)
            {
                incomingShots[0].Update(250, true);
                for(int i = 1; i < incomingShots.Count; i++)
                {
                    incomingShots[i].Update(250 + (i * 50), false);
                }
            }

            if (gameTime.TotalGameTime.Seconds - startTimeOfInterval > currentInterval.Seconds)
            {
                intervalQueue.Enqueue(currentInterval);
                currentInterval = intervalQueue.Dequeue();
                startTimeOfInterval = gameTime.TotalGameTime.Seconds;
            }

            previous = current;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            // TODO: Add your drawing code here

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, $"Levels Completed: {levelsCompleted}", new Vector2(10, 10), Color.Black);
            spriteBatch.DrawString(spriteFont, $"{intervalQueue.Count} Intervals: {currentInterval.Seconds}, {currentInterval.Speed}", new Vector2(10, 30), Color.Black);
            if (!gameover)
            {
                foreach (Ball b in spokeBalls)
                {
                    Vector2 point = new Vector2(((float)Math.Sin(rotation + ToRad(b.PlacementDegree)) * 100) + 200, ((float)Math.Cos(rotation + ToRad(b.PlacementDegree)) * 100) + 150);
                    DrawLine(spriteBatch, new Vector2(200, 150), point);
                    DrawAtPoint(point, 15);
                }

                foreach (Shot s in incomingShots)
                {
                    DrawAtPoint(new Vector2(s.x, s.y), 15);
                }
                DrawAtPoint(new Vector2(200, 150), 100);
            } 
            
            base.Draw(gameTime);
            spriteBatch.End();
        }

        public void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end)
        {
            Vector2 edge = end - start;
            // calculate angle to rotate line
            float angle =
                (float)Math.Atan2(edge.Y, edge.X);


            sb.Draw(line,
                new Rectangle(// rectangle defines shape of line and position of start of line
                    (int)start.X,
                    (int)start.Y,
                    (int)edge.Length(), //sb will strech the texture to fill this rectangle
                    1), //width of line, change this to make thicker line
                null,
                Color.Black, //colour of line
                angle,     //angle of line (calulated above)
                new Vector2(0, 0), // point in line about which to rotate
                SpriteEffects.None,
                0);

        }

        float ToRad(double degree)
        {
            return (float)(degree * Math.PI / 180);
        }

        public void DrawAtPoint(Vector2 point, int diameter)
        {
            spriteBatch.Draw(ballTexture, RectAtPoint(point, diameter), Color.White);
        }
        
        public Rectangle RectAtPoint(Vector2 point, int diameter)
        {
            return new Rectangle((int)point.X - diameter / 2, (int)point.Y - diameter / 2, diameter, diameter);
        }

        public bool InContact()
        {
            foreach(Ball b in spokeBalls)
            {
                Rectangle r = RectAtPoint(new Vector2(((float)Math.Sin(rotation + ToRad(b.PlacementDegree)) * 100) + 200, ((float)Math.Cos(rotation + ToRad(b.PlacementDegree)) * 100) + 150), 15);
                if (r.Intersects(RectAtPoint(new Vector2(200, 250), 15))) return true;
            }
            return false;
        }

        public void LandShot(Shot shot)
        {
            gameover = InContact();
            spokeBalls.Add(new Ball(-(rotation * 180 / Math.PI)));
            shotFired = false;
            incomingShots.Remove(shot);
            if(incomingShots.Count == 0)
            {
                NewGame();
                levelsCompleted++;
            }
        }

        public void NewGame(int sections, int abundance)
        {
            spokeBalls = new List<Ball>();

            int[] arr = new int[abundance];
            int total = 0;
            rotation = 0;
            Random rand = new Random();

            for (int ball = 0; ball < abundance; ball++)
            {
                int temp = total + rand.Next((360 / sections) / abundance);
                arr[ball] = temp;
                total = temp;
            }

            for (int sect = 0; sect < sections; sect++)
            {
                foreach (int ball in arr)
                {
                    spokeBalls.Add(new Ball(sect * (360 / sections) + ball));
                }
            }

            incomingShots = new List<Shot>();

            for (int i = 0; i < 10; i++)
            {
                incomingShots.Add(new Shot(200, (i * 50) + 300));
                incomingShots[i].LandEvent = LandShot;
            }

            intervalQueue = new Queue<Interval>();

            startTimeOfInterval = 0;
            int amountOfIntervals = rand.Next(1,6);
            for(int i = 0; i < amountOfIntervals; i++)
            {
                Interval temp = new Interval(rand.Next(1, 3), rand.Next(-30, 30) / 10.0);
                while(temp.Speed == 0)
                {
                    temp.Speed = rand.Next(-30, 30) / 10.0;
                }
                intervalQueue.Enqueue(temp);
            }
            currentInterval = intervalQueue.Dequeue();
            intervalQueue.Enqueue(currentInterval);

            Ball[] tempBalls = new Ball[spokeBalls.Count];

            for (int i = 0; i < tempBalls.Length; i++)
            {
                tempBalls[i] = spokeBalls[i];
            }
            Interval[] tempIntervals = new Interval[intervalQueue.Count];
            for (int i = 0; i < tempIntervals.Length; i++)
            {
                tempIntervals[i] = intervalQueue.Dequeue();
                intervalQueue.Enqueue(tempIntervals[i]);
            }
            currentLevel = new Level(tempBalls, tempIntervals);
        }

        public void NewGame()
        {
            Random r = new Random();
            NewGame(r.Next(1, 6), r.Next(1, 5));
        }

        public void LoadLevel(Level level)
        {
            spokeBalls = new List<Ball>();
            intervalQueue = new Queue<Interval>();
            startTimeOfInterval = 0;
            gameover = false;
            currentLevel = level;
            

            foreach(Ball b in level.Spokes)
            {
                spokeBalls.Add(b);
            }

            foreach (Interval interval in level.Timeframes)
            {
                intervalQueue.Enqueue(interval);
            }

            currentInterval = intervalQueue.Dequeue();
            intervalQueue.Enqueue(currentInterval);

            incomingShots = new List<Shot>();

            for (int i = 0; i < 10; i++)
            {
                incomingShots.Add(new Shot(200, (i * 50) + 300));
                incomingShots[i].LandEvent = LandShot;
            }

        }
    }
}
