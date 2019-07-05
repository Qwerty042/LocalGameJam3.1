using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace VladimirIlyichLeninNuclearPowerPlant
{
    class ControlRod
    {
        public Rectangle rodRectangle;
        private Rectangle controlRodSlot;
        private int minY;
        private int maxY;

        public int InsertedPercentage { get; private set; }


        public ControlRod(Rectangle _controlRodSlot, Point _controlRodSize)
        {
            controlRodSlot = _controlRodSlot;
            rodRectangle = new Rectangle(_controlRodSlot.Location, _controlRodSize);
            minY = _controlRodSlot.Top;
            maxY = _controlRodSlot.Bottom - _controlRodSize.Y;
        }


        public void Update(Point mousePosition)
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && controlRodSlot.Contains(mousePosition))
            {
                mousePosition.Y -= 10;
                if (mousePosition.Y > maxY)
                {
                    rodRectangle.Y = maxY;
                }
                else if (mousePosition.Y < minY)
                {
                    rodRectangle.Y = minY;
                }
                else
                {
                    rodRectangle.Y = mousePosition.Y;
                }
            }
        }
    }
}
