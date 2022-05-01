using System;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics;
using RayTracer_App.Photon_Mapping;
using RayTracer_App.World;

//MATRIX 4D -> MATRIX4X4

namespace RayTracer_App.Camera
{
	public class Camera
	{
		//Ward's contrived constants
		private const float W_FLOAT = 1.219f; //contrived number greg ward came up with
		private const float W_INNER_POW = .4f; //contrived power to raise inside terms
		private const float W_OUTER_POW = 2.5f; //contrived power to raise whole fraction that reps scale factor

		//field-plane
		private Vector _up;
		//eyepoint
		private Point _eyePoint;
		//lookat
		private Point _lookAt;
		//cameraTransform
		private Matrix4x4 _camTransformMat;

		//tone reproduction operator
		private TR_MODEL _trOperator;

		//the max luminance of the target device...
		private float _ldMax; //typically 80 - 120 nits is acceptable range...

		// the max luminance of the display device
		// private float _displayMax;

		public Vector up { get => this._up; set => this._up = value; } 
		public Point eyePoint { get => this._eyePoint; set => this._eyePoint = value; }
		public Point lookAt { get => this._lookAt; set => this._lookAt = value; }
		public Matrix4x4 camTransformMat { get => this._camTransformMat; set => this._camTransformMat = value; }
		public TR_MODEL trOperator { get => this._trOperator; set => this._trOperator = value; }

		public enum TR_MODEL
		{
			ERROR = -1,
			LINEAR = 0,
			WARD = 1,
			REINHARD = 2,
		}

		//default constructor... TODO define world origin as default
		public Camera()
		{
			this._up = new Vector( 0, 1, 0 );
			this._eyePoint = new Point( 0, 0, 0 ); // world origin is the default
			this._lookAt = new Point( 0, 0, 10 ); // default lookat position
			this.camTransformMat = Matrix4x4.Identity;
			this._trOperator = TR_MODEL.LINEAR;
			this._ldMax = 80f;
		}

		// parameter constructor...
		public Camera( Vector up, Point eyePoint, Point lookAt, TR_MODEL trOperator = TR_MODEL.LINEAR, float _ldMax = 100f ) // my monitor's max luminance is 270 nits
		{
			this._up = up;
			this._eyePoint = eyePoint;
			this._lookAt = lookAt;
			this.camTransformMat = Matrix4x4.Identity;
			this._trOperator = trOperator;
			this._ldMax = _ldMax;  //typically 80 - 120 nits is acceptable range for max luminances of computer monitors

		}

		//METHODS
		//http://www.songho.ca/opengl/gl_camera.html
		// https://github.com/sgorsten/linalg/issues/29 ... sanity checks that I am doing this correctly

		//gives the forward = target - camPos
		private Vector calculateN() { return lookAt - eyePoint; } 

		//gives the left = norm(cross(up, forward))
		private Vector calculateU(Vector forward) { return this.up.crossProduct( forward ); }

		//gives the y-axis (up-axis) = cross( forward, left)
		private Vector calculateV(Vector forward, Vector left) { return forward.crossProduct( left ); } //should be normalized now... 

		private void makeCamMat()
		{
			//use identity if world origin
			Matrix4x4 camCoordMat = Matrix4x4.Identity; //row major
		
			//https://www.geertarien.com/blog/2017/07/30/breakdown-of-the-lookAt-function-in-OpenGL/... from LHS

			Vector zAxis = calculateN(); // camera FORWARD direction
			Vector xAxis = calculateU( zAxis );
			Vector yAxis = calculateV( zAxis, xAxis );
			Vector eyeVec = eyePoint.toVec();

			/*	camCoordMat = new Matrix4x4( xAxis.v1, yAxis.v1, zAxis.v1, 0,
							xAxis.v2, yAxis.v2, zAxis.v2, 0,
							xAxis.v3, yAxis.v3, zAxis.v3, 0,
		-(eyeVec.dotProduct( xAxis)), -(eyeVec.dotProduct( yAxis)), -(eyeVec.dotProduct( zAxis)), 1);*/
			float eyeXDP = -(eyeVec.dotProduct( xAxis ));
			float eyeYDP = -(eyeVec.dotProduct( yAxis ));
			float eyeZDP = -(eyeVec.dotProduct( zAxis ));

			camCoordMat = new Matrix4x4
							( xAxis.v1, xAxis.v2, xAxis.v3,	eyeXDP,
							yAxis.v1, yAxis.v2, yAxis.v3, eyeYDP,
							zAxis.v1, zAxis.v2, zAxis.v3, eyeZDP,
							0, 0, 0, 1 );

			this.camTransformMat = camCoordMat;
			
		}

