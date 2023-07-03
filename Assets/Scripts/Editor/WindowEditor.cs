using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class WindowEditor : EditorWindow
{
    private const int width = 550;
    private const int height = 550;

    [MenuItem("WindowEditor/Map Editor")]
    private static void OpenWindow()
    {
        var window = GetWindow<WindowEditor>();
        window.titleContent = new GUIContent("Editor");
        window.minSize = new Vector2(width, height);
        window.Show();
    }

    ////////////////////////////////////////////////////
    private Vector2Int mapSize = new Vector2Int(100, 100);
    private GameObject groundObject;
    private GameObject cowObject;
    private GameObject stoneObject;
    private GameObject mushObject;
    private GameObject carrotObject;

    private Vector2Int start;
    private Vector2Int goal;
    private List<Vector2Int> path;

    private string saveName = ""; // New variable for save name
    private string loadName = "";

    ////////////////////////////////////////////////////
    private string[,] grid;
    private Vector2Int selectedCell;
    private const int GridSize = 1;
    private GameObject mapParent;

    ////////////////////////////////////////////////////

    private bool[,] visited;

    private void OnGUI()
    {
        mapSize = EditorGUILayout.Vector2IntField("Size", mapSize);
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        groundObject = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Cube.prefab");
        ButtonGenerateGround();

        stoneObject = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Stone.prefab");
        ButtonGenerateStone();

        cowObject = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Cow.prefab");
        ButtonGenerateCow();

        mushObject = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Mushroom.prefab");
        ButtonGenerateMush();

        carrotObject = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Carrot.prefab");
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        PathButton();
        // Save name text field
        saveName = EditorGUILayout.TextField(saveName);
        ButtonSaveFile();
        // Load name text field
        loadName = EditorGUILayout.TextField(loadName);
        ButtonLoadFile();
        EditorGUILayout.EndHorizontal();

        ////////////////////////////////////////////////////
        EditorGUILayout.Space();
        if (grid != null)
        {
            DrawGrid(path);
        }
        ////////////////////////////////////////////////////

    }
    ////////////////////////////////////////////////////
    private void DrawGrid(List<Vector2Int> path)
    {
        var gridWidth = position.width - 20f;
        var gridHeight = position.height - 200f;

        Rect rect = GUILayoutUtility.GetRect(gridWidth, gridHeight);
        //float cellSize = Mathf.Min(gridWidth / mapSize.x, gridHeight / mapSize.y);
        //Rect rect = GUILayoutUtility.GetRect(position.width, position.width / mapSize.x * mapSize.y);
        float cellSize = Mathf.Min(position.width, position.height) / Mathf.Max(mapSize.x, mapSize.y);

        Handles.color = Color.white;

        ////////////////////////////////////////////////////
        var currentEvent = Event.current;

        if (currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition))
        {
            var mousePos = currentEvent.mousePosition - rect.position;
            var cellPos = new Vector2Int((int)(mousePos.x / cellSize), (int)(mousePos.y / cellSize));

            if (currentEvent.button == 0) // Left mouse button
            {
                if (grid[cellPos.x, cellPos.y] == "ground")
                {
                    InstantiateStone(cellPos);
                }
                else if (grid[cellPos.x, cellPos.y] == "stone")
                {
                    ClearStones(cellPos);
                }
                else if (grid[cellPos.x, cellPos.y] == "cow")
                {
                    ClearCow(cellPos);

                }
                else if (grid[cellPos.x, cellPos.y] == "mush")
                {
                    ClearMush(cellPos);
                }
                Repaint();
            }
        }

        ////////////////////////////////////////////////////
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Rect cellRect = new Rect(rect.x + x * cellSize, rect.y + y * cellSize, cellSize, cellSize);

                Handles.DrawWireCube(cellRect.center, new Vector3(cellRect.width, cellRect.height));

                //Selected cell
                if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                {
                    selectedCell = new Vector2Int(x, y);
                    Repaint();
                }

                Rect selectedRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.width - 2, cellRect.height - 2);

                if (path != null && path.Count > 0)
                {
                    for (int i = 1; i < path.Count - 1; i++)
                    {
                        var positionPath = path[i];
                        grid[positionPath.x, positionPath.y] = "path";
                        InstantiateCarrot(positionPath);
                    }
                }

                if (grid[x, y] == "stone")
                {
                    EditorGUI.DrawRect(selectedRect, Color.red);
                }
                else if (grid[x, y] == "cow")
                {
                    EditorGUI.DrawRect(selectedRect, Color.blue);
                }
                else if (grid[x, y] == "mush")
                {
                    EditorGUI.DrawRect(selectedRect, Color.yellow);
                }
                else if (grid[x, y] == "path")
                {
                    EditorGUI.DrawRect(selectedRect, Color.gray);
                }
                else
                {
                    EditorGUI.DrawRect(selectedRect, Color.green);
                }
            }
        }
    }

    ////////////////////////////////////////////////////
    private void ButtonGenerateGround()
    {
        if (GUILayout.Button("Generate Ground"))
        {
            if (groundObject != null)
            {
                InstantiateGround();
            }
            else
            {
                Debug.LogWarning("Object is not selected!");
            }
        }
    }

    private void InstantiateGround()
    {
        if (mapParent != null)
        {
            DestroyImmediate(mapParent);
        }

        if (mapParent == null)
        {
            mapParent = new GameObject("MapParent");
        }

        // Clear existing cells if any
        //ClearCells();
        
        grid = new string[mapSize.x, mapSize.y];

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                InstantiateCube(position);
            }
        }
    }

    private void InstantiateCube(Vector2Int position)
    {
        if (grid[position.x, position.y] == "ground")
            return; // Cube already instantiated at this position

        grid[position.x, position.y] = "ground";

        var cellPosition = new Vector3(position.y * GridSize, 0f, position.x * GridSize);
        var cell = Instantiate(groundObject, cellPosition, Quaternion.identity);
        cell.transform.position = cellPosition;
        cell.transform.SetParent(mapParent.transform);
        cell.name = "Cell" + position.x + "_" + position.y;
    }

    private void ClearCells()
    {
        if (mapParent != null)
        {
            DestroyImmediate(mapParent);
        }

        mapParent = new GameObject("MapParent");
    }
    ////////////////////////////////////////////////////
   
    private void ButtonGenerateStone()
    {
        if (GUILayout.Button("Generate Stone"))
        {
            if (stoneObject != null)
            {
                visited = new bool[mapSize.x, mapSize.y];    // Initialize visited array
                InstantiateObstacle();
                
                // Start generating the maze from the first cell
                //GenerateFromCell(0, 0);
            }
            else
            {
                Debug.LogWarning("Object is not selected!");
            }
        }
    }

    private void InstantiateObstacle()
    {
        // Instantiate outer walls
        //for (int x = 0; x < mapSize.x; x++)
        //{
        //    InstantiateStone(new Vector2Int(x, 0));  // Top wall
        //    InstantiateStone(new Vector2Int(x, mapSize.y - 1));  // Bottom wall
        //}

        //for (int y = 1; y < mapSize.y - 1; y++)
        //{
        //    InstantiateStone(new Vector2Int(0, y));  // Left wall
        //    InstantiateStone(new Vector2Int(mapSize.x - 1, y));  // Right wall
        //}

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                var valueRandom = Random.Range(0f, 1f);

                if (grid[x, y] == "ground" && valueRandom < 0.5f) // Adjust the threshold as needed
                {
                    InstantiateStone(position);
                }
            }
        }
    }

    private void InstantiateStone(Vector2Int position)
    {
        //if (grid[position.x, position.y] == "stone")
        //    return; // Cube already instantiated at this position
        
        grid[position.x, position.y] = "stone";
        
        var cellPosition = new Vector3(position.y, 0.5f, position.x);
        var cell = Instantiate(stoneObject, cellPosition, Quaternion.identity);
        cell.transform.position = cellPosition;
        cell.transform.SetParent(mapParent.transform);
        cell.name = "Stone" + position.x + "_" + position.y;
    }

    private void ClearStones(Vector2Int position)
    {
        if (grid[position.x, position.y] != "stone")
            return; // No stone to remove

        var stonePositions = grid[position.x, position.y] = "ground";

        var stone = GameObject.Find("Stone" + position.x + "_" + position.y);
        if (stone != null)
        {
            DestroyImmediate(stone);
        }
    }

    private void RemoveStone(int x, int y, int direction)
    {
        // Calculate the coordinates of the wall to remove
        int wallX = x + GetDirectionX(direction);
        int wallY = y + GetDirectionY(direction);

        // Destroy the wall game object at the specified coordinates
        //ClearStones(new Vector2Int(wallX, wallY));
        // Check if the wall is within bounds
        if (wallX >= 0 && wallX < mapSize.x && wallY >= 0 && wallY < mapSize.y)
        {
            // Check if the wall exists
            if (grid[wallX, wallY] == "stone")
            {
                // Destroy the wall game object at the specified coordinates
                ClearStones(new Vector2Int(wallX, wallY));
            }
        }
    }

    private void GenerateFromCell(int x, int y)
    {
        visited[x, y] = true;   // Mark current cell as visited

        // Get random order of directions
        int[] directions = GetRandomDirections();

        // Iterate through the directions
        for (int i = 0; i < directions.Length; i++)
        {
            int direction = directions[i];

            // Calculate the next cell coordinates
            int nextX = x + GetDirectionX(direction);
            int nextY = y + GetDirectionY(direction);

            Debug.Log("D " + nextX + " " + nextY);

            // Check if the next cell is within bounds and unvisited
            if (nextX >= 0 && nextX < mapSize.x && nextY >= 0 && nextY < mapSize.y && !visited[nextX, nextY])
            {
                // Remove the wall between the current cell and the next 
                //RemoveStone(x, y, direction);
                InstantiateStone(new Vector2Int(x, y));
                RemoveStone(x, y, direction);
                Debug.Log(" " + grid[x,y] + " " + x + " " + y);

                // Recursively generate from the next cell
                GenerateFromCell(nextX, nextY);
            }
        }
    }

    private int[] GetRandomDirections()
    {
        int[] directions = new int[] { 0, 1, 2, 3 };   // North, East, South, West

        // Shuffle the directions array using Fisher-Yates shuffle algorithm
        for (int i = directions.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = directions[i];
            directions[i] = directions[j];
            directions[j] = temp;
        }

        return directions;
    }

    private int GetDirectionX(int direction)
    {
        // Return the x coordinate change based on the direction
        if (direction == 1)
            return 1;   // East
        else if (direction == 3)
            return -1;  // West
        else
            return 0;   // North or South
    }

    private int GetDirectionY(int direction)
    {
        // Return the y coordinate change based on the direction
        if (direction == 0)
            return 1;   // North
        else if (direction == 2)
            return -1;  // South
        else
            return 0;   // East or West
    }

    ////////////////////////////////////////////////////
    private void ButtonGenerateCow()
    {
        if (GUILayout.Button("Generate Cow"))
        {
            if (cowObject != null && grid[selectedCell.x, selectedCell.y] == "ground")
            {
                start = selectedCell;
                GenerateCow(selectedCell);
            }
            else
            {
                Debug.LogWarning("Object is not selected!");
            }
        }
    }

    private void GenerateCow(Vector2Int position)
    {
        //if (grid[position.x, position.y] == "cow")
        //    return; // Cube already instantiated at this position

        grid[position.x, position.y] = "cow";

        var cellPosition = new Vector3(position.y, 0.5f, position.x);
        var cell = Instantiate(cowObject, cellPosition, Quaternion.identity);
        cell.transform.position = cellPosition;
        cell.transform.SetParent(mapParent.transform);
        cell.name = "Cow" + position.x + "_" + position.y;
    }
    private void ClearCow(Vector2Int position)
    {
        var cowPositions = grid[position.x, position.y] = "ground";

        var cow = GameObject.Find("Cow" + position.x + "_" + position.y);
        if (cow != null)
        {
            DestroyImmediate(cow);
        }
    }
    ////////////////////////////////////////////////////

    private void ButtonGenerateMush()
    {
        if (GUILayout.Button("Generate Mushroom"))
        {
            if (mushObject != null && grid[selectedCell.x, selectedCell.y] == "ground")
            {
                goal = selectedCell;
                GenerateMush(selectedCell);
            }
            else
            {
                Debug.LogWarning("Object is not selected!");
            }
        }
    }

    private void GenerateMush(Vector2Int position)
    {
        //if (grid[position.x, position.y] == "mush")
        //    return; // Cube already instantiated at this position

        grid[position.x, position.y] = "mush";

        var cellPosition = new Vector3(position.y, 0.5f, position.x);
        var cell = Instantiate(mushObject, cellPosition, Quaternion.identity);
        cell.transform.position = cellPosition;
        cell.transform.SetParent(mapParent.transform);
        cell.name = "Mush" + position.x + "_" + position.y;
    }
    private void ClearMush(Vector2Int position)
    {
        var mushPositions = grid[position.x, position.y] = "ground";

        var mush = GameObject.Find("Mush" + position.x + "_" + position.y);
        if (mush != null)
        {
            DestroyImmediate(mush);
        }
    }
    ////////////////////////////////////////////////////

    private void PathButton()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Find Path"))
        {
            if (start != null && goal != null)
            {
                path = FindPath(start, goal);
            }
            else
            {
                Debug.LogWarning("Please select a start cell and a goal cell.");
            }
        }

        if (GUILayout.Button("Clear Path"))
        {
            for (int i = 1; i < path.Count - 1; i++)
            {
                var positionPath = path[i];
                grid[positionPath.x, positionPath.y] = "ground";
            }
            path = new List<Vector2Int>();
        }
        EditorGUILayout.EndHorizontal();
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        // TODO: Implement A* algorithm here
        var openSet = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        gScore[start] = 0;

        var fScore = new Dictionary<Vector2Int, float>();
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            var current = GetLowestFScore(openSet, fScore);

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                var tentativeGScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return new List<Vector2Int>();
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private Vector2Int GetLowestFScore(List<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore)
    {
        var lowestScore = float.MaxValue;
        Vector2Int lowestNode = openSet[0];

        foreach (var node in openSet)
        {
            if (fScore.ContainsKey(node) && fScore[node] < lowestScore)
            {
                lowestScore = fScore[node];
                lowestNode = node;
            }
        }

        return lowestNode;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        var neighbors = new List<Vector2Int>();
        if (cell.x > 0 && grid[cell.x - 1, cell.y] != "stone")
            neighbors.Add(new Vector2Int(cell.x - 1, cell.y));

        if (cell.x < mapSize.x - 1 && grid[cell.x + 1, cell.y] != "stone")
            neighbors.Add(new Vector2Int(cell.x + 1, cell.y));

        if (cell.y > 0 && grid[cell.x, cell.y - 1] != "stone")
            neighbors.Add(new Vector2Int(cell.x, cell.y - 1));

        if (cell.y < mapSize.y - 1 && grid[cell.x, cell.y + 1] != "stone")
            neighbors.Add(new Vector2Int(cell.x, cell.y + 1));

        return neighbors;
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }

    ////////////////////////////////////////////////////
    private void ButtonSaveFile()
    {
        if (GUILayout.Button("Save"))
        {
            if (string.IsNullOrEmpty(saveName))
            {
                Debug.LogWarning("Please enter a save name.");
                return;
            }
            SaveFile(saveName);
        }
    }

    private void SaveFile(string saveName)
    {
        string filePath = Path.Combine(Application.dataPath + "/Maps/", saveName + ".txt");

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine(mapSize.x + " " + mapSize.y);
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    writer.Write(grid[x, y] + " ");
                }
                writer.WriteLine();
            }
        }
    }

    private void ButtonLoadFile()
    {
        if (GUILayout.Button("Load"))
        {
            if (string.IsNullOrEmpty(loadName))
            {
                Debug.LogWarning("Please enter a load name.");
                return;
            }

            if (mapParent != null)
            {
                DestroyImmediate(mapParent);
            }

            LoadFromFile(loadName);
        }
    }

    private void LoadFromFile(string loadName)
    {
        // Load the data from the file with the provided load name
        string filePath = Path.Combine(Application.dataPath + "/Maps/", loadName + ".txt");

        if (File.Exists(filePath))
        {
            // ... load logic ...
            // Read the contents of the file
            string[] lines = File.ReadAllLines(filePath);

            // Update the grid based on the file contents
            UpdateGrid(lines);
        }
        else
        {
            Debug.LogWarning("File not found: " + filePath);
        }
    }

    private void UpdateGrid(string[] lines)
    {
        string line_size = lines[0];
        string[] cells_size = line_size.Split(' ', '\n');
        mapSize.x = int.Parse(cells_size[0]);
        mapSize.y = int.Parse(cells_size[1]);
        
        InstantiateGround();

        // Loop through the lines of the file contents
        for(int y = 1; y < lines.Length; y++)
        {
            string line = lines[y];
            string[] cells = line.Split(' ', '\n');

            for (int x = 0; x < cells.Length - 1; x++)
            {
                string cellType = cells[x];

                //Update the grid based on the cell
                Vector2Int position = new Vector2Int(x, y - 1);
                grid[x, y - 1] = cellType;

                if (grid[x, y - 1] == "stone")
                {
                    InstantiateStone(position);
                }
                else if (grid[x, y - 1] == "cow")
                {
                    GenerateCow(position);
                }
                else if (grid[x, y - 1] == "mush")
                {
                    GenerateMush(position);
                }
            }
        }
    }

    ////////////////////////////////////////////////////

    private void InstantiateCarrot(Vector2Int position)
    {
        //if (grid[position.x, position.y] == "stone")
        //    return; // Cube already instantiated at this position

        grid[position.x, position.y] = "path";

        var cellPosition = new Vector3(position.y, 0.5f, position.x);
        var cell = Instantiate(carrotObject, cellPosition, Quaternion.identity);
        cell.transform.position = cellPosition;
        cell.transform.SetParent(mapParent.transform);
        cell.name = "Path" + position.x + "_" + position.y;
    }

}
