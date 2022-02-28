using System.Collections.Generic;
using RayTracer_App.Scene_Objects;
using RayTracer_App.World;
namespace RayTracer_App.Illumination_Models
{
	public abstract class IlluminationModel
	{
		// calculate ray from intersect to light sources. If it hits no objects, return resultant radience.
		// else return 0 for no radiance, making that point shadowed

		public abstract Color illuminate( Point intersect, Vector normal, LightRay incoming,
			Vector mirrorReflect, Vector cameraRay, LightSource light, SceneObject litObj );
		public abstract Color illuminate( Point intersect, Vector cameraRay, List<LightSource> lights, List<SceneObject> allObjs, SceneObject litObj );

	}
}
