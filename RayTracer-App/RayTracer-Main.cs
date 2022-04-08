using System;
using OpenGLDotNet;
using System.Runtime.InteropServices; //for GCHandle
using RayTracer_App.World;
using RayTracer_App.Camera;
using RayTracer_App.Scene_Objects;
using System.Collections.Generic;
using RayTracer_App.aux_classes;
//DOUBLE -> FLOAT

//https://matrix.reshish.com/multiplication.php 
public class RayTracerMain
{
	static int imageWidth;
	static int imageHeight;
	static IntPtr colsPtr;
	//static GCHandle colsHandle;
	static bool[] valid = new bool[1];
	static float[] rat = new float[4];

	public static Camera setupWhitted( World world, bool includeBunny = false )
	{
		//transparent middle sphere
		float s1Depth = 8.75f; //+z into the scene... I am IN LHS
		float s1Height = .75f; //1.75f.. 45 is good for lots of sky
		float s1Trans = 1.0f;
		float s1Refl = 0f;
		float s1RefIdx = .975f; // n2 < 1 for TIR

		//reflective right sphere
		float s2Depth = s1Depth + 1.75f; //1.85.. like Whitted... 2.75 for far apart
		float sphereRad = 1.5f;
		float s2Refl = 1.0f;
		float s2Trans = 0f;
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

		Point mainLightPos = new Point( .85f, -30.85f, s1Depth + .75f ); // the z was originally s1Depth + .75

		Color mainLightColor = Color.whiteSpecular;
		LightSource mainLight = new LightSource( mainLightPos, mainLightColor );

		//mandatory Whitted stuff
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
		//initialize objects
		float focalLen = 1.25f; //distance from camera to film plane center along N... //1.25

		World world = new World();

		// initialize camera and render world
		imageWidth = 1600;
		imageHeight = imageWidth;

		Camera cam = setupWhitted( world, false );

		// ditto with floats from 0-1 and 0-255, uint, now try byte
		byte[] pixColors = cam.render( world, imageHeight, imageWidth, focalLen );

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

