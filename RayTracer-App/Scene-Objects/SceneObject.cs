using RayTracer_App.Illumination_Models;
using System.Numerics;


//MATRIX 4D -> MATRIX4X4
namespace RayTracer_App.Scene_Objects
{
	public class SceneObject
	{
		//fields
		private string _material;
		private Vector _normal;
		private IlluminationModel _lightModel;

		//properties 
		public string material {get => this._material; set => this._material = value; }
		public Vector normal { get => this._normal; set => this._normal = value; }
		public IlluminationModel lightModel { get => this._lightModel; set => this._lightModel = value; }


		//constructors
		public SceneObject() { 
			this._material = "None";
			this._normal = null;
			this._lightModel = Phong.regularPhong;
		}

		public SceneObject( string material, IlluminationModel lightModel, Vector normal = null )
		{
			this._material = material;
			this._normal = normal;
			this._lightModel = lightModel;
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

		//override ToString()
	}
}
