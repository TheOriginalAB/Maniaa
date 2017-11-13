using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maniaa
{
    public struct Ball
    {
        public double PlacementDegree { get;}

        public Ball(double degree)
        {
            PlacementDegree = degree;
        }
    }

    public class Shot
    {

        public int x, y;
        public delegate void LandDelegate(Shot shot);
        public LandDelegate LandEvent;


        public Shot(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void Update(int targetY, bool isFirst)
        {
            if(y > targetY)
            {
                y -= 3;
            } else if (isFirst)
            {
                LandEvent(this);
            }
            else
            {
                y = targetY;
            }
        }
    }

    public struct Interval
    {
        public double Seconds, Speed;
        public Interval(double sec, double speed)
        {
            Seconds = sec;
            Speed = speed;
        }
    }

    public struct Level
    {
        public Ball[] Spokes;
        public Interval[] Timeframes;

        public Level(Ball[] spokes, Interval[] intervals)
        {
            Spokes = spokes;
            Timeframes = intervals;
        }
    }
}
