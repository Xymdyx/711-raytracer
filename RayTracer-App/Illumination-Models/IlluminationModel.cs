using System.Collections.Generic;
using RayTracer_App.Scene_Objects;
using RayTracer_App.World;
using System;

namespace RayTracer_App.Illumination_Models
{
	//https://gfx.cs.princeton.edu/proj/brdf/brdf.pdf
	public abstract class IlluminationModel
	{
		// calculate ray from intersect to light sources. If it hits no objects, return resultant radience.
		// else return 0 for no radiance, making that point shadowed
		public enum modelIDs
		{
			PHONG = 0,
			PHONGBLINN = 1
		}

		//updated for laziness in CP2
		private float _kd; // Lambertian diffuse
		private float _ks; // specular
		private int _modelID; 

		public float kd { get => this._kd; set => this._kd = value; }
		public float ks { get => this._ks; set => this._ks = value; }
		public int modelID { get => this._modelID; set => this._modelID = value; }

		// given an ID number, we can return the correct casted Illumination Model.
		public static IlluminationModel castToProperModel( IlluminationModel model )
		{
			int modNum = model.modelID;
			switch(modNum)
			{
				case (0):
					return model as Phong;
				case (1):
					return model as PhongBlinn;
				default:
					Console.WriteLine( " Unknown model number, returning uncasted model." );
					break;
			}
			return model;
		}
		public abstract Color illuminate( Point intersect, Vector normal, LightRay incoming,
			Vector mirrorReflect, Vector cameraRay, LightSource light, SceneObject litObj );
		public abstract Color illuminate( Point intersect, Vector cameraRay, List<LightSource> lights, List<SceneObject> allObjs, SceneObject litObj, 
			bool transShadows = false, bool shadowPass = false, float shadowBias = 1e-4f );

		public abstract float mcBRDF( Vector incoming, Vector outgoing, Vector normal );

		//consider asking sampling over a unit disc... the Nusselt Analog https://en.wikipedia.org/wiki/View_factor#Nusselt_analog
		public abstract Vector mcDiffuseDir( float u1, float u2, Vector normal = null );

		// specular direction for PHONG BRDF for Monte Carlo.. picks random specular direction on unit hemisphere
		//u1 and u2 are random variables between 0 and 1 passed as variables
		public abstract Vector mcSpecDir( float u1, float u2, Vector normal = null );

		//pdf for specular component
		public abstract float specContribution( Vector incoming, Vector outgoing, Vector normal );

		//pdf for diffuse direction
		public abstract float diffuseContribution( Vector incoming, Vector normal );
	}
}
