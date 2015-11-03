using UnityEngine;
using System.Collections;

public class MarchingCubeTester : MonoBehaviour
{
	public int x, y, z;
	public bool DownBackLeft, DownBackRight, DownForeLeft, DownForeRight, UpBackLeft, UpBackRight, UpForeLeft, UpForeRight;
	public int Configuration;
	public string BinaryConfiguration;

	protected SimpleMarchingCubes m_SimpleMarchingCubes;

	public void Awake()
	{
		m_SimpleMarchingCubes = GetComponent<SimpleMarchingCubes>();
	}

	public void Update()
	{
		if(m_SimpleMarchingCubes) {
			SimpleMarchingCubes.Cube selected_cube = m_SimpleMarchingCubes.GetCell(x, y, z);

			if(selected_cube != null) {
				selected_cube.DownBackLeft.IsActive = DownBackLeft;
				selected_cube.DownBackRight.IsActive = DownBackRight;
				selected_cube.DownForeLeft.IsActive = DownForeLeft;
				selected_cube.DownForeRight.IsActive = DownForeRight;
				selected_cube.UpBackLeft.IsActive = UpBackLeft;
				selected_cube.UpBackRight.IsActive = UpBackRight;
				selected_cube.UpForeLeft.IsActive = UpForeLeft;
				selected_cube.UpForeRight.IsActive = UpForeRight;

				selected_cube.CalculateConfiguration();
				Configuration = selected_cube.Configuration;
				BinaryConfiguration = System.Convert.ToString(Configuration, 2).PadLeft(8, '0');
			}
		}
	}
}