		//attempt at supersampling with only doing 4 basic corners of a pixel
		public Color superSamplePixel( Point centerPoint, float pixHeight, float pixWidth, World.World world )
		{
			Color averageHitColor = null;
			Color[] hitColors = new Color[4]; //for super-sampling
			Point[] hitPoints = new Point[4] {
				new Point( centerPoint.x - pixWidth / 2f, centerPoint.y + pixHeight / 2f, centerPoint.z), //top-left
				new Point( centerPoint.x + pixWidth / 2f, centerPoint.y + pixHeight / 2f, centerPoint.z ), //top-right
				new Point( centerPoint.x - pixWidth / 2f, centerPoint.y - pixHeight / 2f, centerPoint.z ), //bottom-left
				new Point( centerPoint.x + pixWidth /2f, centerPoint.y - pixHeight / 2f, centerPoint.z ), //bottom-right
			};
			
			for( int hitIdx = 0; hitIdx < 4; hitIdx ++ )
			{
				LightRay hitRay = new LightRay( hitPoints[hitIdx] - this.eyePoint, this.eyePoint );
				hitColors[hitIdx] = world.spawnRay( hitRay, 1 ); //cp5
			}

			//average the 4 colors
			for (int colNum = 0; colNum < 4; colNum++)
			{
				if (hitColors[colNum] != null)
				{
					if (averageHitColor == null)
						averageHitColor = hitColors[colNum];
					else
						averageHitColor += hitColors[colNum];
				}
			}

			if ( averageHitColor != null )
				averageHitColor = averageHitColor.scale( .25f );

			return averageHitColor;
		}

////////////////////////////////////////////////////////// PHOTN MAPPING METHODS////////////////////////////////////////////////////////////////////
		//helper for photon visualizing
		public Color renderPhotons( Color orig, LightRay fire, World.World world, bool blackout = false )
		{
			Color photonColor = world.overlayPhotons( fire, true ); //22 minutes w raw intersection for 1000 photons... 46 caustics. 139 total
																	//23 with kd, 109 total
			if (photonColor == Color.defaultBlack && !blackout) // if we aren't blacking out the scene...
				return orig;

			return photonColor;
		}


	//photon mapping debugging information
		public void pmDebug(World.World world)
		{
			world.photonMapper.rrStats();
			world.photonMapper.printPhotonsInScene( world.sceneBB, PhotonRNG.MAP_TYPE.GLOBAL );
			world.photonMapper.printPhotonsInScene( world.sceneBB, PhotonRNG.MAP_TYPE.CAUSTIC );
			Console.WriteLine( world.photonMapper.photonSearchStats() );
			Console.WriteLine( "Global PM: " + world.photonMapper.globalPM.heapPrint() );
			Console.WriteLine( $"Most photons gathered : {world.highestK}/ {world.allK}" );
			int lightNum = 1;
			foreach (LightSource l in world.lights)
			{
				Console.WriteLine( $"Light {lightNum} @ {l.position} had {l.power} watts and shot {l.ne} photons" );

			}
		}

		//photon overlay debugging information
		public void photOverlayInfo( World.World world )
		{
			Console.WriteLine( $"Lit caustics: {world.causticHits}\n Total hits {world.photoHits}" );
		}


////////////////////////////////////////////////////////// TONE REPRODUCTION METHODS////////////////////////////////////////////////////////////////////

