using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlaceObjectOnGrid : MonoBehaviour
{
    public Transform gridCellPrefab;
    public Transform cube;

    public Transform onMousePrefab;
    public Vector3 smoothMousePosition;

    [SerializeField] private int height;
    [SerializeField] int width;

    private Vector3 mousePosition;
    private Node[,] nodes;
    private Plane plane;

    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
        plane = new Plane(Vector3.up, transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        GetMousePositionOnGrid();   
    }

    void GetMousePositionOnGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out var enter))
        {
            mousePosition = ray.GetPoint(enter);
            smoothMousePosition = mousePosition;
            mousePosition.y = 0;
            mousePosition = Vector3Int.RoundToInt(mousePosition);

            foreach (var node in nodes)
            {
                if (node.cellPosition == mousePosition && node.isPlaceable)
                {
                    if (Input.GetMouseButtonUp(0) && onMousePrefab != null)
                    {
                        node.isPlaceable = false;
                        onMousePrefab.GetComponent<ObjFollowMouse>().isOnGrid = true;
                        onMousePrefab.position = node.cellPosition + new Vector3(0, 0.5f, 0);
                        onMousePrefab = null;
                    }
                }
            }
        }
    }

    public void OnMouseClickOnUI()
    {
        if(onMousePrefab == null)
        {
            onMousePrefab = Instantiate(cube, mousePosition, Quaternion.identity);
        }
    }

    private void CreateGrid()
    {
        nodes = new Node[width, height];
        var name = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 worldPosition = new Vector3(i, 0, j);
                Transform obj = Instantiate(gridCellPrefab, worldPosition, Quaternion.identity);
                //Change color
                if (width % 2 != 0 && name % 2 == 0)
                {
                    obj.GetComponent<MeshRenderer>().material.color = Color.green;
                }
                else if (width % 2 == 0)
                {
                    if(i % 2 == 0 && name % 2 == 0)
                    {
                        obj.GetComponent<MeshRenderer>().material.color = Color.green;
                    }
                    else if (i % 2 != 0 && name % 2 != 0)
                    {
                        obj.GetComponent<MeshRenderer>().material.color = Color.green;
                    }
                }
                obj.name = "Cell " + name;
                nodes[i, j] = new Node(true, worldPosition, obj);
                name++;
            }
        }

    }
}
