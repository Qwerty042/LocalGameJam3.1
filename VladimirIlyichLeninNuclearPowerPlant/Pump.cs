using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace VladimirIlyichLeninNuclearPowerPlant
{
    class Pump
    {
        public float PumpSpeedPercentage
        {
            get { return pumpSpeedPercentage; }
            set
            {
                pumpSpeedPercentage = value;
                pumpSpeed = (value / 100) * maxPumpSpeed;
            }
        }
        private float pumpSpeedPercentage; //0-100%
        public float PumpRotation { get; set; } //rad
        private float pumpSpeed; //rad/sec
        private const int maxPumpSpeed = 20; //rad/sec

        public Pump(float pumpSpeedPercentage)
        {
            PumpRotation = 0;
            PumpSpeedPercentage = pumpSpeedPercentage;
        }

        public void Update(GameTime gameTime)
        {

            PumpRotation = MathHelper.WrapAngle(PumpRotation += (float)gameTime.ElapsedGameTime.TotalSeconds * pumpSpeed);
        }
    }

    class PumpSlider
    {
        public int MaxY { get; private set; }
        public int MinY { get; private set; }
        //public float CurrentY { get; private set; }
        public float Percent { get; private set; }
        public bool Dragging { get; private set; }
        public Rectangle KnobRectangle;

        public PumpSlider(Rectangle knobRectangle, int maxY, int minY, float initPercent)
        {
            KnobRectangle = knobRectangle;
            MaxY = maxY;
            MinY = minY;
            Percent = initPercent;
            KnobRectangle.Y = (int)(MaxY - (initPercent / 100) * (MaxY - MinY));
        }

        public void Update(Point mousePosition)
        {
            if (Mouse.GetState().LeftButton == ButtonState.Released)
            {
                Dragging = false;
            }
            else if (KnobRectangle.Contains(mousePosition))
            {
                Dragging = true;
            }

            if (Dragging)
            {
                KnobRectangle.Y = (int)MathHelper.Clamp(mousePosition.Y - (float)KnobRectangle.Height / 2, MinY, MaxY);
                Percent = (float)(MaxY - KnobRectangle.Y) / (MaxY - MinY) * 100;
            }
        }
    }
}
