using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;

namespace Sexsimiko7AIO
{
    class MinionManager
    {
        public static FarmLocation GetBestLineFarmLocation(List<Vector2> minionPositions, float width, float range)
        {
            var result = new Vector3();
            var minionCount = 0;
            var startPos = ObjectManager.Player.ServerPosition.To2D();

            var posiblePositions = new List<Vector2>();
            posiblePositions.AddRange(minionPositions);

            var max = minionPositions.Count;
            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (minionPositions[j] != minionPositions[i])
                    {
                        posiblePositions.Add((minionPositions[j] + minionPositions[i]) / 2);
                    }
                }
            }

            foreach (var pos in posiblePositions)
            {
                if (pos.Distance(startPos, true) <= range * range)
                {
                    var endPos = startPos + range * (pos - startPos).Normalized();

                    var count =
                        minionPositions.Count(pos2 => pos2.Distance(startPos, endPos, true, true) <= width * width);

                    if (count >= minionCount)
                    {
                        result = endPos.To3D();
                        minionCount = count;
                    }
                }
            }

            return new FarmLocation(result, minionCount);
        }

        public struct FarmLocation
        {
            #region Fields

            /// <summary>
            ///     The minions hit
            /// </summary>
            public int MinionsHit;

            /// <summary>
            ///     The position
            /// </summary>
            public Vector3 Position;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="FarmLocation" /> struct.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="minionsHit">The minions hit.</param>
            public FarmLocation(Vector3 position, int minionsHit)
            {
                this.Position = position;
                this.MinionsHit = minionsHit;
            }

            #endregion
        }
    }
}
