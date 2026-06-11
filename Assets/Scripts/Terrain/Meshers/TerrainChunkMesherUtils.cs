using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Minecraft
{
    public static class TerrainChunkMesherUtils
    {
        /// <returns>All valid neighboring blocks sharing a border with input chunk</returns>
        public static NativeHashMap<int3, TerrainBlock> GetBorderNeighbors(TerrainChunk chunk, Allocator allocator)
        {
            Vector3Int chunkOrigin = chunk.Index * chunk.Size;

            // 1. Iterate the local shell offsets
            var neighborGrid = ChunkBorderNeighborGrid;
            var neighbors = new NativeHashMap<int3, TerrainBlock>(neighborGrid.Length, allocator);

            foreach (int3 offset in neighborGrid)
            {
                Vector3Int worldPos = chunkOrigin + new Vector3Int(offset.x, offset.y, offset.z);
                
                if (!TerrainManager.Instance.TryGetChunkAt(worldPos, out var neighborChunk))
                    continue;

                // 2. Shift to local coordinates of neighboring chunk
                Vector3Int neighborChunkOrigin = (neighborChunk.Index * neighborChunk.Size);
                Vector3Int local = worldPos - neighborChunkOrigin;

                if (neighborChunk.GetBlock(local) is { } block)
                {
                    neighbors.TryAdd(offset, block);
                }

                // debug/visualize the shell positions
                // if (chunkOrigin == default)
                //     Debug.DrawLine(local, local + Vector3Int.up, Color.blue, 15f);
            }

            return neighbors;
        }

        private static int3[] _chunkBorderNeighborGrid;
        private static int3[] ChunkBorderNeighborGrid => _chunkBorderNeighborGrid ??= BakeShellGrid();

        private static int3[] BakeShellGrid()
        {
            Vector3Int size = TerrainManager.Instance.TerrainConfig.chunkSize;

            int minX = 0;
            int minY = 0;
            int minZ = 0;

            int maxX = size.x - 1;
            int maxY = size.y - 1;
            int maxZ = size.z - 1;

            var shell = new List<int3>();

            for (int x = minX - 1; x <= maxX + 1; x++)
            for (int y = minY - 1; y <= maxY + 1; y++)
            for (int z = minZ - 1; z <= maxZ + 1; z++)
            {
                bool inside =
                    x >= minX && x <= maxX &&
                    y >= minY && y <= maxY &&
                    z >= minZ && z <= maxZ;

                if (!inside)
                    shell.Add(new int3(x, y, z));
            }

            return shell.ToArray();
        }
    }
}