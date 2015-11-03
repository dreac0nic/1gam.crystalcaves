using System.Text;
using System.Collections;
﻿using UnityEngine;

public class Simple3DCaveVoxelGeneration : MonoBehaviour
{
	public int ResolutionWidth = 100;
	public int ResolutionHeight = 120;
	public int ResolutionDepth = 100;
	[Range(0, 100)] public float InitialFillPercent = 50.0f;
	[Range(0, 10)] public int SmoothingPasses = 4;

	public bool CustomSeed = false;
	public string Seed;

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

	public void GenerateMap()
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
		m_Map = new bool[ResolutionWidth, ResolutionHeight, ResolutionDepth];

		seedMap();
		smoothMap(SmoothingPasses);

		if(m_SimpleMarchingCubes) {
			m_SimpleMarchingCubes.GenerateMesh(m_Map, 1.0f);
		}
	}

	protected void seedMap()
	{
		for(int x = 0; x < m_Map.GetLength(0); ++x) {
			for(int y = 0; y < m_Map.GetLength(1); ++y) {
				for(int z = 0; z < m_Map.GetLength(2); ++z) {
					if(x <= 0 || x >= ResolutionWidth - 1 || y <= 0 || y >= ResolutionHeight - 1 || z <= 0 || z >= ResolutionDepth - 1) {
						m_Map[x, y, z] = true;
					} else {
						m_Map[x, y, z] = (m_RNG.Next(0,100) < InitialFillPercent) ? true : false;
					}
				}
			}
		}
	}

	protected void smoothMap(int passes = 1)
	{
		for(int pass = 0; pass < passes; ++pass) {
			for(int x = 0; x < ResolutionWidth; ++x) {
				for(int y = 0; y < ResolutionHeight; ++y) {
					for(int z = 0; z < ResolutionDepth; ++z) {
						int neighbor_wallcount = countActiveNeighbors(x, y, z);

						//*
						if(neighbor_wallcount > 15) {
							m_Map[x, y, z] = true;
						} else if(neighbor_wallcount < 13) {
							m_Map[x, y, z] = false;
						}
						//*/

						/* Game of Life! :D
						if(!m_Map[x, y, z] && neighbor_wallcount == 3) {
							m_Map[x, y, z] = true;
						} else if(neighbor_wallcount <= 1 || neighbor_wallcount >= 4) {
							m_Map[x, y, z] = false;
						}
						//*/
					}
				}
			}
		}
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
