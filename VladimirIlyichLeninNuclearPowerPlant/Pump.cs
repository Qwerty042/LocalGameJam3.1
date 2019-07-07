using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

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
}
