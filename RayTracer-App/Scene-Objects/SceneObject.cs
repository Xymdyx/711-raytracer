using RayTracer_App.Illumination_Models;
using System.Numerics;
using RayTracer_App.Voxels;
using System;


//MATRIX 4D -> MATRIX4X4
namespace RayTracer_App.Scene_Objects
{
	public class SceneObject
	{
		//CONSTANTS
		public const float AIR_REF_INDEX = 1.00f;

		//fields
		protected string _material;
		protected Vector _normal;
		protected IlluminationModel _lightModel;
		protected Color _diffuse;
		protected Color _specular;
		protected float _kRefl;
		protected float _kTrans;
		protected float _refIndex;
		

		//properties 
		public string material {get => this._material; set => this._material = value; }
		public Vector normal { get => this._normal; set => this._normal = value; }
		public IlluminationModel lightModel { get => this._lightModel; set => this._lightModel = value; }
		public Color diffuse { get => this._diffuse; set => this._diffuse = value; }
		public Color specular { get => this._specular; set => this._specular = value; }
		public float kRefl { get => this._kRefl; set => this._kRefl = Math.Min( value, 1.0f - kTrans ); }
		public float kTrans { get => this._kTrans; set => this._kTrans = Math.Min( value, 1.0f - kRefl); }
		public float refIndex { get => this._refIndex; set => this._refIndex = value; }

		//constructors
		public SceneObject() { 
			this._material = "None";
			this._normal = null;
			this._lightModel = Phong.regularPhong;
			this._diffuse = Color.defaultBlack;
			this._specular = Color.whiteSpecular;
			this._kRefl = 0.0f;
			this._kTrans = 0.0f;
			this.refIndex = AIR_REF_INDEX;
		}

		public SceneObject( string material, IlluminationModel lightModel, Vector normal = null )
		{
			this._material = material;
			this._normal = normal;
			this._lightModel = lightModel;
			this._diffuse = Color.defaultBlack;
			this._specular = Color.whiteSpecular;
			this._kRefl = 0.0f;
			this._kTrans = 0.0f;
			this.refIndex = AIR_REF_INDEX;

		}

		//now with k Coefficients
		public SceneObject( string material, IlluminationModel lightModel, Color diffuse, Color specular, float kRefl = 0.0f, float kTrans = 0.0f, Vector normal = null )
		{
			this._material = material;
			this._normal = normal;
			this._lightModel = lightModel;
			this._diffuse = diffuse;
			this._specular = specular;
			this._kRefl = kRefl;
			this._kTrans = kTrans;
			this.refIndex = AIR_REF_INDEX;
		}


		//return distance along ray where it intersects an object....
		public virtual float intersect( LightRay ray )
		{
			return 0.0f;
		}

		public virtual Point getRayPoint( LightRay ray, float w )
		{
			return new Point( 0f, 0f, 0f );
		}


		public virtual void transform( Matrix4x4 camViewMat ){ return; }

		public virtual Color illuminate()
		{
			return new Color(); //return the background color
		}

		public virtual Point getMaxPt( int axis ) { return Point.origin;  }

		public virtual Point getMinPt( int axis ) { return Point.origin; }

		public virtual bool hasTexCoord() { return false; }

		//method for getting a random point on the object
		public virtual Point randomPointOn( Photon_Mapping.PhotonRNG pMapper = null ){ return new Point(); }


		//override ToString()
	}
}
