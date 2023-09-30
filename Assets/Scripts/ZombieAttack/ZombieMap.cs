using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Leedong.ZombieAttack
{
    public class ZombieMap : MonoBehaviour
    {
        [SerializeField]
        private Grid _grid;

        [SerializeField]
        private Tilemap _gameTilemap; // 타일맵

        [SerializeField]
        private TileBase[] _walkableTiles; // 이동 가능한 타일 배열

        private Dictionary<Vector3Int, Node> _nodeGrid = new Dictionary<Vector3Int, Node>(); // 그리드

        public Grid Grid => _grid;

        public void Init()
        {
            // 타일맵을 순회하며 그리드 생성
            BoundsInt bounds = _gameTilemap.cellBounds;

            foreach (var pos in bounds.allPositionsWithin)
            {
                Vector3Int tilePos = new Vector3Int(pos.x, pos.y, pos.z);
                TileBase tile = _gameTilemap.GetTile(tilePos);

                bool isWalkable = false;

                foreach (var walkableTile in _walkableTiles)
                {
                    if (tile == walkableTile)
                    {
                        isWalkable = true;
                        break;
                    }
                }

                Node node = new Node(tilePos, isWalkable);
                _nodeGrid.Add(tilePos, node);
            }

            // 인접한 노드 설정
            foreach (var node in _nodeGrid.Values)
            {
                node.SetNeighbours(_nodeGrid);
            }
        }

        public List<Vector3> GetPath(Vector3 startPosition, Vector3 targetPosition)
        {
            Node startNode  = _nodeGrid[_gameTilemap.WorldToCell(startPosition)];
            Node targetNode = _nodeGrid[_gameTilemap.WorldToCell(targetPosition)];

            List<Vector3> list = new List<Vector3>();

            List<Node> nodeList = FindPath(startNode, targetNode);

            if (nodeList != null)
            {
                for (int i = 0; i < nodeList.Count; i++)
                {
                    list.Add(_gameTilemap.CellToWorld(nodeList[i].position) + Vector3.one * 0.5f); // 타일 내 중간 위치값 0.5f
                }

                list.Add(targetPosition);
                
                list.Reverse();
            }

            return list;
        }

        public bool IsGrass(Vector3Int gridTilePosition)
        {
            if (_gameTilemap.GetTile(gridTilePosition) == _walkableTiles[0])
            {
                return true;
            }

            return false;
        }

        // A* 알고리즘을 사용하여 시작 위치에서 목표 위치까지 경로
        private List<Node> FindPath(Node startNode, Node targetNode)
        {
            List<Node> openList = new List<Node>();
            HashSet<Node> closedList = new HashSet<Node>();

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                Node currentNode = openList[0];

                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].fCost < currentNode.fCost || openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost)
                    {
                        currentNode = openList[i];
                    }
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                foreach (var neighbour in currentNode.neighbours)
                {
                    if (!neighbour.isWalkable || closedList.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openList.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openList.Contains(neighbour))
                        {
                            openList.Add(neighbour);
                        }
                    }
                }
            }

            return null;
        }

        // 경로를 추적하여 리스트로 반환
        private List<Node> RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            path.Reverse();
            return path;
        }

        // 두 노드 사이의 거리 계산
        private int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
            int dstY = Mathf.Abs(nodeA.position.y - nodeB.position.y);

            return dstX + dstY;
        }

        // Node
        private class Node
        {
            public Vector3Int position;
            public bool isWalkable;
            public List<Node> neighbours;
            public Node parent;

            public int gCost; // 시작 노드로부터의 이동 비용
            public int hCost; // 목표 노드까지의 예상 이동 비용
            public int fCost => gCost + hCost;

            public Node(Vector3Int position, bool isWalkable)
            {
                this.position = position;
                this.isWalkable = isWalkable;
                neighbours = new List<Node>();
            }

            public void SetNeighbours(Dictionary<Vector3Int, Node> grid)
            {
                Vector3Int[] neighbourOffsets = 
                {
                    new Vector3Int(1, 0, 0),   // R
                    new Vector3Int(-1, 0, 0),  // L
                    new Vector3Int(0, 1, 0),   // T
                    new Vector3Int(0, -1, 0),  // B
                };

                foreach (var offset in neighbourOffsets)
                {
                    Vector3Int neighbourPos = position + offset;

                    if (grid.ContainsKey(neighbourPos))
                    {
                        neighbours.Add(grid[neighbourPos]);
                    }
                }
            }
        }

    }
}