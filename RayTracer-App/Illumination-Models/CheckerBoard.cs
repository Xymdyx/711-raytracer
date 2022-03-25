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
		public static int DEFAULT_DIMS = 32;

		public Color color1 { get => this._color1; set => this._color1 = value; }
		public Color color2 { get => this._color2; set => this._color2 = value; }

		public CheckerBoardPattern()
		{
			this._color1 = new Color( 0.950f, 0.936f, 0.114f ); //yellow
			this._color2 = Color.floorColor;

		}

		public CheckerBoardPattern( Color color1, Color color2)
		{
			this._color1 = color1;
			this._color2 = color2;
		}


		//precondiiton: the negative of the cameraRay gets passed so it is going TO the viewer's eye, not from
		//this returns an irradiance triplet, which will be used by the Phong model
		public Color illuminate( Polygon litObj, int rows = 3, int cols = 3 ) //add list of lights, addObject list both from world, remove incoming, mirrorReflect
		{
			float u = litObj.u;
			float v = litObj.v;
			float w = 1 - (u + v);
			float floorX = 1f; 
			float floorZ = 1f;
			float checkW = (float) (floorX / (float) cols); 
			float checkH = (float) (floorZ / (float) rows); 

			//T = wT0 + uT1 + vT2... same as wp0 + up1 + vp2
			Vector texVec1 = (litObj.vertices[0].texCoord * w).toVec();
			Vector texVec2 = (litObj.vertices[1].texCoord * u).toVec();
			Vector texVec3 = (litObj.vertices[2].texCoord * v).toVec();

			Vector textVec = texVec1.addVec( texVec2 );
			textVec = textVec.addVec( texVec3 );
			int rowNum = (int) ( (textVec.v3 / checkH) );
			int colNum = (int)( (textVec.v1 / checkW) );

			//transform func
			/*transform algo: find row and col where intersect occurs, if row and col's parity match, it's red. else, yellow */
			if ( (rowNum % 2) == (colNum % 2) )
				return this.color2;

			return this.color1;
		}
	}
}
