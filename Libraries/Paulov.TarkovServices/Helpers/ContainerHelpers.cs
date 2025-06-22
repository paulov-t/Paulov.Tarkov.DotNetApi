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

        /// <summary>
        /// TODO: This is a more complex placement algorithm that checks if the item can fit in the container, either horizontally or vertically.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="itemWidth"></param>
        /// <param name="itemHeight"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool PlaceItemInContainer(bool[,] container, int itemWidth, int itemHeight, out Vector2 position, out bool rotation)
        {
            rotation = false;
            position = new Vector2(-1, -1); // Default to an invalid position
            var placed = false;

            var minVolume = (itemWidth < itemHeight ? itemWidth : itemHeight) - 1;
            if (minVolume < 0) minVolume = 0; // Ensure minVolume is not negative

            var maxVolume = (itemWidth > itemHeight ? itemWidth : itemHeight) - 1;
            if (maxVolume < 0) maxVolume = 0; // Ensure maxVolume is not negative

            var containerY = container.GetLength(0);
            if (containerY <= minVolume) return placed; // If container is too small, return false
            var containerX = container.GetLength(1);
            if (containerX <= minVolume) return placed; // If container is too small, return false
            var limitY = containerY - minVolume;
            if (limitY <= 0) return placed; // If limitY is not positive, return false
            var limitX = containerX - minVolume;
            if (limitX <= 0) return placed; // If limitX is not positive, return false

            // Iterate through the container to find a suitable position
            for (var y = 0; y < limitY; y++)
            {
                // Across
                for (var x = 0; x < limitX; x++)
                {
                    // Check if the item can fit in the container at this position
                    if (container[y, x]) continue; // Skip if the position is already occupied
                    // Check if the item fits horizontally
                    bool fitsHorizontally = true;
                    for (var i = 0; i < itemWidth; i++)
                    {
                        if (x + i >= containerX || container[y, x + i])
                        {
                            fitsHorizontally = false;
                            break;
                        }
                    }
                    // Check if the item fits vertically
                    bool fitsVertically = true;
                    for (var j = 0; j < itemHeight; j++)
                    {
                        if (y + j >= containerY || container[y + j, x])
                        {
                            fitsVertically = false;
                            break;
                        }
                    }
                    // If it fits either way, place it
                    if (fitsHorizontally || fitsVertically)
                    {
                        placed = true;
                        position = new Vector2(x, y);
                        rotation = !fitsHorizontally; // Set rotation based on fit type
                        // Mark the positions as occupied
                        if (fitsHorizontally)
                        {
                            for (var i = 0; i < itemWidth; i++)
                                container[y, x + i] = true;
                        }
                        else
                        {
                            for (var i = 0; i < itemWidth; i++)
                                container[y, x] = true;

                            for (var j = 0; j < itemHeight; j++)
                                container[y + j, x] = true;
                        }
                        return placed;
                    }
                }

                return placed;
            }

            return placed; // Return false if no suitable position was found after all attempts

        }
    }
}
