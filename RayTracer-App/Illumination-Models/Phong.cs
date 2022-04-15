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
		//best 0f, .55f, .05f, 20f 
		public static Phong regularPhong = new Phong( 0f, .65f, .25f, 12f );
		public static Phong floorPhong = new Phong( 0f, .45f, .35f, 128f );

		private float _ka; // not going to implement since ambient will be shaved later
		//private float _kd; // Lambertian diffuse
		//private float _ks; // specular
		private float _ke;// specular exponent, determines size of specular highlight

		public float ka { get => this._ka; set => this._ka = value; }

		public float ke { get => this._ke; set => this._ke = value; }

		public Phong()
		{
			this.ka = 0f;
			this.kd = .65f;
			this.ks = .35f;
			this.ke = 1f;
			this.modelID = 0;
		}

		public Phong( float ka, float kd, float ks, float ke ) 
		{
			this.ka = ka;
			this.kd = kd;
			this.ks = ks;
			this.ke = ke;
			this.modelID = 0;
		}


		//precondiiton: all vectors normalized. we know the incoming ray makes it to a light source at this point
		//TODO IMPLEMENT ILLUMINATE... this returns an irradiance triplet, which will be converted by the camera via TR to a color.
		public override Color illuminate( Point intersect, Vector normal, LightRay incoming,
			Vector mirrorReflect, Vector cameraRay, LightSource light, SceneObject litObj ) //add list of lights, addObject list both from world, remove incoming, mirrorReflect
		{

			//// kd * (litObj.illuminate() * light.color * (incoming.dotProduct( Normal) ) + 
			//Vector incomingVec = -incoming.direction;
			//Color diffuseTerm = litObj.diffuse * light.lightColor;
			//diffuseTerm = diffuseTerm.scale( this.kd * incomingVec.dotProduct( normal ) ); //changed to be negative - 2/27

			//// ks * (Color.specular * lights.color * (mirrorReflect.dotProduct( cameraRay)^ke) ;
			//Color specTerm = litObj.specular * light.lightColor;
			//float specReflDp = mirrorReflect.dotProduct( cameraRay );
			//float totalSpecRefl = specReflDp;
			//totalSpecRefl = (float) Math.Pow( specReflDp, ke ) ;

			//specTerm = specTerm.scale( this.ks * totalSpecRefl);
			////( this.ks * litObj.specular * light.lightColor * mirrorReflect.dotProduct( cameraRay ) );
			//return diffuseTerm + specTerm;
			return Color.defaultBlack;
		}

		//precondiiton: the negative of the cameraRay gets passed so it is going TO the viewer's eye, not from
		//TODO IMPLEMENT ILLUMINATE... this returns an irradiance triplet, which will be converted by the camera via TR to a color.
		public override Color illuminate( Point intersect, Vector cameraRay, List<LightSource> lights, List<SceneObject> allObjs, SceneObject litObj, bool transShadows = false, float shadowBias = 1e-4f ) //add list of lights, addObject list both from world, remove incoming, mirrorReflect
		{

			Color lightIrradiance = Color.defaultBlack;

				foreach( LightSource light in lights ) 
				{
				//get normal vectors dependent on type of object. spawn shadow ray
					Vector shadowDisplacement = litObj.normal.scale( shadowBias );
					Point displacedOrigin = intersect + shadowDisplacement;
					LightRay shadowRay = new LightRay( light.position - displacedOrigin, displacedOrigin );

					SceneObject blocking = World.World.checkRayIntersectionObj( shadowRay, allObjs, light );
					float litPercent = 1.0f;

					if ((blocking != null) && (!(transShadows) || (blocking.kTrans <= 0.0f))) //the shadowRay gets blocked by an object on way to light
						continue;

					else if ((blocking != null) && (transShadows) && (blocking.kTrans > 0.0f))
						litPercent = blocking.kTrans;

				// reflect = Incoming - 2( (Incoming.dot(normal) * normal) / (normalLength^2) )
				// kd * (litObj.illuminate() * light.color * (shadowRay.dotProduct( Normal) ) + 
					Vector shadowRayVec = shadowRay.direction;
					Vector reflect = Vector.reflect( shadowRayVec, litObj.normal );

					Color diffuseTerm = litObj.diffuse * light.lightColor;
					float diffuseDP = (float) Math.Max( shadowRayVec.dotProduct( litObj.normal ), 0.0 ); //account for negative cosine
					diffuseTerm = diffuseTerm.scale( this.kd * diffuseDP );
				
				// ks * (Color.specular * lights.color * (mirrorReflect.dotProduct( cameraRay)^ke) ;
					Color specTerm = litObj.specular * light.lightColor;
					float specReflDp = (float) Math.Max( reflect.dotProduct( cameraRay ), 0.0 );
					float totalSpecRefl = specReflDp;
					totalSpecRefl = (float) Math.Pow( specReflDp, ke );

					specTerm = specTerm.scale( this.ks * totalSpecRefl );
					//( this.ks * litObj.specular * light.lightColor * mirrorReflect.dotProduct( cameraRay ) );
					lightIrradiance += (diffuseTerm + specTerm).scale(litPercent);
				}

			return lightIrradiance;
		}

		// BRDF used for Monte Carlo Distributed Ray Tracing as described in 9-2 slide deck
		//https://www.cs.princeton.edu/courses/archive/fall16/cos526/papers/importance.pdf
		//https://www.cs.princeton.edu/courses/archive/fall03/cs526/papers/lafortune94.pdf
		public float mcBRDF( Vector incoming, Vector outgoing, Vector normal )
		{
			// fr( x, Oi, Oo) = kd * (1/pi) + ks * ( (n+2)/(2pi)) * cos^n alpha
			// Oi = incoming, Oo = outgoing, x = intersection pt with object
			// kd, ks, n are all here... n = specular exponent
			// alpha = angle between reflective direction and outgoing ray direction... (their dot product raised to the n) max is pi/2
			// kd + ks <= 1
			Vector refl = Vector.reflect( outgoing, normal );
			float cos = refl.dotProduct( outgoing );
			float diffTerm = (float) (this.kd / Math.PI);
			float specTerm = (float) ( (this.ks) * ((this.ke + 2) / (2 * Math.PI)) * cos );

			return diffTerm + specTerm;
		}

		public Vector mcDiffuseDir( float u1, float u2 )
		{
			float u1Sqrt = (float) Math.Sqrt( u1 );
			float sqrtScaler = (float) Math.Sqrt( 1 - u1 );

			float theta = (float) Math.Acos( u1Sqrt );
			float azithumal = (float) (2 * Math.PI * u2);

			//from princeton... sopherical -> vector // sintheta * cosazi, sintheta * sizazi, costheta
			float x = (float) (sqrtScaler * Math.Cos( azithumal )); // sqrt( 1-u1) * cosazi
			float y = (float)(sqrtScaler * Math.Sin( azithumal )); // sqrt(1-u) *sinazi
			float z = u1Sqrt; // sqrt(u1)

			return new Vector( x, y, z ); //normalized
		}

		// specular direction for PHONG BRDF for Monte Carlo.. picks random specular direction on unit hemisphere
		//u1 and u2 are random variables between 0 and 1 passed as variables
		public Vector mcSpecDir( float u1, float u2 )
		{
			//from princeton... sopherical -> vector // sintheta * cosazi, sintheta * sizazi, costheta
			float u1Pow = (float) Math.Pow( u1, (1/ (this.ke + 1 )) ); // u1^ 1/ (n +1)

			float sqrtScaler = (float)Math.Sqrt( 1 -
				Math.Pow( u1, (2 / (this.ke + 1))) ); // sqrt( u1^ 2/ (n +1) )


			float alpha = (float) Math.Acos( u1Pow );
			float azithumal = (float)(2 * Math.PI * u2);

			//from princeton... sopherical -> vector // sintheta * cosazi, sintheta * sizazi, costheta
			float x = (float)(sqrtScaler * Math.Cos( azithumal )); // sqrtScaler * cosazi
			float y = (float)(sqrtScaler * Math.Sin( azithumal )); // sqrtScaler *sinazi
			float z = u1Pow; 

			return new Vector( x, y, z ); //normalized
		}
	}
}
