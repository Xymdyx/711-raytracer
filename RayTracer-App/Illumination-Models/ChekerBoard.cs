/*
 author: stf8464
Desc: checkerboard pattern model for floor triangles
due date: 3/17/22
*/
using System;
using System.Collections.Generic;
using RayTracer_App.World;
using RayTracer_App.Scene_Objects;
namespace RayTracer_App.Illumination_Models
{
	public class CheckerBoardPattern
	{
		private Color _color1;
		private Color _color2;
		private int _rows;
		private int _cols;

		public Color color1 { get => this._color1; set => this._color1 = value; }
		public Color color2 { get => this._color2; set => this._color2 = value; }
		public int rows { get => this._rows; set => this._rows = value; }
		public int cols { get => this._cols; set => this._cols = value; }


		public CheckerBoardPattern( int rows = 5, int cols = 5)
		{
			this._color1 = new Color( 0.950f, 0.936f, 0.114f ); //yellow
			this._color2 = Color.floorColor;
			this._rows = rows;
			this._cols = cols;
		}

		public CheckerBoardPattern( Color color1, Color color2, int rows = 5, int cols = 5)
		{
			this._color1 = color1;
			this._color2 = color2;
			this._rows = rows;
			this._cols = cols;
		}


		//precondiiton: the negative of the cameraRay gets passed so it is going TO the viewer's eye, not from
		//this returns an irradiance triplet, which will be used by the Phong model
		public Color illuminate( Point intersect, Vector cameraRay, List<LightSource> lights, List<SceneObject> allObjs, Polygon litObj ) //add list of lights, addObject list both from world, remove incoming, mirrorReflect
		{
			/* implement checkerboard procedural texture */
			/* origin of the floor is: -6f, floorHeight, 60.5f  
			 * x : 76.5f
			 * z: 58.5f 
			 * need u and v from triangle.intersect()
			 * The interpolation for a given set of barycentric
				coordinates (u, v, w) is given by:
				T = uT0+ vT1+ wT2
			 * transform algo: find row and col where intersect occurs, if row and col's parity match, it's red. else, yellow */
			Point floorOrigin = Point.floorOrigin;
			float u = litObj.u;
			float v = litObj.v;
			float floorX = 76.5f;
			float floorZ = 76.5f;
			float checkW = (float) (floorX / rows);
			float checkH = (float) (floorZ / cols);

			float w = 1 - (u  + v);

			Vector textVec = (litObj.vertices[0] * u).toVec() + (litObj.vertices[1] * v).toVec() + (litObj.vertices[2] * w).toVec();
			textVec.v2 = litObj.vertices[0].y; //return y back to normal...
			int rowNum = (int) (textVec.v1 / checkW);
			int colNum = (int) (textVec.v3 / checkH); 

			if ( (rowNum % 2) == (colNum % 2) )
				return this.color2;

			return this.color1;
		}
	}
}
