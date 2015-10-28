using System.Text;
using System.Collections;
﻿using UnityEngine;

public class SimpleCaveVoxelGeneration : MonoBehaviour
{
	public int ResolutionWidth = 100;
	public int ResolutionHeight = 120;
	[Range(0, 100)] public float InitialFillPercent = 50.0f;
	[Range(0, 10)] public int SmoothingPasses = 4;

	public bool CustomSeed = false;
	public string Seed;

	protected System.Random m_RNG;
	protected bool[,] m_Map;

	public void Awake()
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
		if(m_Map != null) {
			for(int x = 0; x < m_Map.GetLength(0); ++x) {
				for(int y = 0; y < m_Map.GetLength(1); ++y) {
					Gizmos.color = (m_Map[x, y] ? Color.black : Color.white);
					Vector3 position = new Vector3(-ResolutionWidth/2.0f + x, -ResolutionHeight/2.0f + y);
					Gizmos.DrawCube(this.transform.position + position, Vector3.one);
				}
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
		m_Map = new bool[ResolutionWidth, ResolutionHeight];

		seedMap();
		smoothMap(SmoothingPasses);
	}

	protected void seedMap()
	{
		for(int x = 0; x < m_Map.GetLength(0); ++x) {
			for(int y = 0; y < m_Map.GetLength(1); ++y) {
				if(x <= 0 || x >= ResolutionWidth - 1 || y <= 0 || y >= ResolutionHeight - 1) {
					m_Map[x, y] = true;
				} else {
					m_Map[x, y] = (m_RNG.Next(0,100) < InitialFillPercent) ? true : false;
				}
			}
		}
	}

	protected void smoothMap(int passes = 1)
	{
		for(int pass = 0; pass < passes; ++pass) {
			for(int x = 0; x < ResolutionWidth; ++x) {
				for(int y = 0; y < ResolutionHeight; ++y) {
					int neighbor_wallcount = countActiveNeighbors(x, y);

					//*
					if(neighbor_wallcount > 4) {
						m_Map[x, y] = true;
					} else if(neighbor_wallcount < 4) {
						m_Map[x, y] = false;
					}
					//*/

					/* Game of Life! :D
					if(!m_Map[x, y] && neighbor_wallcount == 3) {
						m_Map[x, y] = true;
					} else if(neighbor_wallcount <= 1 || neighbor_wallcount >= 4) {
						m_Map[x, y] = false;
					}
					//*/
				}
			}
		}
	}

	protected int countActiveNeighbors(int x, int y)
	{
		int wall_count = 0;

		for(int neighbor_x = x - 1; neighbor_x <= x + 1; ++neighbor_x) {
			for(int neighbor_y = y - 1; neighbor_y <= y + 1; ++neighbor_y) {
				if(neighbor_x < 0 || neighbor_x >= ResolutionWidth || neighbor_y < 0 || neighbor_y >= ResolutionHeight || (!(neighbor_x == x && neighbor_y == y) && m_Map[neighbor_x, neighbor_y])) {
					++wall_count;
				}
			}
		}

		return wall_count;
	}
}
