using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace VladimirIlyichLeninNuclearPowerPlant
{
    class Bubble
    {
        public Point Pos { get; set; }
        public Color BubbleColor { get; set; }
        public int PathPos { get; set; }
        public string PathName { get; set; }
        public Point Offset { get; set; }

        public Bubble(Point pos, Color color, int pathPos, string pathName, Point offset)
        {
            Pos = pos;
            BubbleColor = color;
            PathPos = pathPos;
            PathName = pathName;
            Offset = offset;
        }
    }

    class Bubbles
    {
        private Point[] leftWaterPath;
        //private Point[] rightWaterPath;
        //private Point[] rightSteamPath;
        //private Point[] leftSteamPath;

        public List<Bubble> BubblesList { get; private set; }
        public int FlowRate { get; set; }

        private readonly Color steamColor = new Color(200,200,255);
        private readonly Color waterColor = new Color(0,0,255);

        private Random rand = new Random();

        public Bubbles()
        {
            FlowRate = 1;
            leftWaterPath = ConstructLeftWaterPath();
            //rightWaterPath = ConstructRightWaterPath();
            //leftSteamPath = ConstructLeftSteamPath();
            //rightSteamPath = ConstructRightSteamPath();
            BubblesList = new List<Bubble>();
            //generate initial bubbles
            for (int i = 0; i < leftWaterPath.Length; i += 20)
            {
                Bubble bubble = new Bubble(leftWaterPath[i], waterColor, i, "leftWater", new Point(rand.Next(-8, 9), rand.Next(-8, 9)));
                BubblesList.Add(bubble);
            }
        }

        public void Update()
        {
            //update bubble positions along their line
            foreach (Bubble bubble in BubblesList)
            {
                bubble.PathPos += FlowRate;
                if (bubble.PathPos >= leftWaterPath.Length)
                {
                    bubble.PathPos = 0;
                    bubble.Offset = new Point(rand.Next(-8, 9), rand.Next(-8, 9));
                }
                bubble.Pos = leftWaterPath[bubble.PathPos];
            }
        }

        private Point[] ConstructLeftWaterPath()
        {
            //Waypoints: (551,1578)->(822,1578)->(822,826)->(1077,826)->(1077,1498)->(1460,1498)
            List<Point> path = new List<Point>();
            int index = 0;
            for (int i = 551; i <= 822; i++)
            {
                path.Add(new Point(i, 1578));
                index++;
            }
            for (int i = 1578; i >= 826; i--)
            {
                path.Add(new Point(822, i));
                index++;
            }
            for (int i = 822; i <= 1077; i++)
            {
                path.Add(new Point(i, 826));
                index++;
            }
            for (int i = 826; i <= 1498; i++)
            {
                path.Add(new Point(1077, i));
                index++;
            }
            for (int i = 1077; i <= 1460; i++)
            {
                path.Add(new Point(i, 1498));
                index++;
            }

            return path.ToArray();
        }   

        //private Point[] ConstructRightWaterPath()
        //{

        //}

        //private Point[] ConstructLeftSteamPath()
        //{

        //}

        //private Point[] ConstructRightSteamPath()
        //{
          
        //}
    }
}
