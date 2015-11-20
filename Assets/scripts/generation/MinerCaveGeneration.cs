using System.Text;
using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;

public class MinerCaveGeneration : MonoBehaviour
{
	protected class ReferenceMap
	{
		public enum Direction { UP, DOWN, LEFT, RIGHT };

		public class Miner
		{
			public bool IsAlive = true;
			public int x;
			public int y;

			public Miner(int start_x, int start_y)
			{
				this.x = start_x;
				this.y = start_y;
			}

			public bool Mine(Cell[,] world, System.Random rng)
			{
				if(this.IsAlive) {
					List<int[]> minable_directions = GetSolidNeighbors(world, this.x, this.y);

					if(minable_directions.Count > 0) {
						int[] new_position = minable_directions[rng.Next(minable_directions.Count)];

						this.x = new_position[0];
						this.y = new_position[1];
						world[this.x, this.y].IsSolid = false;
					} else {
						this.IsAlive = false;
					}
				}

				return this.IsAlive;
			}
		}

		public class Cell
		{
			public enum TileSpawn { NONE, START, EXIT, ENEMY, ITEM, GOLD, SUPPLIES };

			public bool IsSolid = true;
			public bool IsVisible = false;
			public Cell.TileSpawn Type = Cell.TileSpawn.NONE;
			public int Safety = System.Int32.MaxValue;
			public double SafetyNormalized = 1.0;
		}

		protected int m_Width = 100;
		protected int m_Height = 80;
		protected float m_MinerSpawnRate = 0.15f;
		protected int m_MinerTimeoutLimit = 400;
		protected int m_SmoothingPassCount = 2;
		protected int m_MaximumSafetyLimit = 50;

		protected System.Random m_RNG;
		protected Cell[,] m_Map;

		public static List<int[]> GetNeighbors(Cell[,] map, int x, int y)
		{
			List<int[]> neighbors = new List<int[]>();

			foreach(Direction move_direction in System.Enum.GetValues(typeof(Direction))) {
				int new_x = x;
				int new_y = y;

				switch(move_direction) {
					case Direction.UP:
						new_y -= 1;
						break;

					case Direction.DOWN:
						new_y += 1;
						break;

					case Direction.LEFT:
						new_x -= 1;
						break;

					case Direction.RIGHT:
						new_x += 1;
						break;
				}

				if(new_x >= 0 && new_x < map.GetLength(0) && new_y >= 0 && new_y < map.GetLength(1)) {
					neighbors.Add(new int[] {new_x, new_y});
				}
			}

			return neighbors;
		}

		public static List<int[]> GetSolidNeighbors(Cell[,] map, int x, int y)
		{
			List<int[]> solid_neighbors = new List<int[]>();

			foreach(int[] neighbor in ReferenceMap.GetNeighbors(map, x, y)) {
				if(map[neighbor[0], neighbor[1]].IsSolid) {
					solid_neighbors.Add(neighbor);
				}
			}

			return solid_neighbors;
		}

		public static int CountSolidAdjacents(Cell[,] map, int x, int y)
		{
			int solid_neighbors = 0;

			for(int adjacent_x = x - 1; adjacent_x <= x + 1; ++adjacent_x) {
				for(int adjacent_y = y - 1; adjacent_y <= y + 1; ++adjacent_y) {
					if(adjacent_x == x && adjacent_y == y) {
						continue;
					}

					if(adjacent_x < 0 || adjacent_x >= map.GetLength(0) || adjacent_y < 0 || adjacent_y >= map.GetLength(1) || map[adjacent_x, adjacent_y].IsSolid) {
						solid_neighbors++;
					}
				}
			}

			return solid_neighbors;
		}

