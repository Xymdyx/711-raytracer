using System;
using OpenGLDotNet;
using System.Runtime.InteropServices; //for GCHandle
using RayTracer_App.World;
using RayTracer_App.Camera;
using RayTracer_App.Scene_Objects;
using System.Collections.Generic;
using RayTracer_App.aux_classes;
using RayTracer_App.Illumination_Models;
//DOUBLE -> FLOAT

//Booksmarks... Ctrl + K, Ctrl K to toogle...
// Ctrl + K + N for next..
// Ctrl + N + K for previous
//https://matrix.reshish.com/multiplication.php 
//https://antongerdelan.net/colour/
public class RayTracerMain
{
	static int imageWidth;
	static int imageHeight;
	static IntPtr colsPtr = default;
	//static GCHandle colsHandle;
	static bool[] valid = new bool[1];
	static float[] rat = new float[4];

	//Cornell scene for Photon Mapping
	public static Camera setupCornell( World world, bool includeBunny = false )
	{
		// THESE WEREN'T BEING DRAWN PAST THE FILM PLANE
		float cbXLim = 1.5f; //3
		float cbYLim = 1.5f; //3
		float cbZLim = 1.5f; //3

		//floor border param. 2 triangles.. draw along xz plane as in Whitted.. only difference between top and bottom is the y value
		Point botTL = new Point( -cbXLim, cbYLim, cbZLim );
		Point botTR = new Point( cbXLim, cbYLim, cbZLim );
		Point botBL = new Point( -cbXLim, cbYLim, -cbZLim );
		Point botBR = new Point( cbXLim, cbYLim, -cbZLim );

		List<Point> botVerts1 = new List<Point> { botTL, botBL, botTR };
		List<Point> botVerts2 = new List<Point> { botBR.copy(), botTR.copy(), botBL.copy() };

		Polygon botTri1 = new Polygon( botVerts1, Color.cbGrey, Phong.cornellPhong );
		Polygon botTri2 = new Polygon( botVerts2, Color.cbGrey, Phong.cornellPhong );

		//top border param. 2 triangles.. Color is pale grey. Shared w bot and rear
		Point topTL = new Point( -cbXLim, -cbYLim, cbZLim );
		Point topTR = new Point( cbXLim, -cbYLim, cbZLim );
		Point topBL = new Point( -cbXLim, -cbYLim, -cbZLim );
		Point topBR = new Point( cbXLim, -cbYLim, -cbZLim );

		//draw opposite of bottom to make normals face down!
		List<Point> topVerts1 = new List<Point> { topTR, topBL, topTL, };
		List<Point> topVerts2 = new List<Point> { topBL.copy() , topTR.copy(), topBR.copy() };

		Polygon topTri1 = new Polygon( topVerts1, Color.cbGrey, Phong.cornellPhong );
		Polygon topTri2 = new Polygon( topVerts2, Color.cbGrey, Phong.cornellPhong );

		//right border param. 2 triangles.. draw along the yz plane... only difference will be the x value. Color is red
		Point rightTL = new Point( cbXLim, -cbYLim, cbZLim );
		Point rightTR = new Point( cbXLim, cbYLim, cbZLim );
		Point rightBL = new Point( cbXLim, -cbYLim, -cbZLim );
		Point rightBR = new Point( cbXLim, cbYLim, -cbZLim );

		List<Point> rightVerts1 = new List<Point> { rightTR, rightBL, rightTL };
		List<Point> rightVerts2 = new List<Point> { rightBL.copy(), rightTR.copy(), rightBR.copy() };
		Polygon rightTri1 = new Polygon( rightVerts1, Color.cbBlue, Phong.cornellPhong );
		Polygon rightTri2 = new Polygon( rightVerts2, Color.cbBlue, Phong.cornellPhong );

		//left border param. 2 triangles... Color is blue
		Point leftTL = new Point( -cbXLim, -cbYLim, -cbZLim );
		Point leftTR = new Point( -cbXLim, -cbYLim, cbZLim );
		Point leftBL = new Point( -cbXLim, cbYLim, -cbZLim );
		Point leftBR = new Point( -cbXLim, cbYLim, cbZLim );

		List<Point> leftVerts1 = new List<Point> { leftTL, leftBL, leftTR };
		List<Point> leftVerts2 = new List<Point> { leftBR.copy(), leftTR.copy(), leftBL.copy() };
		Polygon leftTri1 = new Polygon( leftVerts1, Color.cbRed, Phong.cornellPhong );
		Polygon leftTri2 = new Polygon( leftVerts2, Color.cbRed, Phong.cornellPhong );

		//rear wall border param. 2 triangles... intersection of left&top, right&top, left&bot, right&bot at higher z
		//left border param. 2 triangles... Color is blue
		Point rearTL = new Point( -cbXLim, -cbYLim, cbZLim );
		Point rearTR = new Point( cbXLim, -cbYLim, cbZLim );
		Point rearBL = new Point( -cbXLim, cbYLim, cbZLim );
		Point rearBR = new Point( cbXLim, cbYLim, cbZLim );


		List<Point> rearVerts1 = new List<Point> { rearTL, rearBL, rearTR };
		List<Point> rearVerts2 = new List<Point> { rearBR.copy(), rearTR.copy(), rearBL.copy() };
		Polygon rearTri1 = new Polygon( rearVerts1, Color.cbGrey, Phong.cornellPhong );
		Polygon rearTri2 = new Polygon( rearVerts2, Color.cbGrey, Phong.cornellPhong );

		//front wall border param. 2 triangles... intersection of left&top, right&top, left&bot, right&bot at higher z
		//left border param. 2 triangles... Color is blue
		Point frontTL = new Point( -cbXLim, -cbYLim, -cbZLim );
		Point frontTR = new Point( cbXLim, -cbYLim, -cbZLim );
		Point frontBL = new Point( -cbXLim, cbYLim, -cbZLim );
		Point frontBR = new Point( cbXLim, cbYLim, -cbZLim );

		List<Point> frontVerts1 = new List<Point> { frontTR, frontBL, frontTL };
		List<Point> frontVerts2 = new List<Point> { frontBL.copy(), frontTR.copy(),frontBR.copy() };
		Polygon frontTri1 = new Polygon( frontVerts1, Color.cbGrey, Phong.cornellPhong );
		Polygon frontTri2 = new Polygon( frontVerts2, Color.cbGrey, Phong.cornellPhong );
		// finally make spheres
		//left sphere params
		float sphereRad = .7f; //1f

		float s1X = cbXLim - sphereRad;
		float s1Depth = cbZLim - sphereRad; //+z into the scene... I am IN LHS
		float s1Height = cbYLim - sphereRad; //1.75f.. 45 is good for lots of sky
		float s1Trans = 0f;
		float s1Refl = 1- s1Trans;
		float s1RefIdx = SceneObject.AIR_REF_INDEX; // ni > nt for TIR

		//right sphere param
		float s2X = -cbXLim + sphereRad;
		float s2Depth = s1Depth; //1.85.. like Whitted... 2.75 for far apart
		float s2Height = s1Height;
		float s2Refl = .45f;
		float s2Trans = 1 - s2Refl;
		float s2RefIdx = .955f;

		Sphere sphere1 = new Sphere( new Point( s1X, s1Height, s1Depth ), sphereRad, Color.cbChrome, s1Refl, s1Trans, s1RefIdx );
		Sphere sphere2 = new Sphere( new Point( s2X, s2Height, s2Depth ), sphereRad, Color.cbChrome, s2Refl, s2Trans, s2RefIdx );
		//sphere2.translate( 1.75f, s1Height, 0 ); //doing it here gives same results as after cam transform ...

		if (includeBunny)
		{
			float bunnyDepth = s1Depth - 10f; //s1Depth - 4f;
			Point bunnyOrigin = new Point( 1.25f, 0f, bunnyDepth );
			List<Polygon> bunnyTris = PlyParser.parseEdgePly( bunnyOrigin.toVec() );

			//bunny loop
			foreach (Polygon p in bunnyTris)
				world.addObject( p );
		}

		//place mainLight on top wall near its center
		Point ceilLightPos = new Point( 0f, -cbXLim + .005f, s1Depth - 1f ); ; // 0f, -cbYLim + .5f, 0f ... 0f, -2f, s1Depth - 1f
		Color ceilLightColor = Color.whiteSpecular;
		LightSource ceilLight = new LightSource( ceilLightPos, ceilLightColor );

		//add world stuff
		world.addLight( ceilLight );
		world.addObject( botTri1 );
		world.addObject( botTri2 );
		world.addObject( topTri1 );
		world.addObject( topTri2 );
		world.addObject( rightTri1 );
		world.addObject( rightTri2 );
		world.addObject( leftTri1 );
		world.addObject( leftTri2 );
		world.addObject( rearTri1 );
		world.addObject( rearTri2 );
		world.addObject( frontTri1 );
		world.addObject( frontTri2 );
		world.addObject( sphere1 );
		world.addObject( sphere2 );

		Vector up = new Vector( 0f, 1f, 0f );
		Point eyePos = new Point( 0f, .75f/2f, -2.75f/2f ); //0f, .5f, -2.75f... facing rear // facing frontBound 0f, .5f, -.75f
		Point lookAt = new Point( 0f, -1.5f/2f, cbZLim ); //0f, -1.5f, cbZLim...-cbZlim for rear
		Camera cam = new Camera( up, eyePos, lookAt ); //-z = backing up...

		return cam;
	}

