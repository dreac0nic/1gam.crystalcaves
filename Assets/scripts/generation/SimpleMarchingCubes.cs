using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;

public class SimpleMarchingCubes : MonoBehaviour
{
	public class Square
	{
		public int Configuration;
		public Node CenterTop, CenterRight, CenterBottom, CenterLeft;
		public ControlNode TopLeft, TopRight, BottomLeft, BottomRight;

		public Square(ControlNode top_left, ControlNode top_right, ControlNode bottom_left, ControlNode bottom_right)
		{
			TopLeft = top_left;
			TopRight = top_right;
			BottomLeft = bottom_left;
			BottomRight = bottom_right;

			CenterTop = top_left.Right;
			CenterRight = bottom_right.Above;
			CenterBottom = bottom_left.Right;
			CenterLeft = bottom_left.Above;

			Configuration = 8*System.Convert.ToInt32(top_left.IsActive) + 4*System.Convert.ToInt32(top_right.IsActive) + 2*System.Convert.ToInt32(bottom_right.IsActive) + System.Convert.ToInt32(bottom_left.IsActive);
		}
	}

	public class SquareGrid
	{
		public Square[,] Squares;

		public SquareGrid(bool[,] map, float SquareSize)
		{
			int node_count_width = map.GetLength(0);
			int node_count_height = map.GetLength(1);
			int map_width = (int)(SquareSize*node_count_width);
			int map_height = (int)(SquareSize*node_count_height);

			ControlNode[,] control_nodes = new ControlNode[node_count_width, node_count_height];

			for(int x = 0; x < node_count_width; ++x) {
				for(int y = 0; y < node_count_height; ++y) {
					Vector3 node_position = new Vector3(-map_width/2.0f + SquareSize*x + SquareSize/2.0f, 0.0f, -map_height/2.0f + SquareSize*y + SquareSize/2.0f);
					control_nodes[x, y] = new ControlNode(node_position, map[x, y], SquareSize);
				}
			}

			Squares = new Square[node_count_width - 1, node_count_height - 1];

			for(int x = 0; x < node_count_width - 1; ++x) {
				for(int y = 0; y < node_count_height - 1; ++y) {
					Squares[x, y] = new Square(control_nodes[x, y + 1], control_nodes[x + 1, y + 1], control_nodes[x, y], control_nodes[x + 1, y]);
				}
			}
		}
	}

	public class Node
	{
		public Vector3 Position;
		public int VertexIndex = -1;

		public Node(Vector3 position)
		{
			Position = position;
		}
	}

	public class ControlNode : Node
	{
		public bool IsActive;
		public Node Above, Right;

		public ControlNode(Vector3 position, bool is_active, float square_size) : base(position)
		{
			IsActive = is_active;
			Above = new Node(position + Vector3.forward*square_size/2.0f);
			Right = new Node(position + Vector3.right*square_size/2.0f);
		}
	}

	public SquareGrid MapGrid;

	protected List<int> m_Triangles;
	protected List<Vector3> m_Vertices;

	[Header("DEBUG CONTROLS")]
	public bool DrawGizmos = true;

	public void Awake()
	{
		m_Triangles = new List<int>();
		m_Vertices = new List<Vector3>();
	}

	public void OnDrawGizmos()
	{
		if(MapGrid != null && DrawGizmos) {
			for(int x = 0; x < MapGrid.Squares.GetLength(0); ++x) {
				for(int y = 0; y < MapGrid.Squares.GetLength(1); ++y) {
					Gizmos.color = (MapGrid.Squares[x, y].TopLeft.IsActive ? Color.green : Color.red);
					Gizmos.DrawCube(this.transform.position + MapGrid.Squares[x, y].TopLeft.Position, 0.4f*Vector3.one);

					Gizmos.color = (MapGrid.Squares[x, y].TopRight.IsActive ? Color.green : Color.red);
					Gizmos.DrawCube(this.transform.position + MapGrid.Squares[x, y].TopRight.Position, 0.4f*Vector3.one);

					Gizmos.color = (MapGrid.Squares[x, y].BottomLeft.IsActive ? Color.green : Color.red);
					Gizmos.DrawCube(this.transform.position + MapGrid.Squares[x, y].BottomLeft.Position, 0.4f*Vector3.one);

					Gizmos.color = (MapGrid.Squares[x, y].BottomRight.IsActive ? Color.green : Color.red);
					Gizmos.DrawCube(this.transform.position + MapGrid.Squares[x, y].BottomRight.Position, 0.4f*Vector3.one);

					Gizmos.color = 0.4f*Color.black;
					Gizmos.DrawCube(this.transform.position + MapGrid.Squares[x, y].CenterTop.Position, 0.15f*Vector3.one);
					Gizmos.DrawCube(this.transform.position + MapGrid.Squares[x, y].CenterRight.Position, 0.15f*Vector3.one);
					Gizmos.DrawCube(this.transform.position + MapGrid.Squares[x, y].CenterBottom.Position, 0.15f*Vector3.one);
					Gizmos.DrawCube(this.transform.position + MapGrid.Squares[x, y].CenterLeft.Position, 0.15f*Vector3.one);
				}
			}
		}
	}