			//gets log average illuminance in base 10 of the whole scene.
			// i.e... Lbar = base^( ( Sum (for all pixels) logbase( delta + L(x,y) ) / pixels)
			private float getIllumLogAvg( int x, int y, List<Color> illums, double based = Math.E)
		{
			double logAvg = 0f;
			float delta = 1e-6f;

			for (int illum = 0; illum < illums.Count; illum++)
			{
				Color pixIllum = illums[illum];
				logAvg += (float) Math.Log( delta + pixIllum.colVal(), based );
			}

			logAvg /=  illums.Count; //divide by total pixel number to get logAvg

			return (float) Math.Pow( Math.E, logAvg); // base^ logavg.... bases must match
		}

		// given an irradiance value for a pixel
		// convert to CRT illuminance and store in a separate array called
		// hitIlluminances. Needed for Ward and Reinhard to take log average
		// of all illuminances in the scene
		public Color toCRTIllum( Color irradiance )
		{
			// L(x,y) = .27Ri + .67Gi + .06b
			return new Color( irradiance.r * .27f,
				irradiance.g * .67f, irradiance.b * .06f );
		}

		//called by Ward tone reproduction method to get the scale factor to store the lumiannces by
		// sf = [ (1.219 + (LdMax/2)^.4f)/ 1.219 + Lwa^.4)]^2.5
		private float getWardSF( float lwa)
		{
			float num = (float)( W_FLOAT + Math.Pow( (this._ldMax / 2f), W_INNER_POW ));
			float denom = (float) (W_FLOAT + Math.Pow( lwa, W_INNER_POW ));
	
			return (float)Math.Pow( num / denom, W_OUTER_POW );
		}

		//TODO CP7
		// runs tone reproduction via Ward's formula
		public List<Color> runWardTR( List<Color> illuminances, float logAvg )
		{
			List<Color> wardCols = new List<Color>();
			float sf = getWardSF( logAvg );
			Color wardCol;

			//for each pixel world illuminance, Ld = sf * Lw
			foreach ( Color illuminance in illuminances)
			{
				wardCol = illuminance.scale( sf/this._ldMax );
				wardCols.Add( wardCol );
			}

			return wardCols;
		}

		//TODO CP7
		// runs tone reproduction via Reinard's tone reproduction formula
		// based on Ansel Adam's Zone System
		// alpha = trnsparency, which is .18f for 50% reflectivity in grayscale (Zone V)
		public List<Color> runReinhardTR( List<Color> illuminances, float keyVal, float alpha = .18f)
		{
			List<Color> rhCols = new List<Color>();
			Color rhCol;
			foreach (Color illuminance in illuminances)
			{
				//step 1:  Create scaled luminance values Rs, Gs, Bs by mapping 
				// the key value to Zone V(18 % gray)
				//scale RGB illums by alpha/ keyVal
				Color scaledIllum = illuminance.scale( alpha / keyVal ); 

				//step 2:  Find the reflectance for Rr, Gr, Br, based on film-like response
				float reflR = scaledIllum.r / (1f + scaledIllum.r);
				float reflG = scaledIllum.g / (1f + scaledIllum.g);
				float reflB = scaledIllum.b / (1f + scaledIllum.b);
				rhCol = new Color( reflR, reflG, reflB );

				//step 3 is where we would multiply each RGB comp by LDmax and then divide by maxDisplay illuminance
				// since our target and display devices are the same, LdMax/LdMax cancels out and we don't need this.
				//trColor.scale( LdMax/ displayMax);

				rhCols.Add( rhCol );
			}

			return rhCols;
		}

		// linear operator that clamps to 1 right now
		public List<Color> runLinearTRAll( List<Color> irradiances )
		{
			List<Color> linCols = new List<Color>();

			foreach (Color irradiance in irradiances) 
			{
				Color trColor = new Color( 0, 0, 0 );
				trColor.r = irradiance.r >= 1.0f ? trColor.r = 1.0f : trColor.r = irradiance.r;
				trColor.g = irradiance.g >= 1.0f ? trColor.g = 1.0f : trColor.g = irradiance.g;
				trColor.b = irradiance.b >= 1.0f ? trColor.b = 1.0f : trColor.b = irradiance.b;
				linCols.Add( trColor );
			}
			return linCols;
		}

