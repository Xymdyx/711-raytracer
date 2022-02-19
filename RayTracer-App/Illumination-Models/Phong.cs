using System;
using RayTracer_App.World;

namespace RayTracer_App.Illumination_Models
{
	public class Phong : IlluminationModel
	{
		// L = (ka*Co*La) + (kd * Sum..i=light( Li*Co (Si.dot(N) )) + (ks (Sum..i=light( Li*Cs (Ri.dot(V)^ke)
		// Co = objectColor, Li = LightRadiance(RGB), Cs = specular color
		// Vecs: N = normal, R = reflectionDir, S = dir of incoming light, V = incoming ray from cam
		// kd + ks < 1.. they range from 0 - 1 each

		private float _ka; // not going to implement since ambient will be shaved later
		private float _kd; // Lambertian diffuse
		private float _ks; // specular
		private float _ke;// specular exponent, determines size of specular highlight

		public float ka { get => this._ka; set => this._ka = value; }
		public float kd { get => this._kd; set => this._kd = value; }
		public float ks { get => this._ks; set => this._ks = value; }
		public float ke { get => this._ke; set => this._ke = value; }


		//precondiiton: all vectors normalized
		//TODO IMPLEMENT ILLUMINATE
		public override float illuminate( Point intersect, Vector normal, Vector incoming,
			Vector mirrorReflect, Vector cameraRay, LightSource[] lights )
		{
			// reflect = Incoming - 2( (Incoming.dot(normal) * normal) / (normalLength^2) )

			//if it doesn't hit an object, apply Phong sans the ambient
			return 0f; // no irradiance, we are in shadow
		}
	}
}
