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
	public class CheckerBoardPattern : IlluminationModel
	{
		private Color _color1;
		private Color _color2;

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

		//precondiiton: all vectors normalized. we know the incoming ray makes it to a light source at this point
		//TODO IMPLEMENT ILLUMINATE... this returns an irradiance triplet, which will be converted by the camera via TR to a color.
		public override Color illuminate( Point intersect, Vector normal, LightRay incoming,
			Vector mirrorReflect, Vector cameraRay, LightSource light, SceneObject litObj ) //add list of lights, addObject list both from world, remove incoming, mirrorReflect
		{
			return Color.defaultBlack;
		}

		//precondiiton: the negative of the cameraRay gets passed so it is going TO the viewer's eye, not from
		//TODO IMPLEMENT ILLUMINATE... this returns an irradiance triplet, which will be converted by the camera via TR to a color.
		public override Color illuminate( Point intersect, Vector cameraRay, List<LightSource> lights, List<SceneObject> allObjs, SceneObject litObj, float shadowBias = 1e-6f ) //add list of lights, addObject list both from world, remove incoming, mirrorReflect
		{

			Color lightIrradiance = Color.defaultBlack;

			/* implement checkerboard procedural texture */
			/* origin of the floor is: -6f, floorHeight, 60.5f  
			 * x : 76.5f
			 * z: 58.5f 
			 * need u and v from triangle.intersect()
			 * The interpolation for a given set of barycentric
				coordinates (u, v, w) is given by:
				T = uT0+ vT1+ wT2
			 * transform algo: find row and col where intersect occurs, if row and col's parity match, it's red. else, yellow */

			return lightIrradiance;
		}
}
}
