using System;
using System.Collections.Generic;
using System.Text;
using OpenGLDotNet.Math; // for Matrix4d
using RayTracer_App.World;

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
		//cameraTransform
		private Matrix4d _camTransformMat;


		public Vector up { get => this._up; set => this._up = value; } 
		public Point eyePoint { get => this._eyePoint; set => this._eyePoint = value; }
		public Point lookAt { get => this._lookAt; set => this._lookAt = value; }
		public Matrix4d camTransformMat { get => this._camTransformMat; set => this._camTransformMat = value; }



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
//helper methods for facilitating needed components to define camera coords matrix
		private Vector calculateN() { return eyePoint - lookAt; }
		private Vector calculateU() { return up.crossProduct( calculateN() ); }
		private Vector calculateV() { return calculateN().crossProduct( calculateU() ); }

		private Matrix4d makeProjMat( double fov, double aspect, double zNear, double zFar)
		{
			double f = Math.Tan( 2 / (fov * (Math.PI / 180) ));
			double zDivisor = (zNear - zFar);

			return new Matrix4d
			((f/aspect), 0, 0, 0,
			 0, f, 0, 0,
			 0, 0, ( (zFar + zNear) / (zDivisor)), ((2* zFar * zNear) / (zDivisor)),
			 0, 0, -1, 0);
		}

		private void makeCamMat()
		{
			Vector N = calculateN();
			Vector U = calculateU();
			Vector V = calculateV();
			Vector eyeVec = -eyePoint.toVec();
			Matrix4d camCoordMat = new Matrix4d
				( U.v1, V.v1, N.v1, 0,
				U.v2, V.v2, N.v2, 0,
				U.v3, V.v3, N.v3, 0,
				eyeVec.dotProduct( U ), eyeVec.dotProduct( V ), eyeVec.dotProduct( N ), 1 ); //sans the projection...

			Matrix4d persp = makeProjMat( 90, 1.0, 1.0, 50 );
			this.camTransformMat = camCoordMat;
			
		}


//TODO render method
//tried list of float[] and float[]...
		public byte[] render( World.World world, int imageHeight, int imageWidth )
		{
			double focalLen = .25; //1 / Math.Tan( (90 / 2) * (Math.PI / 180) ); //distance from camera to film plane along N...
			double fpHeight = 1.0;
			double fpWidth = 1.0;

			// for re-defining the film-plane width at some poitn

			//pixel info
			double pixHeight = fpHeight / imageHeight;
			double pixWidth = fpWidth / imageWidth;

			//initialize default background color at all pixels to begin with
			Color bgColor = new Color();
			byte[] bgArr = bgColor.asByteArr();
			byte[] pixColors = new byte[imageHeight * imageWidth * 3];


			int modIdx = 0;
			for (int add = 0; add < imageHeight * imageWidth * 3 ; add++)
			{
				modIdx = add % 3;

				if (modIdx == 0) pixColors[add] = bgArr[0]; //red
				else if (modIdx == 1) pixColors[add] = bgArr[1]; //green
				else pixColors[add] = bgArr[2]; //blue
			}

			makeCamMat();
			world.transformAll( camTransformMat );

			//fpHeight - (pixHeight / 2) for y originally
			Point fpPoint = new Point( (-fpWidth / 2) + pixWidth / 2, (pixHeight / 2), focalLen);
			LightRay fire = new LightRay( fpPoint - this.eyePoint , this.eyePoint );
			Color hitColor = null;
			byte[] hitColorArr = null;

			// this converts everything to camera coords
			// for x =- 0; x < x pixels; x+= pixelwidth
			//	for y = -; y < y-pixels; y+= pixelHeight
			//		world.spawnRay()... see what it hits
			//		whatever it hits... rgbs.add( rgb float triplet)
			for ( int y = 0; y < imageHeight; y++) // positive x ->, positive y V
			{
				for ( int x = 0; x < imageWidth; x++)
				{
					fire.direction = fpPoint - this.eyePoint;
					hitColor = world.spawnRay( fire );

					if (hitColor != null)
					{
						hitColorArr = hitColor.asByteArr();
						int pos = (x + y * imageWidth) * 3;
						pixColors[pos] = hitColorArr[0]; //try 0-1.0 floats instead of 255
						pixColors[pos + 1] = hitColorArr[1]; //try 0-1.0 floats instead of 255
						pixColors[pos + 2] = hitColorArr[2]; //try 0-1.0 floats instead of 255
					}

					fpPoint.x += pixWidth;
				}
				fpPoint.y += pixHeight;
			}
			return pixColors ;
		}

	}
}
