using System;
using System.Collections.Generic;
using System.Text;
using RayTracer_App.World_Stuff;

namespace RayTracer_App.Camera
{
	public class Camera
	{
		//field-plane
		private Vector _up;
		//eyepoint
		private Point _eyePoint;
		//lookat
		private Point _lookAt;
		
		public Vector up { get => this._up; set => this._up = value; } 
		public Point eyePoint { get => this._eyePoint; set => this._eyePoint = value; }
		public Point lookAt { get => this._lookAt; set => this._lookAt = value; }

//default constructor... TODO define world origin as default
		public Camera()
		{
			this._up = new Vector( 0, 1, 0 );
			this._eyePoint = new Point( 0, 0, 0 ); // world origin is the default
			this._lookAt = new Point( 0, 0, 10 ); // default lookat position
		}

// parameter constructor...
		public Camera( Vector up, Point eyePoint, Point lookAt )
		{
			this._up = up;
			this._eyePoint = eyePoint;
			this._lookAt = lookAt;
		}

//METHODS
		//render method
		public void render( World world )
		{ 
			return;
		}

	}
}