		public ReferenceMap(string seed, int width, int height, float miner_spawn_rate, int miner_timeout_limit, int smoothing_pass_count, int maximum_safety_limit)
		{
			m_Width = width;
			m_Height = height;
			m_MinerSpawnRate = miner_spawn_rate;
			m_MinerTimeoutLimit = miner_timeout_limit;
			m_SmoothingPassCount = smoothing_pass_count;
			m_MaximumSafetyLimit = maximum_safety_limit;

			m_RNG = new System.Random(seed.GetHashCode());

			this.initializeMap();
			this.generateMap();
		}

		public List<int[]> GetNeighbors(int x, int y)
		{
			return ReferenceMap.GetNeighbors(m_Map, x, y);
		}

		public List<int[]> GetSolidNeighbors(int x, int y)
		{
			return ReferenceMap.GetSolidNeighbors(m_Map, x, y);
		}

		public int CountSolidAdjacents(int x, int y)
		{
			return ReferenceMap.CountSolidAdjacents(m_Map, x, y);
		}

		public double RenormalizeSafety(int new_maximum)
		{
			int cell_count = 0;
			double safety_total = 0.0;

			for(int x = 0; x < m_Width; ++x) {
				for(int y = 0; y < m_Height; ++y) {
					if(!m_Map[x, y].IsSolid) {
						m_Map[x, y].SafetyNormalized = (double)m_Map[x, y].Safety/new_maximum;

						++cell_count;
						safety_total = m_Map[x, y].SafetyNormalized;
					} else {
						m_Map[x, y].Safety = System.Int32.MaxValue;
						m_Map[x, y].SafetyNormalized = 1.0;
					}
				}
			}

			m_MaximumSafetyLimit = new_maximum;

			return safety_total/cell_count;
		}

		public void DrawGizmos(Vector3 anchor)
		{
			if(m_Map != null) {
				for(int x = 0; x < m_Width; ++x) {
					for(int y = 0; y < m_Height; ++y) {
						Gizmos.color = Color.magenta;
						Vector3 position = new Vector3(-m_Width/2.0f + 0.5f + x, 0.0f, -m_Height/2.0f + 0.5f + y);

						//ONE, START, EXIT, ENEMY, ITEM, GOLD, SUPPLIES
						switch(m_Map[x, y].Type) {
							case Cell.TileSpawn.NONE:
								if(m_Map[x, y].IsSolid) {
									Gizmos.color = Color.black;
								} else if(m_Map[x, y].IsVisible) {
									Gizmos.color = Color.yellow;
								} else {
									Gizmos.color = new Color((float)m_Map[x, y].SafetyNormalized, 0.0f, 1.0f - (float)m_Map[x, y].SafetyNormalized);
								}
								break;

							case Cell.TileSpawn.START:
								Gizmos.color = Color.green;
								break;

							case Cell.TileSpawn.EXIT:
								Gizmos.color = Color.red;
								break;

							case Cell.TileSpawn.ENEMY:
								Gizmos.color = new Color(1.0f, 0.325f, 0.0f, 0.0f);
								break;

							case Cell.TileSpawn.ITEM:
								Gizmos.color = new Color((float)(Mathf.Sin(Time.time) + 1.0)/2.0f, 0.0f, (float)(Mathf.Cos(Time.time) + 1.0)/2.0f);
								break;

							case Cell.TileSpawn.GOLD:
								Gizmos.color = Color.yellow;
								break;

							case Cell.TileSpawn.SUPPLIES:
								Gizmos.color = Color.cyan;
								break;
						}

						Gizmos.DrawCube(anchor + position, Vector3.one);
					}
				}
			}
		}

		protected void initializeMap()
		{
			m_Map = new Cell[m_Width, m_Height];

			for(int x = 0; x < m_Width; ++x) {
				for(int y = 0; y < m_Height; ++y) {
					m_Map[x, y] = new Cell();
				}
			}
		}

		protected void generateMap()
		{
			int[] entrance = new int[2] { m_Width/2, m_Height/2 };
			int[] exit;

			digCaverns(entrance[0], entrance[1]);
			smoothCaverns();

			applySafetyZone(entrance[0], entrance[1], 3);
			applyVisibility(entrance[0], entrance[1], 0.25f);

			exit = findSuitableExit();
			m_Map[exit[0], exit[1]].Type = Cell.TileSpawn.EXIT;
			applySafetyZone(exit[0], exit[1], 2);
		}

