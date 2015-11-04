using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;

public class SimpleMarchingCubes : MonoBehaviour
{
	public class Cube
	{
		public byte Configuration = 0;
		public ControlNode DownBackLeft, DownBackRight, DownForeLeft, DownForeRight, UpBackLeft, UpBackRight, UpForeLeft, UpForeRight;
		public Node CenterUpBack, CenterUpLeft, CenterUpFore, CenterUpRight, CenterDownBack, CenterDownLeft, CenterDownFore, CenterDownRight, MidBackLeft, MidBackRight, MidForeLeft, MidForeRight;

		public Cube(ControlNode down_back_left, ControlNode down_back_right, ControlNode down_fore_left, ControlNode down_fore_right, ControlNode up_back_left, ControlNode up_back_right, ControlNode up_fore_left, ControlNode up_fore_right)
		{
			DownBackLeft = down_back_left;
			DownBackRight = down_back_right;
			DownForeLeft = down_fore_left;
			DownForeRight = down_fore_right;
			UpBackLeft = up_back_left;
			UpBackRight = up_back_right;
			UpForeLeft = up_fore_left;
			UpForeRight = up_fore_right;

			CenterUpBack = UpBackLeft.Right;
			CenterUpLeft = UpBackLeft.Forward;
			CenterUpFore = UpForeLeft.Right;
			CenterUpRight = UpBackRight.Forward;
			CenterDownBack = DownBackLeft.Right;
			CenterDownLeft = DownBackLeft.Forward;
			CenterDownFore = DownForeLeft.Right;
			CenterDownRight = DownBackRight.Forward;
			MidBackLeft = DownBackLeft.Above;
			MidBackRight = DownBackRight.Above;
			MidForeLeft = DownForeLeft.Above;
			MidForeRight = DownForeRight.Above;

			CalculateConfiguration();
		}

		public void CalculateConfiguration()
		{
			Configuration = 0;

			for(int index = 0; index < 8; ++index) {
				Configuration += (byte)(Mathf.Pow(2.0f, (float)index)*System.Convert.ToInt32(((ControlNode)IndexToNode(index)).IsActive));
			}
		}

		public Node IndexToNode(int index)
		{
			Node selected = null;

			switch(index) {
				case 0:
					selected = DownBackLeft;
					break;

				case 1:
					selected = DownBackRight;
					break;

				case 2:
					selected = DownForeRight;
					break;

				case 3:
					selected = DownForeLeft;
					break;

				case 4:
					selected = UpBackLeft;
					break;

				case 5:
					selected = UpBackRight;
					break;

				case 6:
					selected = UpForeRight;
					break;

				case 7:
					selected = UpForeLeft;
					break;

				case 8:
					selected = CenterUpBack;
					break;

				case 9:
					selected = CenterUpLeft;
					break;

				case 10:
					selected = CenterUpFore;
					break;

				case 11:
					selected = CenterUpRight;
					break;

				case 12:
					selected = CenterDownBack;
					break;

				case 13:
					selected = CenterDownLeft;
					break;

				case 14:
					selected = CenterDownFore;
					break;

				case 15:
					selected = CenterDownRight;
					break;

				case 16:
					selected = MidBackLeft;
					break;

				case 17:
					selected = MidBackRight;
					break;

				case 18:
					selected = MidForeLeft;
					break;

				case 19:
					selected = MidForeRight;
					break;
			}

			return selected;
		}
	}

	[System.Serializable]
	public class Node
	{
		public Vector3 Position;
		public int VertexIndex = -1;

		public Node(Vector3 node_position)
		{
			Position = node_position;
		}

		public virtual void DrawGizmos()
		{
			DrawGizmos(Vector3.zero);
		}

		public virtual void DrawGizmos(Vector3 anchor_position)
		{
			Gizmos.color = 0.325f*Color.black;
			Gizmos.DrawCube(anchor_position + Position, 0.15f*Vector3.one);
		}
	}

	[System.Serializable]
	public class ControlNode : Node
	{
		public bool IsActive;
		public Node Forward, Above, Right;

		public ControlNode(Vector3 node_position, bool is_active, float cube_size) : base(node_position)
		{
			IsActive = is_active;
			Forward = new Node(node_position + Vector3.forward*cube_size/2.0f);
			Above = new Node(node_position + Vector3.up*cube_size/2.0f);
			Right = new Node(node_position + Vector3.right*cube_size/2.0f);
		}

		public override void DrawGizmos()
		{
			DrawGizmos(Vector3.zero);
		}

		public override void DrawGizmos(Vector3 anchor_position)
		{
			Gizmos.color = IsActive ? Color.green : Color.red;
			Gizmos.DrawCube(anchor_position + Position, 0.4f*Vector3.one);
		}
	}

	[Header("DEBUG CONTROLS")]
	public bool DrawGizmos = true;

	protected Cube[,,] m_CellGrid;
	protected List<int> m_Triangles;
	protected List<Vector3> m_Vertices;

	protected readonly static List<int>[] CUBE_VERTEX_LOOKUP = {
	};

	public void Awake()
	{
		m_Triangles = new List<int>();
		m_Vertices = new List<Vector3>();
	}

	public void OnDrawGizmos()
	{
		if(m_CellGrid != null && DrawGizmos) {
			for(int x = 0; x < m_CellGrid.GetLength(0); ++x) {
				for(int y = 0; y < m_CellGrid.GetLength(1); ++y) {
					for(int z = 0; z < m_CellGrid.GetLength(2); ++z) {
						for(int index = 0; index < 20; ++index) {
							m_CellGrid[x, y, z].IndexToNode(index).DrawGizmos(this.transform.position);
						}
					}
				}
			}
		}
	}

