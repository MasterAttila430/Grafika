namespace Lab2_2megoldas
{
    // A felso lap forgatasanak animacioja (mint a szeminariumban: az Update-ben leptetjuk).
    internal class CubeArrangementModel
    {
        /// <summary>
        /// A felso lap jelenlegi elforgatasa fokban. Ezt rajzoljuk ki.
        /// </summary>
        public double TopLayerAngle { get; private set; } = 0;

        // ahova a felso lapnak el kell fordulnia (fokban)
        private double targetAngle = 0;

        // milyen gyorsan forog: 90 fok / masodperc
        private const double RotationSpeed = 90;

        // Space: -90 fok, Backspace: +90 fok
        public void RotateTopLayer(double angle)
        {
            // ha meg forog, nem kezdunk uj forgatast
            if (TopLayerAngle != targetAngle)
                return;

            targetAngle = targetAngle + angle;
        }

        internal void AdvanceTime(double deltaTime)
        {
            // ha mar a celnal van, nincs mit csinalni
            if (TopLayerAngle == targetAngle)
                return;

            // ennyi fokot fordul most
            double step = RotationSpeed * deltaTime;

            if (TopLayerAngle < targetAngle)
            {
                TopLayerAngle = TopLayerAngle + step;

                // ha tulment, pontosan a celnal allitjuk meg
                if (TopLayerAngle > targetAngle)
                    TopLayerAngle = targetAngle;
            }
            else
            {
                TopLayerAngle = TopLayerAngle - step;

                if (TopLayerAngle < targetAngle)
                    TopLayerAngle = targetAngle;
            }
        }
    }
}
