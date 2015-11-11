using System.Text;
using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;

public class Roguelike3DRoomable : MonoBehaviour
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
		int[] cursor = new int[3];

		this.initializeRNG();
		this.initializeMap();

		cursor[1] = 1;

		// Generate a bunch of random rooms!
		for(int room_attempt = 0; room_attempt < MaxAttemptedRoomPlacements; ++room_attempt) {
			room_exists = false;
			width = m_RNG.Next(RoomWidthMin, RoomWidthMax);
			depth = m_RNG.Next(RoomDepthMin, RoomDepthMax);

			cursor[0] = m_RNG.Next(2, ResolutionWidth - width - 2);
			cursor[2] = m_RNG.Next(2, ResolutionDepth - depth - 2);

			for(int width_walker = -1; width_walker < width + 1; ++width_walker) {
				for(int depth_walker = -1; depth_walker < depth + 1; ++depth_walker) {
					if(!m_Map[cursor[0] + width_walker, cursor[1], cursor[2] + depth_walker]) {
						room_exists = true;
						break;
					}
				}
			}

			if(!room_exists) {
				this.carveRoom(cursor, width, RoomHeight, depth);
			}
		}

		// Mazewalker to create a crazy, crazy maze!
		int maze_height = 6;
		HallwayDirection direction = (HallwayDirection)System.Enum.GetValues(typeof(HallwayDirection)).GetValue(m_RNG.Next(0, 4));
		int[] current_cell = new int[2] {ResolutionWidth/2, ResolutionDepth/2};
		Stack<int> breadcrumb = new Stack<int>();
		List<HallwayDirection> valid_directions = new List<HallwayDirection>();
		int weight = 0;

		while(weight < 2500) {
			weight++;
			foreach(HallwayDirection possible_direction in System.Enum.GetValues(typeof(HallwayDirection))) {
				bool valid = false;

				switch(possible_direction) {
					case HallwayDirection.NORTH:
						valid = withinMap(current_cell[0], maze_height, current_cell[1] + 1) && m_Map[current_cell[0], maze_height, current_cell[1] + 1] &&
						        withinMap(current_cell[0] - 1, maze_height, current_cell[1] + 1) && m_Map[current_cell[0] - 1, maze_height, current_cell[1] + 1] &&
						        withinMap(current_cell[0] + 1, maze_height, current_cell[1] + 1) && m_Map[current_cell[0] + 1, maze_height, current_cell[1] + 1] &&
						        withinMap(current_cell[0], maze_height, current_cell[1] + 2) && m_Map[current_cell[0], maze_height, current_cell[1] + 2];
						break;

					case HallwayDirection.SOUTH:
						valid = withinMap(current_cell[0], maze_height, current_cell[1] - 1) && m_Map[current_cell[0], maze_height, current_cell[1] - 1] &&
					        withinMap(current_cell[0] - 1, maze_height, current_cell[1] - 1) && m_Map[current_cell[0] - 1, maze_height, current_cell[1] - 1] &&
					        withinMap(current_cell[0] + 1, maze_height, current_cell[1] - 1) && m_Map[current_cell[0] + 1, maze_height, current_cell[1] - 1] &&
					        withinMap(current_cell[0], maze_height, current_cell[1] - 2) && m_Map[current_cell[0], maze_height, current_cell[1] - 2];
						break;

					case HallwayDirection.EAST:
						valid = withinMap(current_cell[0] + 1, maze_height, current_cell[1]) && m_Map[current_cell[0] + 1, maze_height, current_cell[1]] &&
						        withinMap(current_cell[0] + 1, maze_height, current_cell[1] + 1) && m_Map[current_cell[0] + 1, maze_height, current_cell[1] + 1] &&
						        withinMap(current_cell[0] + 1, maze_height, current_cell[1] - 1) && m_Map[current_cell[0] + 1, maze_height, current_cell[1] - 1] &&
						        withinMap(current_cell[0] + 2, maze_height, current_cell[1]) && m_Map[current_cell[0] + 2, maze_height, current_cell[1]];
						break;

					case HallwayDirection.WEST:
						valid = withinMap(current_cell[0] - 1, maze_height, current_cell[1]) && m_Map[current_cell[0] - 1, maze_height, current_cell[1]] &&
						        withinMap(current_cell[0] - 1, maze_height, current_cell[1] + 1) && m_Map[current_cell[0] - 1, maze_height, current_cell[1] + 1] &&
						        withinMap(current_cell[0] - 1, maze_height, current_cell[1] - 1) && m_Map[current_cell[0] - 1, maze_height, current_cell[1] - 1] &&
						        withinMap(current_cell[0] - 2, maze_height, current_cell[1]) && m_Map[current_cell[0] - 2, maze_height, current_cell[1]];
						break;
				}

				if(valid) {
					valid_directions.Add(possible_direction);
				}
			}

			if(valid_directions.Count > 1 && CorridorTwistFactor > 0.0f && 100.0*m_RNG.NextDouble() <= CorridorTwistFactor) {
				direction = valid_directions[m_RNG.Next(0, valid_directions.Count - 1)];
			}

			if(valid_directions.Contains(direction) && withinMap(current_cell[0], maze_height, current_cell[1])) {
				m_Map[current_cell[0], maze_height, current_cell[1]] = false;

				breadcrumb.Push(current_cell[1]);
				breadcrumb.Push(current_cell[0]);

				switch(direction) {
					case HallwayDirection.NORTH:
						current_cell[1] += 1;
						break;

					case HallwayDirection.SOUTH:
						current_cell[1] -= 1;
						break;

					case HallwayDirection.EAST:
						current_cell[0] += 1;
						break;

					case HallwayDirection.WEST:
						current_cell[0] -= 1;
						break;
				}
			} else {
				if(valid_directions.Count > 0) {
					direction = valid_directions[m_RNG.Next(0, valid_directions.Count - 1)];
				} else if(breadcrumb.Count > 0) {
					current_cell[0] = breadcrumb.Pop();
					current_cell[1] = breadcrumb.Pop();
				} else {
					break;
				}
			}
		}

		// Generate mesh using marching cubes.
		GetComponent<SimpleMarchingCubes>().GenerateMesh(m_Map, CellSize);
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

	protected bool carveRoom(int[] position, int width = 2, int height = 2, int depth = 2)
	{
		// NOTE: Build carver to attempt to build around around that point regardless of possible flaws.
		if(position.GetLength(0) < 3) {
			return false;
		}

		for(int x = position[0]; x < position[0] + width; ++x) {
			for(int y = position[1]; y < position[1] + height; ++y) {
				for(int z = position[2]; z < position[2] + depth; ++z) {
					if(this.withinMap(x, y, z)) {
						m_Map[x, y, z] = false;
					}
				}
			}
		}

		return true;
	}

	protected bool carveHallway(int[] position, HallwayDirection direction = HallwayDirection.NORTH, int length = 10)
	{
		for(int depth_layer = 0; depth_layer < length; ++depth_layer) {
			for(int layer = 0; layer < HallwayHeight; ++layer) {
				for(int width_layer = 0; width_layer < HallwayWidth; ++width_layer) {
					int x = position[0];
					int y = position[1];
					int z = position[2];

					switch(direction) {
						case HallwayDirection.NORTH:
							x += -(width_layer/2);
							y += layer;
							z += depth_layer;
							break;

						case HallwayDirection.EAST:
							x += depth_layer;
							y += layer;
							z += -(width_layer/2);
							break;

						case HallwayDirection.SOUTH:
							x += -(width_layer/2);
							y += layer;
							z -= depth_layer;
							break;

						case HallwayDirection.WEST:
							x -= depth_layer;
							y += layer;
							z += -(width_layer/2);
							break;
					}

					if(this.withinMap(x, y, z)) {
						m_Map[x, y, z] = false;
					}
				}
			}
		}

		return true;
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
