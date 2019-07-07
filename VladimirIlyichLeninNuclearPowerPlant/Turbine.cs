using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace VladimirIlyichLeninNuclearPowerPlant
{
    class Turbine
    {
        //public float speed { get; set; } = 50;
        public float TurbineSpeedPercentage
        {
            get { return turbineSpeedPercentage; }
            set
            {
                turbineSpeedPercentage = value;
                turbineSpeed = (value / 100) * maxTurbineSpeed;
            }
        }
        private float turbineSpeedPercentage; //0-100%
        public float TurbineRotation { get; set; } //rad
        private float turbineSpeed; //rad/sec
        private const int maxTurbineSpeed = 20; //rad/sec

        public Turbine(float turbineSpeedPercentage)
        {
            TurbineRotation = 0;
            TurbineSpeedPercentage = turbineSpeedPercentage;
        }

        public void Update(GameTime gameTime)
        {

            TurbineRotation = MathHelper.WrapAngle(TurbineRotation += (float)gameTime.ElapsedGameTime.TotalSeconds * turbineSpeed);
        }
    }
}
