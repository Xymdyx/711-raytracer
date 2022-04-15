using System.Collections.Generic;
using RayTracer_App.Scene_Objects;
using RayTracer_App.World;
namespace RayTracer_App.Illumination_Models
{
	public abstract class IlluminationModel
	{
		// calculate ray from intersect to light sources. If it hits no objects, return resultant radience.
		// else return 0 for no radiance, making that point shadowed

		//updated for laziness in CP2
		private float _kd; // Lambertian diffuse
		private float _ks; // specular

		public float kd { get => this._kd; set => this._kd = value; }
		public float ks { get => this._ks; set => this._ks = value; }


		public abstract Color illuminate( Point intersect, Vector normal, LightRay incoming,
			Vector mirrorReflect, Vector cameraRay, LightSource light, SceneObject litObj );
		public abstract Color illuminate( Point intersect, Vector cameraRay, List<LightSource> lights, List<SceneObject> allObjs, SceneObject litObj, bool transShadows = false, float shadowBias = 1e-4f );

		//consider asking sampling over a unit disc... the Nusselt Analog https://en.wikipedia.org/wiki/View_factor#Nusselt_analog
		public virtual Vector mcDiffuseDir( float u1, float u2 )
		{
			return null;
		}

		// specular direction for PHONG BRDF for Monte Carlo.. picks random specular direction on unit hemisphere
		//u1 and u2 are random variables between 0 and 1 passed as variables
		public virtual Vector mcSpecDir( float u1, float u2 )
		{
			return null;
		}
	}
}
