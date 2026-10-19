using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
        layout (location = 1) in vec4 vCol;

        out vec4 outCol;

        void main()
        {
            outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
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
            windowOptions.Title = "1. labhazi - kocka";
            windowOptions.Size = new Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        // Kiirja az osszes GL hibat, ami az elozo CheckError hivas ota keletkezett.
        // Azert kell ciklus, mert a Gl.GetError() egyszerre csak EGY hibat ad vissza,
        private static void CheckError(string hol)
        {
            GLEnum error = Gl.GetError();
            while (error != GLEnum.NoError)
            {
                Console.WriteLine($"[GL HIBA] {hol}: {error}");
                error = Gl.GetError();
            }
        }

        private static void GraphicWindow_Load()
        {
            // egyszeri beallitasok

            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);
            CheckError("inditas");

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            //vertex shader forditasa
            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));
            CheckError("vertex shader");

            //fragment shader forditasa
            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);
            // ez az ellenorzes uj: eredetileg csak a vertex shadert neztuk meg
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));
            CheckError("fragment shader");

            // program osszerakasa (linkeles)
            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            CheckError("shader program");
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // itt nincs GL hivas
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            // --- VAO ---
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);
            CheckError("vao");

            float[] vertexArray = new float[] {
                -0.69f, +0.4f, 0.0f,// bal fent
                +0.0f, 0.8f, 0.0f,//fent
                 0.69f, +0.4f, 0.0f,// jobb fent
                 0f, 0f, 0f,//kozep

                 0f, 0f, 0f,//kozep
                 -0.69f, 0.4f, 0.0f,//bal fent
                 -0.69f, -0.4f, 0.0f,//bal lent
                 0.0f,-0.8f, 0.0f,//lent

                 0.0f, 0.0f, 0.0f,//kozep
                 0.69f, 0.4f, 0.0f,//jobb fent
                 0.69f,-0.4f,0.0f,//jobb lent
                 0.0f,-0.8f,0.0f//lent

            };

            float[] colorArray = new float[] {
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
            };

            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3,
                4,5,6,
                4,6,7,
                8,9,10,
                8,10,11
            };

            // vertex buffer
            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);
            CheckError("vertex buffer");

            //color buffer
            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);
            CheckError("color buffer");

            //index buffer
            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            CheckError("index buffer");

            // program hasznalata
            Gl.UseProgram(program);
            CheckError("use program");

            // rajzolas
            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);
            CheckError("draw");

            // takaritas 
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);
            CheckError("torles");
        }
    }
}