using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Lab2megoldas
{
    internal static class Program
    {
        private static IWindow window;

        private static GL Gl;

        private static uint program;

        // kozos csucs es index buffer, ezt hasznalja mind a 27 kis kocka
        private static uint vertices;
        private static uint indices;
        private static uint indexArrayLength;

        // a 27 kis kocka (mindegyiknek sajat VAO-ja van)
        private static List<GlCube> cubes = new List<GlCube>();

        // es mindegyiknek a helye (model matrix)
        private static List<Matrix4X4<float>> cubeModelMatrices = new List<Matrix4X4<float>>();

        private const string ModelMatrixVariableName = "uModel";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
        layout (location = 1) in vec4 vCol;

        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;

        out vec4 outCol;

        void main()
        {
            outCol = vCol;
            gl_Position = uProjection*uView*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";

        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;

        in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "2. labor - Rubik kocka";
            windowOptions.Size = new Vector2D<int>(500, 500);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();
        }

        // Kiirja a GL hibakat (mint a lab1-ben)
        private static void CheckError(string hol)
        {
            GLEnum error = Gl.GetError();
            while (error != GLEnum.NoError)
            {
                Console.WriteLine($"[GL HIBA] {hol}: {error}");
                error = Gl.GetError();
            }
        }

        private static void Window_Load()
        {
            Gl = window.CreateOpenGL();
            Gl.ClearColor(System.Drawing.Color.LightGray);

            SetUpObjects();

            LinkProgram();

            Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);

            // a kamerat es a vetitest itt allitjuk be egyszer, fix ertekre
            Gl.UseProgram(program);
            SetViewMatrix();
            SetProjectionMatrix();
        }

        private static void LinkProgram()
        {
            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
            CheckError("shader program");
        }

        private static unsafe void SetUpObjects()
        {
            // Minden kis kockanak ugyanazok a csucsai es a haromszogei,
            // ezert ezeket csak egyszer adjuk meg es csak egyszer toltjuk fel.

            // counter clockwise is front facing
            float[] vertexArray = new float[] {
                // 1. lap: teteje
                -0.5f, 0.5f, 0.5f,
                0.5f, 0.5f, 0.5f,
                0.5f, 0.5f, -0.5f,
                -0.5f, 0.5f, -0.5f,

                // 2. lap: eleje
                -0.5f, 0.5f, 0.5f,
                -0.5f, -0.5f, 0.5f,
                0.5f, -0.5f, 0.5f,
                0.5f, 0.5f, 0.5f,

                // 3. lap: bal oldala
                -0.5f, 0.5f, 0.5f,
                -0.5f, 0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f, 0.5f,

                // 4. lap: alja
                -0.5f, -0.5f, 0.5f,
                0.5f, -0.5f, 0.5f,
                0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,

                // 5. lap: hatulja
                0.5f, 0.5f, -0.5f,
                -0.5f, 0.5f, -0.5f,
                -0.5f, -0.5f, -0.5f,
                0.5f, -0.5f, -0.5f,

                // 6. lap: jobb oldala
                0.5f, 0.5f, 0.5f,
                0.5f, 0.5f, -0.5f,
                0.5f, -0.5f, -0.5f,
                0.5f, -0.5f, 0.5f,
            };

            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3,

                4, 5, 6,
                4, 6, 7,

                8, 9, 10,
                10, 11, 8,

                12, 14, 13,
                12, 15, 14,

                17, 16, 19,
                17, 19, 18,

                20, 22, 21,
                20, 23, 22
            };

            vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);

            // Az indexeket is egy sima bufferbe toltjuk fel. Azt, hogy ez index buffer,
            // a kis kockak VAO-janal mondjuk meg (ElementArrayBuffer).
            indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, indices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            indexArrayLength = (uint)indexArray.Length;
            CheckError("kozos bufferek");

            // a Rubik kocka szinei
            float[] white = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] yellow = new float[] { 1.0f, 0.85f, 0.0f, 1.0f };
            float[] red = new float[] { 0.8f, 0.0f, 0.0f, 1.0f };
            float[] orange = new float[] { 1.0f, 0.45f, 0.0f, 1.0f };
            float[] blue = new float[] { 0.0f, 0.25f, 0.8f, 1.0f };
            float[] green = new float[] { 0.0f, 0.6f, 0.2f, 1.0f };
            float[] black = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };

            // 3 x 3 x 3 kis kocka, x, y, z erteke -1, 0 vagy 1
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        // eloszor minden lap fekete (ezek a belso lapok)
                        float[] face1Color = black; // teteje
                        float[] face2Color = black; // eleje
                        float[] face3Color = black; // bal oldala
                        float[] face4Color = black; // alja
                        float[] face5Color = black; // hatulja
                        float[] face6Color = black; // jobb oldala

                        // amelyik lap kifele nez, az szines lesz
                        if (y == 1)
                            face1Color = white;
                        if (z == 1)
                            face2Color = green;
                        if (x == -1)
                            face3Color = orange;
                        if (y == -1)
                            face4Color = yellow;
                        if (z == -1)
                            face5Color = blue;
                        if (x == 1)
                            face6Color = red;

                        GlCube cube = GlCube.CreateCubeWithFaceColors(Gl, vertices, indices, face1Color, face2Color, face3Color, face4Color, face5Color, face6Color);
                        cubes.Add(cube);

                        // a kis kockat a helyere toljuk; 1.1-el szorzunk, igy 0.1 hezag lesz a kockak kozott
                        Matrix4X4<float> modelMatrix = Matrix4X4.CreateTranslation(x * 1.1f, y * 1.1f, z * 1.1f);
                        cubeModelMatrices.Add(modelMatrix);
                    }
                }
            }
            CheckError("kis kockak");
        }

        private static void Window_Update(double deltaTime)
        {
            // NO GL calls
            // itt most nincs mit csinalni, a kocka nem mozog
        }

        private static unsafe void Window_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            // mind a 27 kis kockat kirajzoljuk, mindegyiket a sajat helyere
            for (int i = 0; i < cubes.Count; i++)
            {
                SetModelMatrix(cubeModelMatrices[i]);
                Gl.BindVertexArray(cubes[i].Vao);
                Gl.DrawElements(GLEnum.Triangles, indexArrayLength, GLEnum.UnsignedInt, null);
            }
            Gl.BindVertexArray(0);
        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            int location = Gl.GetUniformLocation(program, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError("model matrix");
        }

        private static unsafe void SetViewMatrix()
        {
            // a kamera jobbrol, felulrol es elolrol nez a kocka kozepere, igy 3 oldala latszik
            Vector3D<float> cameraPosition = new Vector3D<float>(5f, 4.5f, 7f);
            Vector3D<float> cameraTarget = new Vector3D<float>(0f, 0f, 0f);
            Vector3D<float> upVector = new Vector3D<float>(0f, 1f, 0f);

            var viewMatrix = Matrix4X4.CreateLookAt(cameraPosition, cameraTarget, upVector);
            int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError("view matrix");
        }

        private static unsafe void SetProjectionMatrix()
        {
            // az ablak 500 x 500-as (negyzet), ezert a kep aranya 1
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1f, 0.1f, 100f);
            int location = Gl.GetUniformLocation(program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ProjectionMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError("projection matrix");
        }

        private static void Window_Closing()
        {
            foreach (GlCube cube in cubes)
            {
                cube.ReleaseGlCube();
            }
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(indices);
            Gl.DeleteProgram(program);
        }
    }
}