	public void GenerateMesh(bool[,] map, float square_size)
	{
		MapGrid = new SquareGrid(map, square_size);
		Mesh cave_mesh = new Mesh();
		MeshFilter mesh_filter = GetComponent<MeshFilter>();
		m_Vertices.Clear();
		m_Triangles.Clear();

		for(int x = 0; x < MapGrid.Squares.GetLength(0); ++x) {
			for(int y = 0; y < MapGrid.Squares.GetLength(1); ++y) {
				triangulateSquare(MapGrid.Squares[x, y]);
			}
		}

		if(mesh_filter) {
			mesh_filter.mesh = cave_mesh;
		}

		cave_mesh.vertices = m_Vertices.ToArray();
		cave_mesh.triangles = m_Triangles.ToArray();
		cave_mesh.RecalculateNormals();
	}

	protected void triangulateSquare(Square cell)
	{
		switch(cell.Configuration) {
			case 1:
				meshFromNodes(cell.CenterBottom, cell.BottomLeft, cell.CenterLeft);
				break;

			case 2:
				meshFromNodes(cell.CenterBottom, cell.CenterRight, cell.BottomRight);
				break;

			case 3:
				meshFromNodes(cell.CenterLeft, cell.CenterRight, cell.BottomRight, cell.BottomLeft);
				break;

			case 4:
				meshFromNodes(cell.CenterTop, cell.TopRight, cell.CenterRight);
				break;

			case 5:
				meshFromNodes(cell.CenterTop, cell.TopRight, cell.CenterRight, cell.CenterBottom, cell.BottomLeft, cell.CenterLeft);
				break;

			case 6:
				meshFromNodes(cell.CenterTop, cell.TopRight, cell.BottomRight, cell.CenterBottom);
				break;

			case 7:
				meshFromNodes(cell.CenterTop, cell.TopRight, cell.BottomRight, cell.BottomLeft, cell.CenterLeft);
				break;

			case 8:
				meshFromNodes(cell.TopLeft, cell.CenterTop, cell.CenterLeft);
				break;

			case 9:
				meshFromNodes(cell.TopLeft, cell.CenterTop, cell.CenterBottom, cell.BottomLeft);
				break;

			case 10:
				meshFromNodes(cell.TopLeft, cell.CenterTop, cell.CenterRight, cell.BottomRight, cell.CenterBottom, cell.CenterLeft);
				break;

			case 11:
				meshFromNodes(cell.TopLeft, cell.CenterTop, cell.CenterRight, cell.BottomRight, cell.BottomLeft);
				break;

			case 12:
				meshFromNodes(cell.TopLeft, cell.TopRight, cell.CenterRight, cell.CenterLeft);
				break;

			case 13:
				meshFromNodes(cell.TopLeft, cell.TopRight, cell.CenterRight, cell.CenterBottom, cell.BottomLeft);
				break;

			case 14:
				meshFromNodes(cell.TopLeft, cell.TopRight, cell.BottomRight, cell.CenterBottom, cell.CenterLeft);
				break;

			case 15:
				meshFromNodes(cell.TopLeft, cell.TopRight, cell.BottomRight, cell.BottomLeft);
				break;
		}
	}

	protected void meshFromNodes(params Node[] nodes)
	{
		// Give unassigned vertices their index...
		foreach(Node current in nodes) {
			if(current.VertexIndex < 0) {
				current.VertexIndex = m_Vertices.Count;
				m_Vertices.Add(current.Position);
			}
		}

		// Create a triangle fan from first point extending to each following.
		//*
		if(nodes.Length >= 3) {
			for(int current_node = 2; current_node < nodes.Length; ++current_node) {
				m_Triangles.Add(nodes[0].VertexIndex);
				m_Triangles.Add(nodes[current_node - 1].VertexIndex);
				m_Triangles.Add(nodes[current_node].VertexIndex);
			}
		}
	}
}