	public void Update()
	{
		if(Input.GetMouseButtonDown(1)) {
			Remesh();
		}
	}

	public Cube GetCell(int x, int y, int z)
	{
		Cube selected_cube = null;

		if(m_CellGrid != null && x >= 0 && x < m_CellGrid.GetLength(0) && y >= 0 && y < m_CellGrid.GetLength(1) && z >= 0 && z < m_CellGrid.GetLength(2)) {
			selected_cube = m_CellGrid[x, y, z];
		}

		return selected_cube;
	}

	public void GenerateMesh(bool[,,] map, float cell_size)
	{
		m_CellGrid = generateCellMap(map, cell_size);
		Mesh cave_mesh = new Mesh();
		MeshFilter mesh_filter = GetComponent<MeshFilter>();
		m_Vertices.Clear();
		m_Triangles.Clear();

		for(int x = 0; x < m_CellGrid.GetLength(0); ++x) {
			for(int y = 0; y < m_CellGrid.GetLength(1); ++y) {
				for(int z = 0; z < m_CellGrid.GetLength(2); ++z) {
					triangulateCellToLists(m_CellGrid[x, y, z]);
				}
			}
		}

		if(mesh_filter) {
			mesh_filter.mesh = cave_mesh;
		}

		cave_mesh.vertices = m_Vertices.ToArray();
		cave_mesh.triangles = m_Triangles.ToArray();
		cave_mesh.RecalculateNormals();
	}

	public void Remesh()
	{
		Mesh cave_mesh = new Mesh();
		MeshFilter mesh_filter = GetComponent<MeshFilter>();
		m_Vertices.Clear();
		m_Triangles.Clear();

		for(int x = 0; x < m_CellGrid.GetLength(0); ++x) {
			for(int y = 0; y < m_CellGrid.GetLength(1); ++y) {
				for(int z = 0; z < m_CellGrid.GetLength(2); ++z) {
					triangulateCellToLists(m_CellGrid[x, y, z]);
				}
			}
		}

		if(mesh_filter) {
			mesh_filter.mesh = cave_mesh;
		}

		cave_mesh.vertices = m_Vertices.ToArray();
		cave_mesh.triangles = m_Triangles.ToArray();
		cave_mesh.RecalculateNormals();
	}

	protected Cube[,,] generateCellMap(bool[,,] map, float cell_size = 1.0f)
	{
		Cube[,,] cells;
		int node_count_width = map.GetLength(0);
		int node_count_height = map.GetLength(1);
		int node_count_depth = map.GetLength(2);

		float map_width = cell_size*node_count_width;
		float map_height = cell_size*node_count_height;
		float map_depth = cell_size*node_count_depth;

		// Generate all the control nodes based on the given map.
		ControlNode[,,] control_nodes = new ControlNode[node_count_width, node_count_height, node_count_depth];

		for(int x = 0; x < node_count_width; ++x) {
			for(int y = 0; y < node_count_height; ++y) {
				for(int z = 0; z < node_count_depth; ++z) {
					Vector3 node_position = new Vector3(-map_width/2.0f + cell_size/2.0f + cell_size*x, -map_height/2.0f + cell_size/2.0f + cell_size*y, -map_depth/2.0f + cell_size/2.0f + cell_size*z);
					control_nodes[x, y, z] = new ControlNode(node_position, map[x, y, z], cell_size);
				}
			}
		}

		// With control and assist nodes generated, assign them to a cell for triangulazation.
		// XXX: This may be an unnecessary step as this is just used to triangulate the cell later.
		cells = new Cube[node_count_width - 1, node_count_height - 1, node_count_depth - 1];

		for(int x = 0; x < node_count_width - 1; ++x) {
			for(int y = 0; y < node_count_height - 1; ++y) {
				for(int z = 0; z < node_count_depth - 1; ++z) {
					cells[x, y, z] = new Cube(control_nodes[x, y, z], control_nodes[x + 1, y, z], control_nodes[x, y, z + 1], control_nodes[x + 1, y, z + 1], control_nodes[x, y + 1, z], control_nodes[x + 1, y + 1, z], control_nodes[x, y + 1, z + 1], control_nodes[x + 1, y + 1, z + 1]);
				}
			}
		}

		return cells;
	}

	protected void triangulateCellToLists(Cube cell)
	{
		if(cell.Configuration >= CUBE_VERTEX_LOOKUP.GetLength(0)) return;

		int index = 0;
		Node[] nodes = new Node[CUBE_VERTEX_LOOKUP[cell.Configuration].Count];

		foreach(int node_index in CUBE_VERTEX_LOOKUP[cell.Configuration]) {
			nodes[index++] = cell.IndexToNode(node_index);
		}

		appendMeshFromNodes(nodes);
	}

	protected void appendMeshFromNodes(params Node[] nodes)
	{
		if(nodes.Length < 3) return;

		// Assign a vertex index to unassigned nodes
		foreach(Node current in nodes) {
			if(current.VertexIndex < 0) {
				current.VertexIndex = m_Vertices.Count;
				m_Vertices.Add(current.Position);
			}
		}

		// Create a triangle by fanning nodes from first node.
		for(int current_node = 0; current_node < nodes.Length; ++current_node) {
			if((current_node + 1)%3 == 0) {
				m_Triangles.Add(nodes[current_node - 2].VertexIndex);
				m_Triangles.Add(nodes[current_node - 1].VertexIndex);
				m_Triangles.Add(nodes[current_node].VertexIndex);
			}
		}
	}
}
