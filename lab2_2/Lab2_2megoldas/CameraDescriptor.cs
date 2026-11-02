using Silk.NET.Maths;

namespace Lab2_2megoldas
{
    // A szeminariumi kamera csak az origo korul tudott forogni.
    // Ez a kamera szabadon mozog: van egy helye (position), es ket szoggel adjuk meg, merre nez.
    internal class CameraDescriptor
    {
        // a kamera helye (kezdetben ugyanonnan nez, mint a lab2-1-ben)
        private Vector3D<float> position = new Vector3D<float>(5f, 4.5f, 7f);

        // balra-jobbra fordulas szoge fokban
        private double angleLeftRight = -35;

        // fel-le billenes szoge fokban
        private double angleUpDown = -28;

        // ennyit lep egy gombnyomasra
        private const float MoveStepSize = 0.5f;

        // ennyi fokot fordul egy gombnyomasra
        private const double AngleChangeStepSize = 5;

        /// <summary>
        /// Gets the position of the camera.
        /// </summary>
        public Vector3D<float> Position
        {
            get
            {
                return position;
            }
        }

        /// <summary>
        /// Gets the up vector of the camera.
        /// </summary>
        public Vector3D<float> UpVector
        {
            get
            {
                return new Vector3D<float>(0f, 1f, 0f);
            }
        }

        /// <summary>
        /// Az irany, amerre a kamera nez (a ket szogbol szamoljuk).
        /// </summary>
        public Vector3D<float> Direction
        {
            get
            {
                // fokbol radian
                double leftRight = angleLeftRight * Math.PI / 180;
                double upDown = angleUpDown * Math.PI / 180;

                var x = Math.Cos(upDown) * Math.Sin(leftRight);
                var y = Math.Sin(upDown);
                var z = -Math.Cos(upDown) * Math.Cos(leftRight);

                return new Vector3D<float>((float)x, (float)y, (float)z);
            }
        }

        /// <summary>
        /// Gets the target point of the camera view: a pont, ami a kamera elott van.
        /// </summary>
        public Vector3D<float> Target
        {
            get
            {
                return position + Direction;
            }
        }

        // elore: a nezesi iranyba lepunk
        public void MoveForward()
        {
            position = position + Direction * MoveStepSize;
        }

        // hatra: a nezesi irannyal szemben lepunk
        public void MoveBackward()
        {
            position = position - Direction * MoveStepSize;
        }

        public void MoveUp()
        {
            position = position + UpVector * MoveStepSize;
        }

        public void MoveDown()
        {
            position = position - UpVector * MoveStepSize;
        }

        public void TurnLeft()
        {
            angleLeftRight -= AngleChangeStepSize;
        }

        public void TurnRight()
        {
            angleLeftRight += AngleChangeStepSize;
        }

        public void TiltUp()
        {
            angleUpDown += AngleChangeStepSize;

            // ne lehessen teljesen felfele nezni, mert akkor atfordul a kep
            if (angleUpDown > 85)
                angleUpDown = 85;
        }

        public void TiltDown()
        {
            angleUpDown -= AngleChangeStepSize;

            if (angleUpDown < -85)
                angleUpDown = -85;
        }
    }
}
