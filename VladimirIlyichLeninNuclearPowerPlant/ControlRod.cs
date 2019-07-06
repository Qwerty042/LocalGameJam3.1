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
        private readonly Rectangle controlRodSlot;
        private readonly int minY;
        private readonly int maxY;

        private bool? dragging = null;
        private bool prevLeftClickPressed = false;
        private int dragYOffset;
        private Point prevMousePosition;

        public Rectangle rectangle;
        public Rectangle targetRectangle;
        public double targetPercentage;
        public double insertedPercentage;
        

        private const double movementSpeed = 100.0/12.0;


        public ControlRod(Rectangle _controlRodSlot, Point _controlRodSize)
        {
            controlRodSlot = _controlRodSlot;
            rectangle = new Rectangle(_controlRodSlot.Location, _controlRodSize);
            minY = _controlRodSlot.Top;
            maxY = _controlRodSlot.Bottom - _controlRodSize.Y;
            targetRectangle = rectangle;
            targetRectangle.Y = maxY;
            insertedPercentage = 100;
        }


        public void Update(Point mousePosition, GameTime gameTime)
        {
            //if (Mouse.GetState().LeftButton == ButtonState.Pressed && controlRodSlot.Contains(mousePosition))
            //{
            //    mousePosition.Y -= 10;
            //    if (mousePosition.Y > maxY)
            //    {
            //        rectangle.Y = maxY;
            //    }
            //    else if (mousePosition.Y < minY)
            //    {
            //        rectangle.Y = minY;
            //    }
            //    else
            //    {
            //        rectangle.Y = mousePosition.Y;
            //    }
            //}

            if (Mouse.GetState().LeftButton == ButtonState.Released)
            {
                dragging = false;
            }
            else if (dragging != true)
            {
                if (targetRectangle.Contains(mousePosition) && dragging != null)
                {
                    dragging = true;
                    dragYOffset = mousePosition.Y - targetRectangle.Y;
                }
                else if (rectangle.Contains(mousePosition) && dragging != null)
                {
                    dragging = true;
                    dragYOffset = mousePosition.Y - rectangle.Y;
                }
                else
                {
                    dragging = null;
                }
            }

            if (dragging == true)
            {
                int dragYPos = mousePosition.Y - dragYOffset;

                if (dragYPos > maxY)
                {
                    targetRectangle.Y = maxY;
                }
                else if (dragYPos < minY)
                {
                    targetRectangle.Y = minY;
                }
                else
                {
                    targetRectangle.Y = dragYPos;
                }
            }
            targetPercentage = (targetRectangle.Y - minY) * 100 / (maxY - minY);

            if (Math.Abs(insertedPercentage - targetPercentage) < movementSpeed * gameTime.ElapsedGameTime.TotalSeconds)
            {
                insertedPercentage = targetPercentage;
            }
            else if (insertedPercentage > targetPercentage)
            {
                insertedPercentage -= movementSpeed * gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (insertedPercentage < targetPercentage)
            {
                insertedPercentage += movementSpeed * gameTime.ElapsedGameTime.TotalSeconds;
            }

            rectangle.Y = (int)(insertedPercentage / 100 * (maxY - minY) + minY);
        }



    }
}
