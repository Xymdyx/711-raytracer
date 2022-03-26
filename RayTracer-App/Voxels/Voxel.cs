using RayTracer_App;

namespace RayTracer_App.Voxels 
{ 
		public abstract class Voxel
		{

		//fields and properties
			protected int _shape;
			protected Point _max;
			protected Point _min;
			public int shape { get => this._shape; set => this._shape = value; }
			public Point max { get => this._max; set => this._max = value; }
			public Point min { get => this._min; set => this._min = value; }


		public abstract bool intersect( LightRay ray );

		}
}
