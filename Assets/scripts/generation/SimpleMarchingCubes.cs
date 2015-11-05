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

	// XXX: THIS IS A LOT OF MEMORY TO STORE IN LISTS, BEEEES CAREFUL! [PROFILE?]
	protected readonly static List<int>[] CUBE_VERTEX_LOOKUP = {
		new List<int>(),
		new List<int>() {16, 13, 12},
		new List<int>() {17, 12, 15},
		new List<int>() {16, 13, 15, 16, 15, 17},
		new List<int>() {19, 15, 14},
		new List<int>() {16, 13, 12, 19, 15, 14},
		new List<int>() {17, 12, 14, 17, 14, 19},
		new List<int>() {19, 16, 14, 14, 16, 13, 16, 19, 17}, // XXX: ????????
		new List<int>() {18, 14, 13},
		new List<int>() {16, 18, 14, 16, 14, 12},
		new List<int>() {18, 14, 13, 17, 12, 15},
		new List<int>() {16, 18, 17, 17, 14, 15, 17, 18, 14},
		new List<int>() {18, 19, 13, 13, 19, 15},
		new List<int>() {16, 18, 19, 16, 19, 15, 16, 15, 12}, // NOTE: I HATE THESE T_T
		new List<int>() {18, 19, 17, 13, 18, 17, 13, 17, 12},
		new List<int>() {16, 18, 17, 18, 19, 17}, // ezpz 15
		new List<int>() {8, 9, 16},
		new List<int>() {8, 9, 13, 8, 13, 12},
		new List<int>() {8, 9, 16, 17, 12, 15}, // NOTE: why am I not labeling these......... 18?
		new List<int>() {9, 13, 15, 9, 17, 8, 9, 15, 17},
		new List<int>() {8, 9, 16, 14, 19, 15},
		new List<int>() {14, 19, 15, 8, 9, 13, 8, 13, 12}, // cool
		new List<int>() {19, 17, 14, 14, 17, 12, 8, 9, 16},
		new List<int>() {9, 13, 14, 9, 14, 17, 17, 14, 19, 9, 17, 8}, // XXX: MAYBE PLEASE
		new List<int>() {8, 9, 16, 18, 14, 13},
		new List<int>() {8, 14, 12, 8, 18, 14, 8, 9, 18},
		new List<int>() {8, 9, 16, 18, 14, 13, 17, 12, 15},
		new List<int>() {8, 9, 17, 17, 9, 15, 15, 9, 18, 15, 18, 14}, // 27 CHECKED UP TO HERE, BUT FACE-DIRECTION MAY OR MAY NOT BE CORRECT
		new List<int>() {13, 18, 19, 13, 19, 15, 8, 9, 16},
		new List<int>() {12, 8, 15, 15, 8, 18, 15, 18, 19, 8, 9, 18}, // 29 plz send help @_@
		new List<int>() {17, 18, 19, 13, 18, 17, 13, 17, 12, 8, 9, 16},
		new List<int>() {18, 19, 17, 17, 8, 9, 17, 9, 18}, // <3 my first inverse case
		new List<int>() {11, 8, 17},
		new List<int>() {11, 8, 17, 16, 13, 12},
		new List<int>() {11, 8, 15, 8, 12, 15},
		new List<int>() {11, 13, 15, 11, 8, 13, 8, 16, 13},
		new List<int>() {11, 8, 17, 14, 19, 15},
		new List<int>() {11, 8, 17, 14, 19, 15, 16, 13, 12},
		new List<int>() {8, 12, 14, 11, 8, 14, 19, 11, 14},
		new List<int>() {8, 16, 11, 11, 16, 13, 11, 13, 19, 19, 13, 14},
		new List<int>() {11, 8, 17, 18, 14, 13},
		new List<int>() {16, 18, 14, 12, 16, 14, 11, 8, 17},
		new List<int>() {8, 12, 11, 11, 12, 15, 13, 18, 14},
		new List<int>() {8, 16, 11, 11, 16, 14, 11, 14, 15, 18, 14, 16},
		new List<int>() {13, 18, 19, 19, 15, 13, 11, 8, 17},
		new List<int>() {16, 18, 19, 12, 16, 19, 12, 19, 15, 11, 8, 17},
		new List<int>() {8, 12, 13, 8, 13, 19, 18, 19, 13, 11, 8, 19},
		new List<int>() {18, 19, 16, 11, 16, 19, 11, 8, 16},
		new List<int>() {11, 9, 16, 11, 16, 17},
		new List<int>() {11, 9, 13, 11, 13, 12, 11, 12, 17},
		new List<int>() {11, 9, 15, 9, 16, 12, 9, 12, 15},
		new List<int>() {11, 9, 13, 11, 13, 15},
		new List<int>() {16, 11, 9, 16, 17, 11, 14, 19, 15}, // 52
		new List<int>() {14, 19, 15, 11, 9, 13, 11, 13, 12, 11, 12, 17},
		new List<int>() {11, 9, 19, 9, 16, 12, 9, 12, 19, 12, 14, 19},
		new List<int>() {9, 13, 11, 11, 13, 14, 11, 14, 19},
		new List<int>() {9, 16, 11, 11, 16, 17, 13, 18, 14},
		new List<int>() {18, 14, 12, 18, 12, 11, 12, 17, 11, 18, 11, 9},
		new List<int>() {11, 9, 15, 9, 16, 12, 9, 12, 15, 13, 18, 14},
		new List<int>() {11, 9, 15, 9, 14, 15, 9, 18, 14},
		new List<int>() {9, 16, 11, 16, 17, 11, 13, 18, 19, 13, 19, 15},
		new List<int>() {12, 17, 15, 9, 18, 11, 11, 18, 19},
		new List<int>() {16, 12, 13, 9, 18, 11, 11, 18, 19},
		new List<int>() {9, 18, 11, 11, 18, 19},
		new List<int>() {10, 11, 19}, // 64
		new List<int>() {10, 11, 19, 16, 13, 12},
		new List<int>() {10, 11, 19, 17, 12, 15},
		new List<int>() {10, 11, 19, 16, 13, 17, 17, 13, 15},
		new List<int>() {14, 10, 11, 14, 11, 15},
		new List<int>() {14, 10, 11, 14, 11, 15, 16, 13, 12},
		new List<int>() {12, 14, 10, 12, 10, 17, 10, 11, 17},
		new List<int>() {14, 10, 13, 13, 10, 17, 13, 17, 16, 10, 11, 17},
		new List<int>() {10, 11, 19, 13, 18, 14},
		new List<int>() {11, 19, 10, 16, 18, 12, 12, 18, 14},
		new List<int>() {10, 11, 19, 13, 18, 14, 17, 12, 15},
		new List<int>() {10, 11, 19, 16, 18, 17, 17, 18, 14, 17, 14, 15},
		new List<int>() {15, 13, 11, 13, 18, 11, 11, 18, 10},
		new List<int>() {16, 18, 12, 12, 18, 11, 18, 10, 11, 12, 11, 15}, // EZ PZ LEMON SQUEEZY
		new List<int>() {10, 11, 17, 18, 10, 17, 12, 18, 17, 13, 18, 12},
		new List<int>() {16, 18, 17, 18, 10, 17, 10, 11, 17},
		new List<int>() {10, 11, 19, 16, 8, 9}, // 80
		new List<int>() {10, 11, 19, 8, 9, 13, 8, 13, 12},
		new List<int>() {10, 11, 19, 16, 8, 9, 17, 12, 15},
		new List<int>() {10, 11, 19, 9, 13, 15, 8, 9, 15, 8, 15, 17},
		new List<int>() {14, 10, 11, 14, 11, 15, 9, 16, 8},
		new List<int>() {14, 10, 11, 14, 11, 15, 8, 9, 13, 8, 13, 12},
		new List<int>() {12, 14, 10, 12, 10, 17, 10, 11, 17, 16, 8, 9},
		new List<int>() {9, 13, 10, 10, 13, 14, 8, 11, 17},
		new List<int>() {10, 11, 19, 8, 9, 16, 13, 18, 14},
		new List<int>() {10, 11, 19, 12, 8, 14, 8, 9, 18, 8, 18, 14},
		new List<int>() {10, 11, 19, 8, 9, 16, 13, 18, 14, 17, 12, 15},
		new List<int>() {19, 14, 15, 8, 11, 17, 10, 9, 18},
		new List<int>() {8, 9, 16, 11, 15, 13, 13, 18, 11, 18, 10, 11},
		new List<int>() {9, 18, 10, 12, 8, 11, 12, 11, 15},
		new List<int>() {8, 11, 17, 12, 13, 16, 9, 18, 10},
		new List<int>() {8, 11, 17, 9, 18, 10}, // 95
		new List<int>() {10, 8, 17, 19, 10, 17},
		new List<int>() {10, 8, 17, 19, 10, 17, 12, 16, 13},
		new List<int>() {10, 8, 12, 12, 15, 10, 10, 15, 19},
		new List<int>() {13, 15, 16, 16, 15, 10, 15, 19, 10, 16, 10, 8}, // 99!
		new List<int>() {14, 10, 8, 14, 8, 15, 15, 8, 17},
		new List<int>() {14, 10, 8, 14, 8, 15, 15, 8, 17, 16, 13, 12},
		new List<int>() {14, 10, 8, 14, 8, 12},
		new List<int>() {14, 10, 8, 8, 16, 13, 8, 13, 14},
		new List<int>() {10, 8, 17, 10, 17, 19, 13, 18, 14},
		new List<int>() {10, 8, 17, 10, 17, 19, 16, 18, 12, 12, 18, 14},
		new List<int>() {13, 18, 14, 10, 8, 12, 19, 10, 12, 19, 12, 15},
		new List<int>() {18, 8, 16, 18, 10, 8, 19, 14, 15},
		new List<int>() {18, 10, 8, 18, 8, 15, 18, 15, 13, 8, 17, 15},
		new List<int>() {16, 18, 8, 18, 10, 8, 12, 17, 15},
		new List<int>() {12, 10, 8, 13, 10, 12, 13, 18, 10},
		new List<int>() {8, 16, 18, 8, 18, 10},
		new List<int>() {16, 17, 19, 16, 19, 10, 16, 10, 9},
		new List<int>() {13, 12, 17, 10, 13, 17, 10, 17, 19, 9, 13, 10},
		new List<int>() {16, 10, 9, 10, 16, 12, 19, 10, 12, 15, 19, 12},
		new List<int>() {9, 13, 15, 9, 15, 19, 9, 19, 10},
		new List<int>() {17, 14, 9, 16, 17, 9, 9, 14, 10, 17, 15, 14},
		new List<int>() {13, 10, 9, 13, 14, 10, 12, 17, 15},
		new List<int>() {9, 16, 12, 9, 12, 10, 12, 14, 10},
		new List<int>() {9, 13, 10, 10, 13, 14},
		new List<int>() {9, 18, 10, 16, 17, 19, 16, 19, 13, 13, 19, 14},
		new List<int>() {9, 18, 10, 12, 17, 19, 19, 14, 12},
		new List<int>() {9, 18, 10, 16, 12, 13, 19, 14, 15},
		new List<int>() {9, 18, 10, 19, 14, 15},
		new List<int>() {9, 18, 10, 13, 16, 17, 13, 17, 15},
		new List<int>() {9, 18, 10, 12, 17, 15},
		new List<int>() {9, 18, 10, 16, 12, 13},
		new List<int>() {9, 18, 10},
		new List<int>() {9, 10, 18}, // 128
		new List<int>() {9, 10, 18, 16, 13, 12},
		new List<int>() {9, 10, 18, 12, 15, 17},
		new List<int>() {9, 10, 18, 17, 16, 13, 17, 13, 15},
		new List<int>() {9, 10, 18, 14, 19, 15},
		new List<int>() {9, 10, 18, 14, 19, 15, 16, 13, 12},
		new List<int>() {9, 10, 18, 12, 14, 19, 12, 19, 17},
		new List<int>() {9, 10, 18, 16, 19, 17, 16, 14, 19, 16, 13, 14},
		new List<int>() {13, 9, 10, 13, 10, 14},
		new List<int>() {12, 10, 14, 16, 10, 12, 16, 9, 10},
		new List<int>() {13, 9, 10, 13, 10, 14, 17, 12, 15},
		new List<int>() {17, 16, 15, 15, 16, 10, 16, 9, 10, 15, 10, 14},
		new List<int>() {13, 9, 15, 9, 19, 15, 9, 10, 19},
		new List<int>() {16, 9, 10, 16, 10, 12, 12, 10, 19, 12, 19, 15},
		new List<int>() {13, 9, 12, 12, 9, 19, 12, 19, 17, 9, 10, 19},
		new List<int>() {16, 19, 17, 16, 10, 19, 16, 9, 10},
		new List<int>() {16, 8, 10, 16, 10, 18},
		new List<int>() {12, 8, 10, 12, 10, 18, 12, 18, 13},
		new List<int>() {8, 10, 16, 16, 10, 18, 12, 15, 17},
		new List<int>() {8, 10, 17, 17, 10, 13, 17, 13, 15, 10, 18, 13},
		new List<int>() {15, 14, 19, 16, 8, 10, 16, 10, 18},
		new List<int>() {12, 8, 10, 12, 10, 18, 12, 18, 13, 15, 14, 19},
		new List<int>() {16, 8, 10, 16, 10, 18, 12, 14, 19, 12, 19, 17},
		new List<int>() {13, 14, 18, 17, 8, 10, 17, 10, 19},
		new List<int>() {8, 10, 14, 8, 14, 13, 16, 8, 13},
		new List<int>() {12, 8, 10, 12, 10, 14},
		new List<int>() {8, 10, 14, 8, 14, 13, 16, 8, 13, 12, 15, 17},
		new List<int>() {8, 10, 14, 17, 8, 14, 17, 14, 15},
		new List<int>() {15, 13, 19, 13, 8, 19, 13, 16, 8, 19, 8, 10},
		new List<int>() {12, 8, 10, 12, 10, 15, 15, 10, 19},
		new List<int>() {17, 8, 10, 17, 10, 19, 12, 13, 16}, // 148
		new List<int>() {17, 8, 10, 17, 10, 19},
		new List<int>() {9, 10, 18, 8, 17, 11},
		new List<int>() {9, 10, 18, 8, 17, 11, 16, 13, 12},
		new List<int>() {9, 10, 18, 12, 11, 8, 12, 15, 11},
		new List<int>() {9, 10, 18, 11, 13, 15, 13, 11, 16, 16, 11, 8},
		new List<int>() {9, 10, 18, 14, 19, 15, 8, 17, 11},
		new List<int>() {9, 10, 18, 14, 19, 15, 8, 17, 11, 12, 16, 13},
		new List<int>() {9, 10, 18, 8, 19, 11, 8, 14, 19, 12, 14, 8},
		new List<int>() {11, 10, 19, 16, 9, 8, 13, 14, 18},
		new List<int>() {13, 9, 10, 13, 10, 14, 8, 17, 11},
		new List<int>() {12, 10, 14, 12, 16, 10, 16, 9, 10, 8, 17, 11},
		new List<int>() {14, 13, 9, 14, 9, 10, 12, 15, 8, 15, 11, 8},
		new List<int>() {16, 9, 8, 15, 11, 10, 15, 10, 14},
		new List<int>() {8, 17, 11, 13, 9, 15, 9, 10, 19, 15, 9, 19},
		new List<int>() {16, 9, 8, 11, 10, 19, 12, 17, 15},
		new List<int>() {12, 9, 8, 12, 13, 9, 11, 10, 19},
		new List<int>() {16, 9, 8, 11, 10, 19},
		new List<int>() {16, 17, 18, 17, 10, 18, 17, 11, 10},
		new List<int>() {17, 11, 10, 17, 10, 18, 12, 17, 18, 12, 18, 13},
		new List<int>() {18, 16, 10, 10, 16, 15, 16, 12, 15, 10, 15, 11},
		new List<int>() {15, 11, 13, 13, 11, 10, 13, 10, 18},
		new List<int>() {15, 14, 19, 16, 17, 18, 17, 10, 18, 17, 11, 10},
		new List<int>() {13, 14, 18, 12, 17, 15, 11, 10, 19},
		new List<int>() {16, 12, 18, 18, 12, 14, 11, 10, 19},
		new List<int>() {11, 10, 19, 13, 14, 18},
		new List<int>() {14, 11, 10, 11, 14, 16, 13, 16, 14, 16, 17, 11},
		new List<int>() {12, 10, 14, 17, 10, 12, 17, 11, 10},
		new List<int>() {12, 13, 16, 15, 11, 10, 15, 10, 14},
		new List<int>() {15, 11, 10, 15, 10, 14},
		new List<int>() {11, 10, 19, 13, 16, 17, 13, 17, 15},
		new List<int>() {11, 10, 19, 12, 17, 15},
		new List<int>() {11, 10, 19, 16, 12, 13}, // 190
		new List<int>() {11, 10, 19},
		new List<int>() {18, 9, 11, 18, 11, 19},
		new List<int>() {18, 9, 11, 18, 11, 19, 16, 13, 12},
		new List<int>() {18, 9, 11, 18, 11, 19, 12, 15, 17},
		new List<int>() {18, 9, 11, 18, 11, 19, 16, 13, 17, 13, 15, 17},
		new List<int>() {15, 9, 11, 14, 18, 9, 14, 9, 15},
		new List<int>() {15, 9, 11, 14, 18, 9, 14, 9, 15, 16, 13, 12},
		new List<int>() {9, 11, 17, 9, 17, 14, 14, 18, 9, 12, 14, 17},
		new List<int>() {13, 14, 18, 16, 9, 11, 16, 11, 17},
		new List<int>() {13, 9, 11, 13, 11, 14, 14, 11, 19},
		new List<int>() {12, 19, 14, 12, 9, 19, 12, 16, 9, 19, 9, 11},
		new List<int>() {13, 9, 11, 13, 11, 14, 14, 11, 19, 12, 15, 17},
		new List<int>() {16, 9, 11, 16, 11, 17, 19, 14, 15},
		new List<int>() {13, 9, 11, 13, 11, 15},
		new List<int>() {15, 9, 11, 16, 9, 15, 16, 15, 12},
		new List<int>() {13, 9, 11, 13, 11, 17, 12, 13, 17},
		new List<int>() {16, 9, 11, 16, 11, 17},
		new List<int>() {16, 19, 18, 16, 11, 19, 16, 8, 11},
		new List<int>() {19, 18, 11, 11, 18, 12, 11, 12, 8, 18, 13, 12},
		new List<int>() {16, 19, 18, 16, 11, 19, 16, 8, 11, 12, 15, 17},
		new List<int>() {15, 19, 13, 13, 19, 18, 8, 11, 17},
		new List<int>() {11, 15, 8, 8, 15, 18, 8, 18, 16, 15, 14, 18},
		new List<int>() {12, 8, 11, 12, 11, 15, 14, 18, 13},
		new List<int>() {8, 11, 17, 12, 18, 16, 12, 14, 18},
		new List<int>() {8, 11, 17, 13, 14, 18},
		new List<int>() {8, 11, 16, 16, 11, 13, 13, 11, 19, 13, 19, 14},
		new List<int>() {12, 8, 14, 14, 8, 11, 14, 11, 19}, // 217
		new List<int>() {8, 11, 17, 12, 13, 16, 15, 19, 14},
		new List<int>() {8, 11, 17, 15, 19, 14},
		new List<int>() {13, 11, 15, 13, 8, 11, 13, 16, 8},
		new List<int>() {12, 8, 11, 12, 11, 15},
		new List<int>() {8, 11, 17, 12, 13, 16},
		new List<int>() {8, 11, 17},
		new List<int>() {17, 19, 18, 17, 18, 8, 8, 18, 9},
		new List<int>() {17, 19, 18, 17, 18, 8, 8, 18, 9, 12, 16, 13},
		new List<int>() {9, 8, 12, 9, 12, 19, 9, 19, 18, 12, 15, 19},
		new List<int>() {19, 18, 13, 19, 13, 15, 16, 9, 8},
		new List<int>() {18, 9, 8, 14, 17, 15, 17, 18, 8, 17, 14, 18}, // 228
		new List<int>() {12, 17, 15, 16, 9, 8, 13, 14, 18},
		new List<int>() {12, 14, 8, 8, 14, 18, 8, 18, 9},
		new List<int>() {16, 9, 8, 13, 14, 18},
		new List<int>() {13, 9, 14, 14, 9, 17, 9, 8, 17, 14, 17, 19},
		new List<int>() {16, 9, 8, 12, 17, 19, 12, 19, 14},
		new List<int>() {13, 9, 12, 12, 9, 8, 15, 19, 14},
		new List<int>() {16, 9, 8, 15, 19, 14},
		new List<int>() {13, 9, 15, 15, 9, 8, 15, 8, 17},
		new List<int>() {16, 9, 8, 12, 17, 15},
		new List<int>() {13, 9, 8, 12, 13, 8},
		new List<int>() {16, 9, 8},
		new List<int>() {16, 17, 18, 17, 19, 18},
		new List<int>() {17, 19, 18, 13, 17, 18, 12, 17, 13},
		new List<int>() {16, 19, 18, 16, 12, 19, 12, 15, 19},
		new List<int>() {13, 19, 18, 13, 15, 19},
		new List<int>() {16, 17, 18, 17, 14, 18, 17, 15, 14},
		new List<int>() {12, 17, 15, 13, 14, 18},
		new List<int>() {12, 14, 18, 12, 18, 16},
		new List<int>() {13, 14, 18},
		new List<int>() {16, 17, 19, 16, 19, 13, 13, 19, 14},
		new List<int>() {12, 17, 19, 12, 19, 14},
		new List<int>() {12, 13, 16, 15, 19, 14},
		new List<int>() {15, 19, 14},
		new List<int>() {13, 16, 17, 13, 17, 15},
		new List<int>() {12, 17, 15},
		new List<int>() {12, 13, 16}
	};

	[Header("DEBUG CONTROLS")]
	public bool DrawGizmos = true;
	public bool DrawWithinRange = false;
	public int lower_x, upper_x;
	public int lower_y, upper_y;
	public int lower_z, upper_z;

	protected Cube[,,] m_CellGrid;
	protected List<int> m_Triangles;
	protected List<Vector3> m_Vertices;

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
						if(!DrawWithinRange || (x >= lower_x && x <= upper_x && y >= lower_y && y <= upper_y && z >= lower_z && z <= upper_z)) {
							for(int index = 0; index < 20; ++index) {
								m_CellGrid[x, y, z].IndexToNode(index).DrawGizmos(this.transform.position);
							}
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

		meshCellGrid();
	}

	public void Remesh()
	{
		for(int x = 0; x < m_CellGrid.GetLength(0); ++x) {
			for(int y = 0; y < m_CellGrid.GetLength(1); ++y) {
				for(int z = 0; z < m_CellGrid.GetLength(2); ++z) {
					for(int index = 0; index < 20; ++index) {
						m_CellGrid[x, y, z].IndexToNode(index).VertexIndex = -1;
					}
				}
			}
		}

		meshCellGrid();
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

	protected void meshCellGrid()
	{
		int index = 0;
		Vector2[] uvs;
		Mesh cave_mesh = new Mesh();
		MeshFilter mesh_filter = GetComponent<MeshFilter>();
		MeshCollider mesh_collider = GetComponent<MeshCollider>();
		m_Vertices.Clear();
		m_Triangles.Clear();

		// Generate vertices and triangles
		for(int x = 0; x < m_CellGrid.GetLength(0); ++x) {
			for(int y = 0; y < m_CellGrid.GetLength(1); ++y) {
				for(int z = 0; z < m_CellGrid.GetLength(2); ++z) {
					triangulateCellToLists(m_CellGrid[x, y, z]);
				}
			}
		}

		// Build prelim mesh
		cave_mesh.vertices = m_Vertices.ToArray();
		cave_mesh.triangles = m_Triangles.ToArray();
		cave_mesh.RecalculateNormals();

		// Use new mesh to build simple uvs.
		uvs = new Vector2[m_Vertices.Count];
		foreach(Vector3 vertex in m_Vertices) {
			Vector3 normal = cave_mesh.normals[index];
			uvs[index] = new Vector2(vertex.x, vertex.z);

			++index;
		}

		cave_mesh.uv = uvs;

		if(mesh_filter) {
			mesh_filter.mesh = cave_mesh;
		}

		if(mesh_collider) {
			mesh_collider.sharedMesh = cave_mesh;
		}
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