		protected void digCaverns(int start_x, int start_y)
		{
			int miners_spawned = 1;
			List<Miner> miners = new List<Miner>();
			miners.Add(new Miner(start_x, start_y));

			m_Map[start_x, start_y].IsSolid = false;
			m_Map[start_x, start_y].Type = Cell.TileSpawn.START;

			while(miners.Count > 0 && miners_spawned < m_MinerTimeoutLimit) {
				List<Miner> miner_buffer = new List<Miner>();

				foreach(Miner grunt in miners) {
					if(grunt.Mine(m_Map, m_RNG)) {
						miner_buffer.Add(grunt);

						if(m_RNG.NextDouble() <= m_MinerSpawnRate) {
							List<int[]> valid_spaces = GetSolidNeighbors(grunt.x, grunt.y);

							if(valid_spaces.Count > 0) {
								int[] spawn_space = valid_spaces[m_RNG.Next(valid_spaces.Count)];

								m_Map[spawn_space[0], spawn_space[1]].IsSolid = false;
								miner_buffer.Add(new Miner(spawn_space[0], spawn_space[1]));
								++miners_spawned;
							}
						}
					}
				}

				if(miner_buffer.Count <= 0) {
					List<int[]> possible_dig_sites = new List<int[]>();

					for(int x = 1; x < m_Width - 1; ++x) {
						for(int y = 1; y < m_Height - 1; ++y) {
							if(!m_Map[x, y].IsSolid && GetSolidNeighbors(x, y).Count > 0) {
								possible_dig_sites.Add(new int[2] { x, y });
							}
						}
					}

					if(possible_dig_sites.Count > 0) {
						int[] new_site = possible_dig_sites[m_RNG.Next(possible_dig_sites.Count)];

						miner_buffer.Add(new Miner(new_site[0], new_site[1]));
					}
				}

				miners = miner_buffer;
			}
		}

		// NOTE: Possibly improve smoothing to take advantage of extra passes or special shaping.
		protected void smoothCaverns()
		{
			for(int pass = 0; pass < m_SmoothingPassCount; ++pass) {
				for(int x = 0; x < m_Width; ++x) {
					for(int y = 0; y < m_Height; ++y) {
						if(m_Map[x, y].IsSolid && CountSolidAdjacents(x, y) <= 2) {
							m_Map[x, y].IsSolid = false;
						}
					}
				}
			}
		}

		protected void applySafetyZone(int x, int y, int strength)
		{
			Queue<int[]> cells = new Queue<int[]>();

			m_Map[x, y].Safety = 0;
			cells.Enqueue(new int[2] { x, y });

			while(cells.Count > 0) {
				int[] current = cells.Dequeue();
				int potential_safety = m_Map[current[0], current[1]].Safety + 1;
				m_Map[current[0], current[1]].Safety = (m_Map[current[0], current[1]].Safety - strength < 0 ? 0 : m_Map[current[0], current[1]].Safety - strength);
				m_Map[current[0], current[1]].SafetyNormalized = (double)m_Map[current[0], current[1]].Safety/m_MaximumSafetyLimit;

				foreach(int[] neighbor in GetNeighbors(current[0], current[1])) {
					if(!m_Map[neighbor[0], neighbor[1]].IsSolid && m_Map[neighbor[0], neighbor[1]].Safety > potential_safety) { // XXX: MAY NOT PASS FOR OVERLAYING SAFETIES, CHECK STRENGTH CODE
						m_Map[neighbor[0], neighbor[1]].Safety = potential_safety;

						cells.Enqueue(neighbor);
					}
				}
			}
		}

