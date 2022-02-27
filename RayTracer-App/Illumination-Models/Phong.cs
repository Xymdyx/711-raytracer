﻿using System;
using RayTracer_App.World;
using RayTracer_App.Scene_Objects;
using System.Collections.Generic;


namespace RayTracer_App.Illumination_Models
{
	public class Phong : IlluminationModel
	{
		// L = (ka*Co*La) + (kd * Sum..i=light( Li*Co (Si.dot(N) )) + (ks (Sum..i=light( Li*Cs (Ri.dot(V)^ke)
		// Co = objectColor, Li = LightRadiance(RGB), Cs = specular color
		// Vecs: N = normal, R = reflectionDir, S = dir of incoming light, V = incoming ray from cam
		// kd + ks < 1.. they range from 0 - 1 each

		//static constants for Phong
		public static Phong regularPhong = new Phong( 0f, .55f, .45f, 1f );

		private float _ka; // not going to implement since ambient will be shaved later
		private float _kd; // Lambertian diffuse
		private float _ks; // specular
		private float _ke;// specular exponent, determines size of specular highlight

		public float ka { get => this._ka; set => this._ka = value; }
		public float kd { get => this._kd; set => this._kd = value; }
		public float ks { get => this._ks; set => this._ks = value; }
		public float ke { get => this._ke; set => this._ke = value; }

		public Phong()
		{
			this.ka = 0f;
			this.kd = .65f;
			this.ks = .35f;
			this.ke = 1f;
		}

		public Phong( float ka, float kd, float ks, float ke ) 
		{
			this.ka = ka;
			this.kd = kd;
			this.ks = ks;
			this.ke = ke;
		}


		//precondiiton: all vectors normalized. we know the incoming ray makes it to a light source at this point
		//TODO IMPLEMENT ILLUMINATE... this returns an irradiance triplet, which will be converted by the camera via TR to a color.
		public override Color illuminate( Point intersect, Vector normal, LightRay incoming,
			Vector mirrorReflect, Vector cameraRay, LightSource light, SceneObject litObj )
		{
			// kd * (litObj.illuminate() * light.color * (incoming.dotProduct( Normal) ) + 
			Color diffuseTerm = litObj.diffuse * light.lightColor;
			diffuseTerm = diffuseTerm.scale( this.kd * incoming.direction.dotProduct( normal ) );

			// ks * (Color.specular * lights.color * (mirrorReflect.dotProduct( cameraRay)^ke) ;
			Color specTerm = litObj.specular * light.lightColor;
			float specReflDp = mirrorReflect.dotProduct( cameraRay );

			for (int exp = 1; exp < this.ke; exp++) //carry out power operation
				specReflDp *= specReflDp;

			specTerm = specTerm.scale( this.ks * specReflDp);
			//( this.ks * litObj.specular * light.lightColor * mirrorReflect.dotProduct( cameraRay ) );
			return diffuseTerm + specTerm;
		}
	}
}