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
        public Vector2 Pos { get; set; }
        public string PathName { get; set; }
        public int NextWaypointIndex { get; set; }
        public Vector2 Offset { get; set; }
        public Color BubbleColor { get; set; }

        public Bubble(Vector2 pos, string pathName, int nextWaypointIndex, Vector2 offset, Color bubbleColor)
        {
            Pos = pos;
            PathName = pathName;
            NextWaypointIndex = nextWaypointIndex;
            Offset = offset;
            BubbleColor = bubbleColor;
        }
    }

    class Pipe
    {
        public Vector2[] Waypoints { get; set; }
        public float FlowVelocity { get; set; }
        public Color BubbleColor { get; set; }
        //public float BubbleFreq { get; set; }
        public double LastBubbleSpawn { get; set; }

        public Pipe(Vector2[] waypoints, float flowVelocity, Color bubbleColor/*, float bubleFreq*/)
        {
            Waypoints = waypoints;
            FlowVelocity = flowVelocity;
            BubbleColor = bubbleColor;
            //BubbleFreq = bubleFreq;
            LastBubbleSpawn = 0;
        }
    }

    class Bubbles
    {
        public List<Bubble> BubblesList { get; private set; }
        
        private Dictionary<string, Pipe> pipes;

        private float bubbleFreq = 40f;
        //private double prevTotalSeconds = 0;
        private int offsetRange = 14;
        private Random rand = new Random();

        private readonly Color waterColor = new Color(50,100,200);
        private readonly Color steamColor = new Color(220,220,255);

        private readonly Vector2[] pumpLeftPath = new Vector2[]
        {
            new Vector2(1077, 826),
            new Vector2(1077, 1498),
            new Vector2(1460, 1498),
        };

        private readonly Vector2[] pumpRightPath = new Vector2[]
        {
            new Vector2(2116, 838),
            new Vector2(2116, 1500),
            new Vector2(1728, 1500),
        };

        private readonly Vector2[] coreSteamLeftPath = new Vector2[]
        {
            new Vector2(1460, 952),
            new Vector2(1147, 952),
            new Vector2(1147, 826),
        };

        private readonly Vector2[] coreSteamRightPath = new Vector2[]
        {
            new Vector2(1728, 952),
            new Vector2(2050, 952),
            new Vector2(2050, 838),
        };

        private readonly Vector2[] turbineSteamLeftPath = new Vector2[]
        {
            new Vector2(1078, 826),
            new Vector2(1078, 661),
            new Vector2(763, 661),
            new Vector2(763, 1186),
            new Vector2(345, 1186),
            new Vector2(345, 1362),
        };

        private readonly Vector2[] turbineSteamRightPath = new Vector2[]
        {
            new Vector2(2116, 838),
            new Vector2(2116, 661),
            new Vector2(763, 661),
            new Vector2(763, 1186),
            new Vector2(345, 1186),
            new Vector2(345, 1362),
        };

        private readonly Vector2[] turbineWaterLeftPath = new Vector2[]
        {
            new Vector2(551, 1578),
            new Vector2(822, 1578),
            new Vector2(822, 826),
            new Vector2(1077, 826),
        };

        private readonly Vector2[] turbineWaterRightPath = new Vector2[]
        {
            new Vector2(551, 1578),
            new Vector2(822, 1578),
            new Vector2(822, 720),
            new Vector2(2237, 720),
            new Vector2(2237, 838),
            new Vector2(2116, 838),
        };


        public Bubbles()
        {
            pipes = new Dictionary<string, Pipe>
            {
                { "pumpLeftPath", new Pipe(pumpLeftPath, 400f, waterColor)},
                { "pumpRightPath", new Pipe(pumpRightPath, 400f, waterColor)},
                { "coreSteamLeftPath", new Pipe(coreSteamLeftPath, 400f, steamColor)},
                { "coreSteamRightPath", new Pipe(coreSteamRightPath, 400f, steamColor)},
                { "turbineSteamLeftPath", new Pipe(turbineSteamLeftPath, 400f, steamColor)},
                { "turbineSteamRightPath", new Pipe(turbineSteamRightPath, 400f, steamColor)},
                { "turbineWaterLeftPath", new Pipe(turbineWaterLeftPath, 400f, waterColor)},
                { "turbineWaterRightPath", new Pipe(turbineWaterRightPath, 400f, waterColor)},
            };
            BubblesList = new List<Bubble>();

            //start pipes with some bubbles in them already
            for (int i = 0; i < 175; i++)
            {
                foreach (KeyValuePair<string, Pipe> pipe in pipes)
                {
                    Bubble bubble = new Bubble(pipe.Value.Waypoints[0], pipe.Key, 1, new Vector2(rand.Next(-offsetRange, offsetRange + 1), rand.Next(-offsetRange, offsetRange + 1)), pipe.Value.BubbleColor);
                    BubblesList.Add(bubble);
                }

                List<Bubble> deadBubbles = new List<Bubble>();

                foreach (Bubble bubble in BubblesList)
                {
                    Pipe pipe = pipes[bubble.PathName];
                    Vector2 direction = Vector2.Normalize(pipe.Waypoints[bubble.NextWaypointIndex] - bubble.Pos);
                    bubble.Pos += direction * pipe.FlowVelocity * 0.04f;

                    if (bubble.PathName == "turbineWaterRightPath")
                    {
                        if ((bubble.Pos.X > 1030 && bubble.Pos.X < 1120) || (bubble.Pos.X > 2070 && bubble.Pos.X < 2160))
                        {
                            bubble.BubbleColor = new Color(0, 0, 0, 0);
                        }
                        else
                        {
                            bubble.BubbleColor = waterColor;
                        }
                    }

                    if (Math.Abs(Vector2.Dot(direction, Vector2.Normalize(pipe.Waypoints[bubble.NextWaypointIndex] - bubble.Pos)) + 1) < 0.1f)
                    {
                        bubble.Pos = pipe.Waypoints[bubble.NextWaypointIndex];
                        bubble.NextWaypointIndex++;
                        if (bubble.NextWaypointIndex >= pipe.Waypoints.Length)
                        {
                            deadBubbles.Add(bubble);
                        }
                    }
                }

                foreach (var bubble in deadBubbles)
                {
                    BubblesList.Remove(bubble);
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (KeyValuePair<string, Pipe> pipe in pipes)
            {
                double elapsedTime = gameTime.TotalGameTime.TotalSeconds - pipe.Value.LastBubbleSpawn;
                if (elapsedTime > (1 / bubbleFreq))
                {
                    Bubble bubble = new Bubble(pipe.Value.Waypoints[0], pipe.Key, 1, new Vector2(rand.Next(-offsetRange, offsetRange + 1), rand.Next(-offsetRange, offsetRange + 1)), pipe.Value.BubbleColor);
                    BubblesList.Add(bubble);
                    pipe.Value.LastBubbleSpawn = gameTime.TotalGameTime.TotalSeconds;
                }
            }

            List<Bubble> deadBubbles = new List<Bubble>();

            foreach (Bubble bubble in BubblesList)
            {
                Pipe pipe = pipes[bubble.PathName];
                Vector2 direction = Vector2.Normalize(pipe.Waypoints[bubble.NextWaypointIndex] - bubble.Pos);
                bubble.Pos += direction * pipe.FlowVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (bubble.PathName == "turbineWaterRightPath")
                {
                    if ((bubble.Pos.X > 1030 && bubble.Pos.X < 1120) || (bubble.Pos.X > 2070 && bubble.Pos.X < 2160))
                    {
                        bubble.BubbleColor = new Color(0, 0, 0, 0);
                    }
                    else
                    {
                        bubble.BubbleColor = waterColor;
                    }
                }

                if (Math.Abs(Vector2.Dot(direction, Vector2.Normalize(pipe.Waypoints[bubble.NextWaypointIndex] - bubble.Pos)) + 1) < 0.1f)
                {
                    bubble.Pos = pipe.Waypoints[bubble.NextWaypointIndex];
                    bubble.NextWaypointIndex++;
                    if (bubble.NextWaypointIndex >= pipe.Waypoints.Length)
                    {
                        deadBubbles.Add(bubble);
                    }
                }
            }

            foreach (var bubble in deadBubbles)
            {
                BubblesList.Remove(bubble);
            }
        }

        public void SetFlowVelocity(string pathName, float flowVelocity)
        {
            pipes[pathName].FlowVelocity = flowVelocity;
        }
    }

    //class Bubble
    //{
    //    public Point Pos { get; set; }
    //    public Color BubbleColor { get; set; }
    //    public int PathPos { get; set; }
    //    public string PathName { get; set; }
    //    public Point Offset { get; set; }

    //    public Bubble(Point pos, Color color, int pathPos, string pathName, Point offset)
    //    {
    //        Pos = pos;
    //        BubbleColor = color;
    //        PathPos = pathPos;
    //        PathName = pathName;
    //        Offset = offset;
    //    }
    //}

    //class Bubbles
    //{
    //    private Point[] leftWaterPath;
    //    //private Point[] rightWaterPath;
    //    //private Point[] rightSteamPath;
    //    //private Point[] leftSteamPath;

    //    public List<Bubble> BubblesList { get; private set; }
    //    public int FlowRate { get; set; }

    //    private readonly Color steamColor = new Color(200,200,255);
    //    private readonly Color waterColor = new Color(0,0,255);

    //    private Random rand = new Random();

    //    public Bubbles()
    //    {
    //        FlowRate = 1;
    //        leftWaterPath = ConstructLeftWaterPath();
    //        //rightWaterPath = ConstructRightWaterPath();
    //        //leftSteamPath = ConstructLeftSteamPath();
    //        //rightSteamPath = ConstructRightSteamPath();
    //        BubblesList = new List<Bubble>();
    //        //generate initial bubbles
    //        for (int i = 0; i < leftWaterPath.Length; i += 20)
    //        {
    //            Bubble bubble = new Bubble(leftWaterPath[i], waterColor, i, "leftWater", new Point(rand.Next(-8, 9), rand.Next(-8, 9)));
    //            BubblesList.Add(bubble);
    //        }
    //    }

    //    public void Update()
    //    {
    //        //update bubble positions along their line
    //        foreach (Bubble bubble in BubblesList)
    //        {
    //            bubble.PathPos += FlowRate;
    //            if (bubble.PathPos >= leftWaterPath.Length)
    //            {
    //                bubble.PathPos = 0;
    //                bubble.Offset = new Point(rand.Next(-8, 9), rand.Next(-8, 9));
    //            }
    //            bubble.Pos = leftWaterPath[bubble.PathPos];
    //        }
    //    }

    //    private Point[] ConstructLeftWaterPath()
    //    {
    //        //Waypoints: (551,1578)->(822,1578)->(822,826)->(1077,826)->(1077,1498)->(1460,1498)
    //        List<Point> path = new List<Point>();
    //        int index = 0;
    //        for (int i = 551; i <= 822; i++)
    //        {
    //            path.Add(new Point(i, 1578));
    //            index++;
    //        }
    //        for (int i = 1578; i >= 826; i--)
    //        {
    //            path.Add(new Point(822, i));
    //            index++;
    //        }
    //        for (int i = 822; i <= 1077; i++)
    //        {
    //            path.Add(new Point(i, 826));
    //            index++;
    //        }
    //        for (int i = 826; i <= 1498; i++)
    //        {
    //            path.Add(new Point(1077, i));
    //            index++;
    //        }
    //        for (int i = 1077; i <= 1460; i++)
    //        {
    //            path.Add(new Point(i, 1498));
    //            index++;
    //        }

    //        return path.ToArray();
    //    }   

    //    //private Point[] ConstructRightWaterPath()
    //    //{

    //    //}

    //    //private Point[] ConstructLeftSteamPath()
    //    //{

    //    //}

    //    //private Point[] ConstructRightSteamPath()
    //    //{
          
    //    //}
    //}
}
