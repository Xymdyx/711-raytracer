using System;
using RayTracer_App.World;
using RayTracer_App.Scene_Objects;
using System.Collections.Generic;

namespace RayTracer_App.Illumination_Models
{
	public class PhongBlinn : IlluminationModel
	{
		// L = (ka*Co*La) + (kd * Sum..i=light( Li*Co (Si.dot(N) )) + (ks (Sum..i=light( Li*Cs (H.dot(N)^ke)
		// Co = objectColor, Li = LightRadiance(RGB), Cs = specular color
		// Vecs: N = normal, H = halfway vector ( camera + shadowRay.direction ), S = dir of incoming light, V = incoming ray from cam
		// kd + ks < 1.. they range from 0 - 1 each

		//static constants for PhongBlinn
		//best 0f, .55f, .05f, 20f 
		public static PhongBlinn regularPhongBlinn = new PhongBlinn ( 0f, .65f, .25f, 12f );
		public static PhongBlinn floorPhongBlinn = new PhongBlinn( 0f, .45f, .35f, 128f );
		public static PhongBlinn bunnyBlinn = new PhongBlinn( 0f, .55f, .45f, 128f );


		private float _ka; // not going to implement since ambient will be shaved later
		private float _kd; // Lambertian diffuse
		private float _ks; // specular
		private float _ke;// specular exponent, determines size of specular highlight

		public float ka { get => this._ka; set => this._ka = value; }
		public float kd { get => this._kd; set => this._kd = value; }
		public float ks { get => this._ks; set => this._ks = value; }
		public float ke { get => this._ke; set => this._ke = value; }

		public PhongBlinn()
		{
			this.ka = 0f;
			this.kd = .65f;
			this.ks = .35f;
			this.ke = 1f;
			this.modelID = 1;
		}

		public PhongBlinn( float ka, float kd, float ks, float ke ) 
		{
			this.ka = ka;
			this.kd = kd;
			this.ks = ks;
			this.ke = ke;
			this.modelID = 1;
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
			return Color.defaultBlack;
		}

		//precondiiton: the negative of the cameraRay gets passed so it is going TO the viewer's eye, not from
		//TODO IMPLEMENT ILLUMINATE... this returns an irradiance triplet, which will be converted by the camera via TR to a color.
		public override Color illuminate( Point intersect, Vector cameraRay, List<LightSource> lights, List<SceneObject> allObjs, SceneObject litObj, 
			bool transShadows = false, bool shadowPass = false, float shadowBias = 1e-6f ) //add list of lights, addObject list both from world, remove incoming, mirrorReflect
		{

			Color lightIrradiance = Color.defaultBlack;

				foreach( LightSource light in lights ) //TODO - 2/20
				{
				//get normal vectors dependent on type of object. spawn shadow ray with slight shadow bias to get it above the surface of the object
					Vector shadowDisplacement = litObj.normal.scale( shadowBias );
					Point displacedOrigin = intersect + shadowDisplacement;
					LightRay shadowRay = new LightRay( light.position - displacedOrigin, displacedOrigin );
					SceneObject blocking = World.World.checkRayIntersectionObj( shadowRay, allObjs, light );
					float litPercent = 1.0f;
			
					if ( !shadowPass && (blocking != null) && ( !(transShadows) || (blocking.kTrans <= 0.0f) ) ) //the shadowRay gets blocked by an object on way to light
						continue;

					else if ( !shadowPass && (blocking != null) && (transShadows) && (blocking.kTrans > 0.0f))
						litPercent = blocking.kTrans;

					// kd * (litObj.illuminate() * light.color * (shadowRay.dotProduct( Normal) ) + 
					Vector shadowRayVec = shadowRay.direction;

					//use halfway vector in lieu of the normal
					// H = L + V
					Vector halfWay = cameraRay + shadowRayVec; //these need to go TO the intersection point and bounce to the viewer...according to wikipedia

					Color diffuseTerm = litObj.diffuse * light.lightColor;
					float diffuseDP = (float) Math.Max( shadowRayVec.dotProduct( litObj.normal ), 0.0 ); //account for negative cosine
					diffuseTerm = diffuseTerm.scale( this.kd * diffuseDP ); //changed to be negative - 2/27

					// ks * (Color.specular * lights.color * (halfway.dotProduct(obj.Normal)^ke) ;
					Color specTerm = litObj.specular * light.lightColor;
					float specReflDp = (float) Math.Max( halfWay.dotProduct( litObj.normal ), 0.0 );
					float totalSpecRefl = specReflDp;
					totalSpecRefl = (float )Math.Pow( specReflDp, ke );

					specTerm = specTerm.scale( this.ks * totalSpecRefl );
					//( this.ks * litObj.specular * light.lightColor * mirrorReflect.dotProduct( cameraRay ) );
					lightIrradiance += (diffuseTerm + specTerm).scale( litPercent);
				}

			return lightIrradiance;
		}
	}
}
