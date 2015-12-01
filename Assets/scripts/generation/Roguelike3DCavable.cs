using System.Text;
using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;

public class Roguelike3DCavable : MonoBehaviour
{
	public enum HallwayDirection { NORTH, EAST, SOUTH, WEST };

	public int ResolutionWidth = 120;
	public int ResolutionHeight = 20;
	public int ResolutionDepth = 120;

	public bool CustomSeed = false;
	public string Seed;

	public int MaxAttemptedRoomPlacements = 100;

	public int RoomWidthMin = 5;
	public int RoomWidthMax = 15;
	public int RoomDepthMin = 5;
	public int RoomDepthMax = 15;
	public int RoomHeight = 5;
	[Range(0, 100)] public double CorridorTwistFactor = 70.0;

	public int HallwayWidth = 3;
	public int HallwayHeight = 5;

	public float CellSize = 1.0f;

	[Header("DEBUG CONTROLS")]
	public bool DrawMapGizmos = true;
	public bool DrawWireframeCubes = false;
	public bool OnlyDrawActiveCubes = true;
	public bool ColorCubesWithPosition = false;
	public bool OnlyColorWithHeight = true;
	public bool OnlyDrawHighlightedLayer = true;
	public int HighlightedLayer = 0;
	public int LayerDrawCap = 0;

	protected System.Random m_RNG;
	protected bool[,,] m_Map;

	protected SimpleMarchingCubes m_SimpleMarchingCubes;

	public void Awake()
	{
		m_SimpleMarchingCubes = GetComponent<SimpleMarchingCubes>();
	}

	public void Start()
	{
		GenerateMap();
	}

	public void Update()
	{
		if(Input.GetMouseButtonDown(0)) {
			GenerateMap();
		}
	}

	public void OnDrawGizmos()
	{
		if(m_Map != null && DrawMapGizmos) {
			if(OnlyDrawHighlightedLayer) {
				for(int x = 0; x < m_Map.GetLength(0); ++x) {
					for(int z = 0; z < m_Map.GetLength(2); ++z) {
						HighlightedLayer = (HighlightedLayer < 0 ? 0 : HighlightedLayer >= m_Map.GetLength(1) ? m_Map.GetLength(1) - 1 : HighlightedLayer);
						drawMapGizmo(x, HighlightedLayer, z);
					}
				}
			} else {
				for(int x = 0; x < m_Map.GetLength(0); ++x) {
					for(int y = 0; y < m_Map.GetLength(1); ++y) {
						for(int z = 0; z < m_Map.GetLength(2); ++z) {
							if(LayerDrawCap <= 0 || y <= LayerDrawCap) {
								drawMapGizmo(x, y, z);
							}
						}
					}
				}
			}
		}
	}

	public void GenerateMap()
	{
		bool room_exists;
		int width, depth;
		int[] cursor;

		this.initializeRNG();
		this.initializeMap();

		// CAVE IT
		cursor = new int[3] {m_RNG.Next(1, ResolutionWidth - 2), 1, m_RNG.Next(1, ResolutionDepth)};

		// Generate mesh using marching cubes.
		this.GetComponent<SimpleMarchingCubes>().GenerateMesh(m_Map);
	}

	protected void drawMapGizmo(int x, int y, int z)
	{
		Vector3 cube_position = new Vector3(-ResolutionWidth/2.0f + 0.5f + x, -ResolutionHeight/2.0f + 0.5f + y, -ResolutionDepth/2.0f + 0.5f + z);

		if(ColorCubesWithPosition) {
			Color gizmo_color = Color.red*((float)x/ResolutionWidth) + Color.green*((float)y/ResolutionHeight) + Color.blue*((float)z/ResolutionDepth);

			if(OnlyColorWithHeight) {
				gizmo_color.r = 0.0f;
				gizmo_color.b = 0.0f;
			}

			gizmo_color.a = 1.0f;

			Gizmos.color = gizmo_color;
		} else {
			Gizmos.color = m_Map[x, y, z] ? Color.black : Color.white;
		}

		if(!OnlyDrawActiveCubes || m_Map[x, y, z]) {
			if(DrawWireframeCubes) {
				Gizmos.DrawWireCube(this.transform.position + cube_position, Vector3.one);
			} else {
				Gizmos.DrawCube(this.transform.position + cube_position, Vector3.one);
			}
		}
	}

	protected void initializeRNG()
	{
		if(!CustomSeed) {
			System.DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			double epoch_ms = (System.DateTime.UtcNow - epoch).TotalSeconds*1000;
			StringBuilder process_buffer = new StringBuilder();

			foreach(System.Diagnostics.Process process in System.Diagnostics.Process.GetProcesses()) {
				process_buffer.Append(process.ProcessName);
				process_buffer.Append(process.Id);
			}

			Seed = (epoch_ms.ToString().GetHashCode() + process_buffer.ToString().GetHashCode()).ToString();
		}

		m_RNG = new System.Random(Seed.GetHashCode());
	}

	protected void initializeMap()
	{
		m_Map = new bool[ResolutionWidth, ResolutionHeight, ResolutionDepth];

		for(int x = 0; x < ResolutionWidth; ++x) {
			for(int y = 0; y < ResolutionHeight; ++y) {
				for(int z = 0; z < ResolutionDepth; ++z) {
					m_Map[x, y, z] = true;
				}
			}
		}
	}

	protected bool withinMap(int x, int y, int z)
	{
		return (x >= 0 && x < ResolutionWidth && y >= 0 && y < ResolutionHeight && z >= 0 && z < ResolutionDepth);
	}

	protected int countActiveNeighbors(int x, int y, int z)
	{
		int wall_count = 0;

		for(int neighbor_x = x - 1; neighbor_x <= x + 1; ++neighbor_x) {
			for(int neighbor_y = y - 1; neighbor_y <= y + 1; ++neighbor_y) {
				for(int neighbor_z = z - 1; neighbor_z <= z + 1; ++neighbor_z) {
					if(neighbor_x < 0 || neighbor_x >= ResolutionWidth || neighbor_y < 0 || neighbor_y >= ResolutionHeight || neighbor_z < 0 || neighbor_z >= ResolutionDepth || (!(neighbor_x == x && neighbor_y == y && neighbor_z == z) && m_Map[neighbor_x, neighbor_y, neighbor_z])) {
						++wall_count;
					}
				}
			}
		}

		return wall_count;
	}
}