	public static Camera setupWhitted( World world, bool includeBunny = false )
	{
		//transparent middle sphere
		float s1Depth = 8.75f; //+z into the scene... I am IN LHS
		float s1Height = .75f; //1.75f.. 45 is good for lots of sky
		float s1Trans = .85f;
		float s1Refl = 0f;
		float s1RefIdx = .955f; // ni > nt for TIR

		//reflective right sphere
		float s2Depth = s1Depth + 1.75f; //1.85.. like Whitted... 2.75 for far apart
		float sphereRad = 1.75f;
		float s2Refl = 1f;
		float s2Trans = 1- s2Refl;
		float s2RefIdx = SceneObject.AIR_REF_INDEX;

		float floorHeight = 5.5f;

		float bunnyDepth = s1Depth - 10f; //s1Depth - 4f;

		// THESE WEREN'T BEING DRAWN PAST THE FILM PLANE
		Point topLeft = new Point( -6f, floorHeight, 68.5f );
		Point topRight = new Point( 30f, floorHeight, 68.5f );
		Point topRight2 = new Point( 30f, floorHeight, 68.5f );
		Point bottomLeft = new Point( -6f, floorHeight, 2.0f );
		Point bottomLeft2 = new Point( -6f, floorHeight, 2.0f );
		Point bottomRight = new Point( 30f, floorHeight, 2.0f );

		topLeft.texCoord = new Point( 0, 0, 0 );
		topRight.texCoord = new Point( 1, 0, 0 );
		topRight2.texCoord = new Point( 1, 0, 0 );
		bottomLeft.texCoord = new Point( 0, 0, 1 );
		bottomLeft2.texCoord = new Point( 0, 0, 1 );
		bottomRight.texCoord = new Point( 1, 0, 1 );

		List<Point> triVerts1 = new List<Point> { topLeft, bottomLeft, topRight };
		List<Point> triVerts2 = new List<Point> { bottomRight, topRight2, bottomLeft2 };

		Polygon triangle1 = new Polygon( triVerts1 );
		Polygon triangle2 = new Polygon( triVerts2 );
		triangle1.translate( -5f, 0, 0 );
		triangle2.translate( -5f, 0, 0 );

		Sphere sphere1 = new Sphere( new Point( 0, s1Height, s1Depth ), sphereRad, s1Refl, s1Trans, s1RefIdx );
		Sphere sphere2 = new Sphere( new Point( 0, 0f, s2Depth ), sphereRad, s2Refl, s2Trans, s2RefIdx ); //setting the point elsewhere gives translating whole sphere
		sphere2.translate( 1.75f, s1Height + 1.4f, 0 ); //doing it here gives same results as after cam transform ... ( 1.75f, s1Height + 1.4f, 0 );
		//sphere1.translate( 1.75f, +2.0f, 0 ); //sphere 1 experiments

		//adv cp 1... parse Bunny
		if (includeBunny)
		{
			Point bunnyOrigin = new Point( 1.25f, 0f, bunnyDepth );
			List<Polygon> bunnyTris = PlyParser.parseEdgePly( bunnyOrigin.toVec() );

			//bunny loop
			foreach (Polygon p in bunnyTris)
				world.addObject( p );
		}

		//cp3... place mainLight source above the spheres 	// 1.5f, -1f, -5.0f //.85f, -30.85f, s1Depth - 5.5f , in front and way high

		Point mainLightPos = new Point( .85f, -30.85f, s1Depth + .75f ); // .85f, -30.85f, s1Depth + .75f

		Color mainLightColor = Color.whiteSpecular;
		LightSource mainLight = new LightSource( mainLightPos, mainLightColor );

		//mandatory whitted box
		world.addLight( mainLight );
		world.addObject( triangle1 );
		world.addObject( triangle2 );
		world.addObject( sphere2 );
		world.addObject( sphere1 );

		Vector up = new Vector( 0f, 1f, 0f );
		Point eyePos = new Point( 0f, -1f, -5f ); //0f, -1f, -5f
		Point lookAt = new Point( .5f, .5f, s1Depth + 1f );
		Camera cam = new Camera( up, eyePos, lookAt ); //-z = backing up...

		return cam;
	}