		// convenience method for running proper TR method based on camera trOperator field
		// runs tone reproduction on the irradiance triplet retrieved from an intersection
		public List<Color> runTRAll( List<Color> irradiances, List<Color> illums, int x, int y, float rhKey = float.MinValue )
		{
			List<Color> trColors = new List<Color>();
			float logAvg = 0f; //change to get logAvg

			switch (this._trOperator)
			{
				case (TR_MODEL.LINEAR):
					trColors = runLinearTRAll( irradiances );
					break;
				case (TR_MODEL.WARD):
					logAvg = getIllumLogAvg( x, y, illums );
					trColors = runWardTR( illums, logAvg );
					break;
				case (TR_MODEL.REINHARD):
					if (rhKey == float.MinValue)
					{
						logAvg = getIllumLogAvg( x, y, illums );
						trColors = runReinhardTR( illums, logAvg );
					}
					else 
						trColors = runReinhardTR( illums, rhKey );
					break;
				default:
					trColors = new List<Color> ( new Color[irradiances.Count]); //all background colors
					break;
			}

			return trColors;
		}

////////////////////////////////////////////////////////// RENDER METHODS////////////////////////////////////////////////////////////////////

		//helper method to convert a list of Colors to a byte[] that holds rgb triplets
		// for openGL draw pixls to render...
		public byte[] colsToBytes( List<Color> trCols, int imageWidth, int imageHeight )
		{
			byte[] pixCols = new byte[imageHeight * imageWidth * 3]; //openGL drawPixels byte[]
			byte[] trCol; //the 255 byte array rep of a pixel

			for ( int pixTriplet = 0; pixTriplet < trCols.Count; pixTriplet++)
			{
				trCol = trCols[pixTriplet].asByteArr();
				int tripPos = pixTriplet * 3;
				pixCols[tripPos] = trCol[0];
				pixCols[tripPos+  1] = trCol[1];
				pixCols[tripPos + 2] = trCol[2];
			}

			return pixCols;
		}

