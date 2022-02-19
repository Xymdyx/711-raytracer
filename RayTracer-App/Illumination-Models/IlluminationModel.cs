using System;
using System.Collections.Generic;
using System.Text;
using RayTracer_App.World;
namespace RayTracer_App.Illumination_Models
{
	public abstract class IlluminationModel
	{
		// calculate ray from intersect to light sources. If it hits no objects, return resultant radience.
		// else return 0 for no radiance, making that point shadowed

		//TODO IMPLEMENT REFLECT FUNCTION
		public abstract float illuminate( Point intersect, Vector normal, Vector incoming,
			Vector mirrorReflect, Vector cameraRay, LightSource[] lights );
	}
}