	//OPENGL DRAW CCW order matters. We are in LHS system. +y is down. +x is right. +z into screen. Row major. Postmultiply.
	//list triangles in CCW ORDER from the point containing the largest angle/ opposite of the hypotenuse!
	public static void doRayTracing()
	{
		float focalLen = 1.25f; //distance from camera to film plane center along N... //1.25, -1.25

		World world = new World();

		// initialize camera and render world
		imageWidth = 1600;
		imageHeight = imageWidth;

		/* PHOTON MAPPING TODO LIST (page 47 onwards in Jensen's 2008 notes) :
		* * make photon and pointKdTree classes (PM maps are kdTrees)
		* * make Russian roulette
		* * figure out how to shoot photons ( the points where photons land will be sent into the kdTree as splitting criterion)  //buggy right now
 		* * setup Cornell box scene with Whitted method
		* * figure out how to balance photons in kdTree as we go
		* * photon tracing
		* collect the k nearest photons and make calculation for global and caustic PMs
		* What is the tone reproduction formula?
		* figure out caustics and indirect illumination
		* implement a cone filter if ambitious
		*/

		//Camera cam = setupWhitted( world, false );
		Camera cam = setupCornell( world, false );

		// ditto with floats from 0-1 and 0-255, uint, now try byte
		byte[] pixColors = cam.render( world, imageHeight, imageWidth, focalLen, false, true );

		unsafe //this is how to work with pointers in C#
		{
			fixed (byte* colArrPtr = pixColors) { colsPtr = new IntPtr( (void*)colArrPtr ); }
		}

		return;

	}
	public static void display()
	{
		//put in display function
		GL.ClearColor( 0, 0, 0, 1 );
		GL.Clear( GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT );
		GL.RasterPos2i( -1, -1 );

		GL.PixelStorei( GL.GL_UNPACK_ALIGNMENT, 1);
		GL.GetBooleanv( GL.GL_CURRENT_RASTER_POSITION_VALID, valid );

		GL.GetFloatv( GL.GL_CURRENT_RASTER_POSITION, rat );
		Console.WriteLine( $" {rat[0]} , {rat[1]}, {rat[2]} is valid: {valid[0]}" ); //print out default { 0,0,0}"
		GL.DrawPixels( imageWidth, imageHeight, GL.GL_RGB, GL.GL_UNSIGNED_BYTE, colsPtr );

		FG.SwapBuffers();
		return;
	}

