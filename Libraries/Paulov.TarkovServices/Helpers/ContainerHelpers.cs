using System.Numerics;

namespace Paulov.TarkovServices.Helpers
{
    public sealed class ContainerHelpers
    {
        /// <summary>
        /// TODO: FIXME: This is purely pseudo code to get something in the containers and needs removing as it wont handle anything larger than 1x
        /// </summary>
        /// <param name="container"></param>
        /// <param name="itemWidth"></param>
        /// <param name="itemHeight"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool PlaceItemInRandomSpotInContainer(bool[,] container, int itemWidth, int itemHeight, out Vector2 position, out bool rotation)
        {
            rotation = false;
            position = new Vector2(-1, -1); // Default to an invalid position
            var placed = false;
            int x = 0, y = 0;
            var containerY = container.GetLength(0);
            var containerX = container.GetLength(1);
            int attempts = 0;
            do
            {
                x = Random.Shared.Next(0, containerX);
                y = Random.Shared.Next(0, containerY);
                if (container[y, x]) continue; // Skip if the position is already occupied

                // Check if the position is within the container bounds
                if (x < 0 || x >= containerX || y < 0 || y >= containerY)
                    continue;

                placed = true;
                container[y, x] = true; // Mark the position as occupied
                position = new Vector2(x, y);
            }
            while (!placed && attempts < 10);

            return placed;

        }
    }
}
