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

			public bool IsSolid;
			public bool IsVisible;
			public Cell.TileSpawn Type;
			public int Safety;
			public double SafetyNormalized;

			public Cell()
			{
				this.IsSolid = true;
				this.IsVisible = false;
				this.Type = Cell.TileSpawn.NONE;
				this.Safety = System.Int32.MaxValue;
				this.SafetyNormalized = 1.0;
			}

			public Cell(Cell source)
			{
				this.IsSolid = source.IsSolid;
				this.IsVisible = source.IsVisible;
				this.Type = source.Type;
				this.Safety = source.Safety;
				this.SafetyNormalized = source.SafetyNormalized;
			}
		}

		public int Width { get { return m_Width; } }
		public int Height { get { return m_Height; } }

		protected int m_Width;
		protected int m_Height;
		protected float m_MinerSpawnRate;
		protected int m_MinerTimeoutLimit;
		protected int m_SmoothingPassCount;
		protected int m_MaximumSafetyLimit;
		protected int m_EnemyPopulation;
		protected float m_EnemySpawnModifier;
		protected double m_ItemSpawnRequiredSafety;
		protected int m_ItemSpawnEnemySearchRadius;
		protected float m_Profitability;
		protected float m_Materialability;

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

				if(new_x >= 1 && new_x < map.GetLength(0) - 1 && new_y >= 1 && new_y < map.GetLength(1) - 1) {
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

		public ReferenceMap(string seed, int width, int height, float miner_spawn_rate, int miner_timeout_limit, int smoothing_pass_count, int maximum_safety_limit, int enemy_population, float enemy_spawn_modifier, double item_spawn_required_safety, int item_spawn_enemy_search_radius, float profitability, float materialability)
		{
			m_Width = width;
			m_Height = height;
			m_MinerSpawnRate = miner_spawn_rate;
			m_MinerTimeoutLimit = miner_timeout_limit;
			m_SmoothingPassCount = smoothing_pass_count;
			m_MaximumSafetyLimit = maximum_safety_limit;
			m_EnemyPopulation = enemy_population;
			m_EnemySpawnModifier = enemy_spawn_modifier;
			m_ItemSpawnRequiredSafety = item_spawn_required_safety;
			m_ItemSpawnEnemySearchRadius = item_spawn_enemy_search_radius;
			m_Profitability = profitability;
			m_Materialability = materialability;


			m_RNG = new System.Random(seed.GetHashCode());

			this.initializeMap();
			this.generateMap();
		}

		public Cell GetCell(int x, int y)
		{
			return new Cell(m_Map[x, y]);
		}

		public int[] FindBorders()
		{
			int[] borders = new int[4] { m_Width, m_Height, -1, -1 };

			for(int x = 0; x < m_Width; ++x) {
				for(int y = 0; y < m_Height; ++y) {
					if(!m_Map[x, y].IsSolid) {
						if(x - 1 < borders[0]) {
							borders[0] = x - 1;
						}

						if(x + 1 > borders[2]) {
							borders[2] = x + 1;
						}

						if(y - 1 < borders[1]) {
							borders[1] = y - 1;
						}

						if(y + 1 > borders[3]) {
							borders[3] = y + 1;
						}
					}
				}
			}

			return borders;
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

		public void DrawGizmos(Vector3 anchor, bool draw_visibility, bool draw_safety, bool show_map_borders)
		{
			if(m_Map != null) {
				int[] borders = FindBorders();

				for(int x = 0; x < m_Width; ++x) {
					for(int y = 0; y < m_Height; ++y) {
						Gizmos.color = Color.magenta;
						Vector3 position = new Vector3(-m_Width/2.0f + 0.5f + x, 0.0f, -m_Height/2.0f + 0.5f + y);

						//ONE, START, EXIT, ENEMY, ITEM, GOLD, SUPPLIES
						switch(m_Map[x, y].Type) {
							case Cell.TileSpawn.NONE:
								if(m_Map[x, y].IsSolid) {
									if(show_map_borders && (x == borders[0] || x == borders[2] || y == borders[1] || y == borders[3])) {
										Gizmos.color = Color.grey;
									} else {
										Gizmos.color = Color.black;
									}
								} else if(draw_visibility && m_Map[x, y].IsVisible) {
									Gizmos.color = Color.yellow;
								} else if(draw_safety) {
									Gizmos.color = new Color((float)m_Map[x, y].SafetyNormalized, 0.0f, 1.0f - (float)m_Map[x, y].SafetyNormalized);
								} else {
									Gizmos.color = Color.white;
								}
								break;

							case Cell.TileSpawn.START:
								Gizmos.color = Color.green;
								break;

							case Cell.TileSpawn.EXIT:
								Gizmos.color = Color.red;
								break;

							case Cell.TileSpawn.ENEMY:
								Gizmos.color = new Color(1.0f, 0.675f, 0.15f, 1.0f);
								break;

							case Cell.TileSpawn.ITEM:
								Gizmos.color = new Color(0.0f, (float)(Mathf.Sin(10*Time.time) + 1.0)/2.0f, (float)(Mathf.Cos(10*Time.time) + 1.0)/2.0f);
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

			// Create dungeon geometry
			digCaverns(entrance[0], entrance[1]);
			smoothCaverns();

			// Place keypoints
			applySafetyZone(entrance[0], entrance[1], 3);
			applyVisibility(entrance[0], entrance[1], 0.25f);

			exit = findSuitableExit();
			m_Map[exit[0], exit[1]].Type = Cell.TileSpawn.EXIT;
			applySafetyZone(exit[0], exit[1], 2);

			// Populate
			placeEnemyTiles();
			placeDungeonItem();
			placeGoldAndMaterialDrops();
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

		protected void placeEnemyTiles()
		{
			List<int[]> spawn_list = new List<int[]>();

			for(int x = 1; x < m_Width - 1; ++x) {
				for(int y = 1; y < m_Height - 1; ++y) {
					if(!m_Map[x, y].IsSolid && !m_Map[x, y].IsVisible && m_Map[x, y].Type == Cell.TileSpawn.NONE && m_RNG.NextDouble() <= m_EnemySpawnModifier*m_Map[x, y].SafetyNormalized) {
						spawn_list.Add(new int[2] {x, y});
						m_Map[x, y].Type = Cell.TileSpawn.ENEMY;
					}
				}
			}

			while(spawn_list.Count > m_EnemyPopulation) {
				int[] spawn = spawn_list[0];
				spawn_list.RemoveAt(0);

				if(m_RNG.NextDouble() > 0.3*m_Map[spawn[0], spawn[1]].SafetyNormalized) {
					m_Map[spawn[0], spawn[1]].Type = Cell.TileSpawn.NONE;
				} else {
					spawn_list.Add(spawn);
				}
			}
		}

		protected int[] placeDungeonItem()
		{
			int enemy_spawn_cap = 10;
			int[] spawn;
			List<int> enemy_spawns_count = new List<int>();
			List<int[]> enemy_spawns = new List<int[]>();
			List<int[]> safe_spawns = new List<int[]>();

			// Create an index of spawns.
			for(int x = 1; x < m_Width - 1; ++x) {
				for(int y = 1; y < m_Height - 1; ++y) {
					if(!m_Map[x, y].IsSolid && m_Map[x, y].Type == Cell.TileSpawn.NONE && m_Map[x, y].SafetyNormalized >= m_ItemSpawnRequiredSafety) {
						int depth = 0;
						int enemy_count = 0;
						Queue<int[]> cells = new Queue<int[]>();
						Dictionary<int, bool> visited = new Dictionary<int, bool>();

						// Breadth-First Expansion for Enemy Checking
						cells.Enqueue(new int[2] {x, y});
						visited[x*m_Width + y] = true;

						while(cells.Count > 0 && depth++ < m_ItemSpawnEnemySearchRadius) {
							int[] cell = cells.Dequeue();

							foreach(int[] neighbor in GetNeighbors(cell[0], cell[1])) {
								if(!m_Map[neighbor[0], neighbor[1]].IsSolid) {
									if(!visited.ContainsKey(m_Width*neighbor[0] + neighbor[1]) || !visited[m_Width*neighbor[0] + neighbor[1]]) {
										cells.Enqueue(neighbor);
									}

									if(m_Map[neighbor[0], neighbor[1]].Type == Cell.TileSpawn.ENEMY) {
										++enemy_count;
									}
								}
							}
						}

						// Add to appropriate places in lists
						safe_spawns.Add(new int[2] {x, y});

						if(enemy_count > 0) {
							int index = 0;

							while(index < enemy_spawns_count.Count && enemy_spawns_count[index] > enemy_count) {
								++index;
							}

							if(enemy_spawns.Count < enemy_spawn_cap) {
								enemy_spawns.Insert(index, new int[2] {x, y});
								enemy_spawns_count.Insert(index, enemy_count);
							} else if(index < enemy_spawns_count.Count) {
								enemy_spawns[index] = new int[2] {x, y};
								enemy_spawns_count[index] = enemy_count;
							}
						}
					}
				}
			}

			// Pick a list based on whether enemy tiles were found or not.
			if(enemy_spawns.Count > 0) {
				spawn = enemy_spawns[0];
			} else {
				spawn = safe_spawns[m_RNG.Next(safe_spawns.Count)];
			}

			m_Map[spawn[0], spawn[1]].Type = Cell.TileSpawn.ITEM;

			return spawn;
		}

		protected void placeGoldAndMaterialDrops()
		{
			List<int[]> possible_spawns = new List<int[]>();

			for(int x = 1; x < m_Width - 1; ++x) {
				for(int y = 1; y < m_Height - 1; ++y) {
					if(!m_Map[x, y].IsSolid && m_Map[x, y].Type == Cell.TileSpawn.NONE && CountSolidAdjacents(x, y) >= 3) {
						bool gold = (float)m_RNG.NextDouble() <= m_Profitability;
						bool supply = (float)m_RNG.NextDouble() <= m_Materialability;

						if(gold && supply) {
							if((float)m_RNG.NextDouble() <= m_Materialability/(m_Materialability + m_Profitability)) {
								gold = false;
							} else {
								supply = false;
							}
						}

						if(gold) {
							m_Map[x, y].Type = Cell.TileSpawn.GOLD;
						} else if(supply) {
							m_Map[x, y].Type = Cell.TileSpawn.SUPPLIES;
						}
					}
				}
			}
		}
	}

	[Header("Wall Map")]
	[Range(1, 32)] public int RoomHeight = 8;
	[Range(1, 16)] public int MapCellSubdivision = 1;
	public float CellSize = 1.0f;
	[Range(1, 100)] public int CellChunkSize = 10;
	public int FuzzPasses = 3;
	[Range(0, 100)] public float ChanceOfFuzzing = 20.0f;
	public Material DefaultMaterial;

	[Header("Reference Map")]
	public int Width = 200;
	public int Height = 275;
	[Range(0, 100)] public float MinerSpawnRate = 0.15f;
	public int MinerTimeoutLimit = 400;
	public int SmoothingPassCount = 2;
	public int MaximumSafetyLimit = 50;
	public int EnemyPopulation = 15;
	public int EnemyPopulationVariance = 5;
	[Range(0, 100)] public float EnemySpawnModifier = 60.0f;
	[Range(0, 1)] public double ItemSpawnRequiredSafety = 0.8;
	public int ItemSpawnEnemySearchRadius = 5;
	[Range(0, 100)] public float Profitability = 2.5f;
	[Range(0, 100)] public float Materialability = 8.0f;

	[Header("Random Number Generator")]
	public bool GenerateSeed = true;
	public string Seed;

	[Header("Debug Controls")]
	public bool GenerateMesh = true;

	[Header("Gizmo Controls")]
	public bool DrawGizmos = false;
	public int HighlightedLayer = 0;
	public bool OnlyDrawHighlightedLayer = false;
	public bool CapDrawAtHighlightedLayer = false;
	public bool DrawReference = true;
	public bool ReferenceDrawVisibility = false;
	public bool ReferenceDrawSafety = false;
	public bool ReferenceDrawBorders = false;
	public bool DrawWallMap = false;

	protected System.Random m_RNG;
	protected ReferenceMap m_ReferenceMap;
	protected bool[,,] m_WallMap;
	protected List<GameObject> m_MapChunks;

	protected SimpleMarchingCubes m_MarchingCubes;

	public void Awake()
	{
		m_MarchingCubes = GetComponent<SimpleMarchingCubes>();
		m_MapChunks = new List<GameObject>();
	}

	public void Start()
	{
		GenerateMap();
	}

	public void Update()
	{
		//*
		if(Input.GetKeyDown("space")) {
			GenerateMap();
		} else if(Input.GetKeyDown("left ctrl") && m_ReferenceMap != null) {
			m_ReferenceMap.RenormalizeSafety(MaximumSafetyLimit);
		}
		//*/
	}

	public void OnDrawGizmos()
	{
		if(DrawGizmos) {
			if(DrawReference && m_ReferenceMap != null) {
				m_ReferenceMap.DrawGizmos(this.transform.position, ReferenceDrawVisibility, ReferenceDrawSafety, ReferenceDrawBorders);
			}

			if(DrawWallMap && m_WallMap != null) {
				Gizmos.color = Color.black;
				for(int x = 0; x < m_WallMap.GetLength(0); ++x) {
					for(int y = 0; y < m_WallMap.GetLength(1); ++y) {
						if(OnlyDrawHighlightedLayer && y < HighlightedLayer) {
							continue;
						} else if((OnlyDrawHighlightedLayer || CapDrawAtHighlightedLayer) && y > HighlightedLayer) {
							break;
						}

						for(int z = 0; z < m_WallMap.GetLength(2); ++z) {
							if(m_WallMap[x, y, z]) {
								if(!OnlyDrawHighlightedLayer) {
									Gizmos.color = new Color(0.0f, 0.2f + 0.4f*y/m_WallMap.GetLength(1), 0.0f, 1.0f);
								}
								Vector3 cube_position = new Vector3(-m_WallMap.GetLength(0)/2.0f + 0.5f + x, -m_WallMap.GetLength(1)/2.0f + 0.5f + y, -m_WallMap.GetLength(2)/2.0f + 0.5f + z);
								Gizmos.DrawCube(this.transform.position + cube_position, Vector3.one);
							}
						}
					}
				}
			}
		}
	}

	public void GenerateMap()
	{
		if(m_MapChunks.Count > 0) {
			foreach(GameObject chunk in m_MapChunks) {
				Destroy(chunk);
			}

			m_MapChunks.Clear();
		}

		this.initializeRandomNumberGenerator();

		m_ReferenceMap = new ReferenceMap(Seed, Width, Height, 0.01f*MinerSpawnRate, MinerTimeoutLimit, SmoothingPassCount, MaximumSafetyLimit, EnemyPopulation + m_RNG.Next(EnemyPopulationVariance), 0.01f*EnemySpawnModifier, ItemSpawnRequiredSafety, ItemSpawnEnemySearchRadius, 0.01f*Profitability, 0.01f*Materialability);

		this.buildWallMap();
		this.smoothWallMap();
		this.fuzzWallMap();

		if(GenerateMesh && m_MarchingCubes != null && m_WallMap != null) {
			int chunk_count_width = (int)System.Math.Ceiling((double)m_WallMap.GetLength(0)/CellChunkSize);
			int chunk_count_height = (int)System.Math.Ceiling((double)m_WallMap.GetLength(1)/CellChunkSize);
			int chunk_count_depth = (int)System.Math.Ceiling((double)m_WallMap.GetLength(2)/CellChunkSize);

			//*
			chunk_count_width += (int)System.Math.Ceiling((double)chunk_count_width/CellChunkSize);
			chunk_count_height += (int)System.Math.Ceiling((double)chunk_count_width/CellChunkSize);
			chunk_count_depth += (int)System.Math.Ceiling((double)chunk_count_depth/CellChunkSize);
			//*

			if(CellChunkSize > 1) {
				for(int chunk_x = 0; chunk_x < chunk_count_width; ++chunk_x) {
					for(int chunk_y = 0; chunk_y < chunk_count_height; ++chunk_y) {
						for(int chunk_z = 0; chunk_z < chunk_count_depth; ++chunk_z) {
							bool occupied_tile = false;
							bool[,,] partial_wallmap = new bool[CellChunkSize, CellChunkSize, CellChunkSize];
							GameObject map_chunk;

							// Create partial chunk and generate a small mesh from it.
							for(int x = 0; x < CellChunkSize; ++x) {
								for(int y = 0; y < CellChunkSize; ++y) {
									for(int z = 0; z < CellChunkSize; ++z) {
										int real_x = CellChunkSize*chunk_x - chunk_x + x;
										int real_y = CellChunkSize*chunk_y - chunk_y + y;
										int real_z = CellChunkSize*chunk_z - chunk_z + z;

										if(real_x < m_WallMap.GetLength(0) && real_y < m_WallMap.GetLength(1) && real_z < m_WallMap.GetLength(2)) {
											if(!m_WallMap[real_x, real_y, real_z]) {
												occupied_tile = true;
											}

											partial_wallmap[x, y, z] = m_WallMap[real_x, real_y, real_z];
										} else {
											partial_wallmap[x, y, z] = true;
										}
									}
								}
							}

							if(occupied_tile) {
								map_chunk = new GameObject("chunk_" + chunk_x + "." + chunk_y + "." + chunk_z);

								map_chunk.AddComponent<MeshFilter>();
								map_chunk.AddComponent<MeshRenderer>();
								map_chunk.GetComponent<MeshRenderer>().material = DefaultMaterial;
								map_chunk.AddComponent<MeshCollider>();
								map_chunk.AddComponent<SimpleMarchingCubes>();
								map_chunk.GetComponent<SimpleMarchingCubes>().CellSize = this.CellSize;
								map_chunk.GetComponent<SimpleMarchingCubes>().DrawGizmos = false;

								map_chunk.transform.position = new Vector3(CellSize*(CellChunkSize - 1)*(-chunk_count_width/2 + chunk_x), CellSize*(CellChunkSize - 1)*(-chunk_count_height/2 + chunk_y), CellSize*(CellChunkSize - 1)*(-chunk_count_depth/2 + chunk_z));
								map_chunk.transform.SetParent(this.transform);
								m_MapChunks.Add(map_chunk);

								map_chunk.GetComponent<SimpleMarchingCubes>().GenerateMesh(partial_wallmap);
							}
						}
					}
				}
			} else {
				m_MarchingCubes.GenerateMesh(m_WallMap);
			}
		}
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

	protected void buildWallMap()
	{
		int width, height, depth;
		bool[,,] wall_map;
		int[] borders = m_ReferenceMap.FindBorders();

		width = MapCellSubdivision*(borders[2] - borders[0] + 1);
		height = MapCellSubdivision*RoomHeight;
		depth = MapCellSubdivision*(borders[3] - borders[1] + 1);
		wall_map = new bool[width, height, depth];

		for(int x = 0; x < width; ++x) {
			for(int z = 0; z < depth; ++z) {
				bool cell_value = this.m_ReferenceMap.GetCell(borders[0] + (int)(x/MapCellSubdivision), borders[1] + (int)(z/MapCellSubdivision)).IsSolid;

				for(int y = 0; y < height; ++y) {
					if(y > 0 && y < height - MapCellSubdivision) {
						wall_map[x, y, z] = cell_value;
					} else {
						wall_map[x, y, z] = true;
					}
				}
			}
		}

		m_WallMap = wall_map;
	}

	protected void fuzzWallMap()
	{
		for(int pass = 0; pass < FuzzPasses; ++pass) {
			for(int x = 1; x < m_WallMap.GetLength(0) - 1; ++x) {
				for(int y = 1; y < m_WallMap.GetLength(1) - 1; ++y) {
					for(int z = 1; z < m_WallMap.GetLength(2) - 1; ++z) {
						int neighbor_count = countWallMapNeighbors(x, y, z);

						if(m_WallMap[x, y, z] && neighbor_count < 25 && (float)m_RNG.NextDouble() <= 0.01f*ChanceOfFuzzing) {
							m_WallMap[x, y, z] = false;
						}
					}
				}
			}
		}
	}

	protected void smoothWallMap()
	{
		for(int pass = 0; pass < 3; ++pass) {
			for(int x = 0; x < m_WallMap.GetLength(0); ++x) {
				for(int y = 0; y < m_WallMap.GetLength(1); ++y) {
					for(int z = 0; z < m_WallMap.GetLength(2); ++z) {
						int neighbor_count = countWallMapNeighbors(x, y, z);

						if(neighbor_count > 18) {
							m_WallMap[x, y, z] = true;
						} else if(neighbor_count < 8) {
							m_WallMap[x, y, z] = false;
						}
					}
				}
			}
		}
	}

	protected int countWallMapNeighbors(int x, int y, int z)
	{
		int neighbor_count = 0;

		for(int adj_x = x - 1; adj_x <= x + 1; ++adj_x) {
			for(int adj_y = y - 1; adj_y <= y + 1; ++adj_y) {
				for(int adj_z = z - 1; adj_z <= z + 1; ++adj_z) {
					if((adj_x != x || adj_y != y || adj_z != z) && (adj_x < 1 || adj_y < 1 || adj_z < 1 || adj_x >= m_WallMap.GetLength(0) - 1 || adj_y >= m_WallMap.GetLength(1) - 1 || adj_z >= m_WallMap.GetLength(2) - 1 || m_WallMap[adj_x, adj_y, adj_z])) {
						++neighbor_count;
					}
				}
			}
		}

		return neighbor_count;
	}
}