		//tried list of float[] and float[]...
		public byte[] render( World.World world, int imageHeight, int imageWidth, float focalLen, int rhPixVal = -1, bool makeKd = false, bool doPM = false, bool doCaustics = false )
		{
			// this converts everything to camera coords
			makeCamMat();
			world.transformAll( this.camTransformMat );

			if (makeKd)
			{
				world.findBB(); //advanced cp 1
				world.buildKd();
			}

			if(doPM)
			{
				if( world.sceneBB == null)
					world.findBB(); 

				world.beginpmPassOne( doCaustics ); //phton trace
				world.photonMapper.printPhotonsInScene( world.sceneBB, PhotonRNG.MAP_TYPE.GLOBAL );
				world.photonMapper.printPhotonsInScene( world.sceneBB, PhotonRNG.MAP_TYPE.CAUSTIC );
				world.beginpmPassTwo(); // mark to gather phtons
			}

			//time the render here...
			Stopwatch renderTimer = new Stopwatch();
			renderTimer.Start();

			float fpHeight = 6f; //smaller the more zoomed in
			float fpWidth = fpHeight;

			//pixel info
			float pixHeight = fpHeight / imageHeight;
			float pixWidth = fpWidth / imageWidth;

			//initialize default background color at all pixels to begin with
			byte[] pixColors = new byte[imageHeight * imageWidth * 3]; // this is what we return...

			//byte[] pixIllums = new byte[imageHeight * imageWidth * 3]; // this is the illuminance array....

			List<Color> pixIrrads = new List<Color>();
			List<Color> pixIllums = new List<Color>();

			// originally had (-fpHeight/2 + pixHeight/2.. was positive y upward...
			//focalLen + eyePoint.z if I want to move relative to my z
			Point fpPoint = new Point ( (-fpWidth / 2) + (pixWidth / 2), (fpHeight / 2) - (pixHeight / 2), focalLen + this.eyePoint.z); //gldrawPixels starts drawing lower-left corner at raster positions

			//Point fpPoint = new Point( (-fpWidth / 2) + (pixWidth / 2), (fpHeight / 2) - (pixHeight / 2), focalLen); //gldrawPixels starts drawing lower-left corner at raster positions
			LightRay fire = new LightRay( fpPoint - this.eyePoint , this.eyePoint );
			Color hitColor = null;
			Color hitIllum = null;

			byte[] hitColorArr = null;

			//modes
			bool isSuperSampling = false;
			bool photonOverlay = false;
			bool justPhotons = false;
			bool pathTrace = false;

			int hits = 0;
			for ( int y = 0; y < imageHeight; y++) // positive x ->, positive y V
			{
				for ( int x = 0; x < imageWidth; x++)
				{
					fire.direction = fpPoint - this.eyePoint;
					//supersample branch here... have an array of hitcolors... average them then pass to TR below
					if( doPM)
					{
						hitColor = Color.defaultBlack;
						hitColor += world.spawnRayPM( fire, 1 );	
					}

					else if(pathTrace) //my not so very good pathtracing implementation.
					{
						int samples = 1;
						hitColor = Color.defaultBlack;
						for (int sk = 0; sk < samples; sk++)
						{
							float randomX = world.photonMapper.randomRange( -pixWidth / 2f, pixWidth / 2f );
							float randomY = world.photonMapper.randomRange( -pixHeight / 2f, pixHeight / 2f );
							Vector randOffset = new Vector( randomX, randomY, 0f, false );
							fire.direction = (fpPoint + randOffset) - this.eyePoint;
							hitColor += world.spawnRayPath( fire, 1 );
						}
						hitColor = hitColor.scale( (float)1f / samples );
						if (hitColor.whiteOrHigher())
							Console.WriteLine( "Pathtrace sample output white or higher" );
					}

					else if (!isSuperSampling && !justPhotons)
						hitColor = world.spawnRay( fire, 1 ); //this will be irradiance.... CP5	

					else if (!isSuperSampling && justPhotons) //quick PM debug
						hitColor = renderPhotons( hitColor, fire, world, true );

					else
						hitColor = superSamplePixel( fpPoint, pixHeight, pixWidth, world );

					if (hitColor != null)
					{
						if (photonOverlay && !justPhotons) //if we're visualizing the Photon Maps
							hitColor = renderPhotons( hitColor, fire, world, false) ;

						// TODO run tone reproduction as we go -> after we calculated irradiances
						hitIllum = toCRTIllum( hitColor );
						pixIrrads.Add( hitColor );
						pixIllums.Add( hitIllum );

						if( !hitColor.Equals(Color.bgColor) ) hits++;
					}

					fpPoint.x += pixWidth;
				}
				//reset x to default position
				fpPoint.x = (-fpWidth / 2) + (pixWidth / 2);
				fpPoint.y -= pixHeight; // positive y is down
			}

			float rhKeyVal = float.MinValue;

			//Only set the passed rhPixel value if it's in a valid rangge of pixels to prevent Index out of bound error
			if (rhPixVal >= 0 && rhPixVal < (imageHeight * imageWidth))
				rhKeyVal = pixIllums[rhPixVal].colVal();

			//do tone reproduction all at once here.
			List<Color> trCols = runTRAll( pixIrrads, pixIllums, imageHeight, imageWidth, rhKeyVal );
			pixColors = colsToBytes( trCols, imageWidth,imageHeight );

			renderTimer.Stop();
			Console.WriteLine( "Rendering the scene took " + (renderTimer.ElapsedMilliseconds) + " milliseconds" );
			Console.WriteLine( $" There are {hits} non-background colors/ {imageHeight * imageWidth} colors total" );

			//pm debug
			if (doPM)
				pmDebug(world);

			if (photonOverlay || justPhotons)
				photOverlayInfo(world);

			return pixColors ;
		}

	}
}
