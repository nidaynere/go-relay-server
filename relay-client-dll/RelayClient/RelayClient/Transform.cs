using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayClient
{
    namespace Client
    {
        /// <summary>
        /// Holds the position and angles of the object.
        /// </summary>
        [System.Serializable]
        public class Transform
        {
            public Transform(float[] Pos, float[] Ang)
            {
                SetPosition(Pos);
                SetAngle(Ang);
            }

            public Transform()
            {
                SetPosition(new float[3]);
                SetAngle(new float[3]);
            }

            public static void Decimal(ref float value)
            {
                value = (float)Math.Floor((double)value * 100) / 100;
            }

            /// <summary>
            /// Position axises;
            /// </summary>
            public float PX, PY, PZ;

            /// <summary>
            /// Returns position with float[3]
            /// </summary>
            /// <returns></returns>
            public float[] GetPosition()
            {
                return new float[3] { PX, PY, PZ };
            }

            /// <summary>
            /// Set poisiton of the transform.
            /// </summary>
            /// <param name="pos"></param>
            public void SetPosition(float[] pos)
            {
                for (int i=0; i<3; i++)
                    Decimal(ref pos[i]);

                PX = pos[0]; PY = pos[1]; PZ = pos[2];
            }

            /// <summary>
            /// Angle axises;
            /// </summary>
            public float RX, RY, RZ;

            /// <summary>
            /// Returns position with float[3]
            /// </summary>
            /// <returns></returns>
            public float[] GetAngle ()
            {
                return new float[3] { RX, RY, RZ };
            }

            /// <summary>
            /// Set angles of the transform.
            /// </summary>
            /// <param name="ang"></param>
            public void SetAngle (float[] ang)
            {
                for (int i = 0; i < 3; i++)
                    Decimal(ref ang[i]);

                RX = ang[0]; RY = ang[1]; RZ = ang[2];
            }
        }
    }
}