	static int Main( string [] args )
	{
		//	//camera lookat(position, center, up): [ 0.0, 2.0, -7.2], [0, -1.5, 0], [0, 1, 0]
		//	//camera perspective projection(vertical fov, aspect ratio, near, far)): [radians(90.0), 1.0, 1.0, 300.0]

		//	//foreground - sphere size:[5,5,5]
		//	//foreground - sphere location:[1,1.0, -1.0]

		//	//background - sphere size:[5,5,5]
		//	//background - sphere location:[-2.5, -1.75 , 2.0]

		//	//floor - cube size:[25, 2.5, 60]
		//	//floor - cube location:[ -2.0, -6.5, -0.5]

		doRayTracing();
		
		if (colsPtr == default)
		{
			Console.WriteLine( "Did not receive information to draw OpenGL pixels. Stopping now" );
			return -1;
		}

		//begin OpenGl
		int[] argc = new int[1]; argc[0] = 0; string[] argv = null;
		FG.Init( argc, argv );
		FG.InitDisplayMode( GLUT.GLUT_RGB | GLUT.GLUT_SINGLE | GLUT.GLUT_DEPTH );
		FG.InitWindowSize( imageWidth, imageHeight );
		FG.InitWindowPosition( 0, 0 );
		FG.CreateWindow( "RayTracing KdTree" );
		GL.Init( true );            //I forgot to call this...

		//fixed pixels being at a higher depth being in front of those with lower depth
		GL.Enable( GL.GL_DEPTH_TEST );
		GL.DepthFunc( GL.GL_LEQUAL );
		GL.FrontFace( GL.GL_CCW );
		GL.CullFace( GL.GL_BACK );

		GL.GetBooleanv( GL.GL_CURRENT_RASTER_POSITION_VALID, valid );

		FG.DisplayFunc( display ); //white screen without this
		FG.MainLoop(); //end

		return 0;
	}
}