		protected void applyVisibility(int x, int y, float step = 1.0f)
		{
			for(float angle = 0.0f; 360.0f - angle > float.Epsilon; angle += step) {
				float delta_x = (float)Mathf.Cos(0.01745f*angle);
				float delta_y = (float)Mathf.Sin(0.01745f*angle);
				float position_x = 0.5f + x;
				float position_y = 0.5f + y;

				while(position_x > 0.0f && position_y > 0.0f && position_x < m_Width && position_y < m_Height && !m_Map[(int)position_x, (int)position_y].IsSolid) {
					position_x += delta_x;
					position_y += delta_y;

					m_Map[(int)position_x, (int)position_y].IsVisible = true;
				}
			}
		}

		protected int[] findSuitableExit()
		{
			int[][] exits;
			Queue<int[]> exit_set = new Queue<int[]>();

			while(exit_set.Count < 20) {
				Queue<int[]> new_exits = new Queue<int[]>();
				new_exits.Enqueue(new int[2] { m_RNG.Next(m_Width/2), m_RNG.Next(m_Height/2) });
				new_exits.Enqueue(new int[2] { m_Width/2 + m_RNG.Next(m_Width/2), m_RNG.Next(m_Height/2) });
				new_exits.Enqueue(new int[2] { m_RNG.Next(m_Width/2), m_Height/2 + m_RNG.Next(m_Height/2) });
				new_exits.Enqueue(new int[2] { m_Width/2 + m_RNG.Next(m_Width/2), m_Height/2 + m_RNG.Next(m_Height/2) });

				while(new_exits.Count > 0) {
					int[] exit = new_exits.Dequeue();

					if(!m_Map[exit[0], exit[1]].IsSolid && m_Map[exit[0], exit[1]].Type == Cell.TileSpawn.NONE && m_Map[exit[0], exit[1]].Safety > 0) {
						exit_set.Enqueue(exit);
					}
				}
			}

			exits = exit_set.ToArray();

			// Sort exit array by their safety value.
			for(int current = 0; current < exits.GetLength(0); ++current) {
				for(int target = current + 1; target < exits.GetLength(0); ++target) {
					if(m_Map[exits[target][0], exits[target][1]].Safety > m_Map[exits[current][0], exits[current][1]].Safety) {
						int[] temp = exits[current];
						exits[current] = exits[target];
						exits[target] = temp;
					}
				}
			}

			return exits[(int)((double)exits.Length*m_RNG.NextDouble()*m_RNG.NextDouble())];
		}
	}

	[Header("Reference Map")]
	public int Width = 200;
	public int Height = 275;
	[Range(0, 100)] public float MinerSpawnRate = 0.15f;
	public int MinerTimeoutLimit = 400;
	public int SmoothingPassCount = 2;
	public int MaximumSafetyLimit = 50;

	[Header("Random Number Generator")]
	public bool GenerateSeed = true;
	public string Seed;

	protected System.Random m_RNG;
	protected ReferenceMap m_ReferenceMap;

	public void Awake()
	{

	}

	public void Start()
	{
		GenerateMap();
	}

	public void Update()
	{
		if(Input.GetKeyDown("space")) {
			GenerateMap();
		} else if(Input.GetKeyDown("left ctrl") && m_ReferenceMap != null) {
			m_ReferenceMap.RenormalizeSafety(MaximumSafetyLimit);
		}
	}

	public void OnDrawGizmos()
	{
		if(m_ReferenceMap != null) {
			m_ReferenceMap.DrawGizmos(this.transform.position);
		}
	}

	public void GenerateMap()
	{
		this.initializeRandomNumberGenerator();

		m_ReferenceMap = new ReferenceMap(Seed, Width, Height, 0.01f*MinerSpawnRate, MinerTimeoutLimit, SmoothingPassCount, MaximumSafetyLimit);
	}

	protected void initializeRandomNumberGenerator()
	{
		if(GenerateSeed) {
			System.DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			double epoch_ms = 1000*(System.DateTime.UtcNow - epoch).TotalSeconds;
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
		// Initialize a clear map.
	}
}
