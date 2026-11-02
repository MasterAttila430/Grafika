using Silk.NET.OpenGL;

namespace Lab2megoldas
{
    internal class GlCube
    {
        public uint Vao { get; }
        public uint Colors { get; }

        private GL Gl;

        private GlCube(uint vao, uint colors, GL gl)
        {
            this.Vao = vao;
            this.Colors = colors;
            this.Gl = gl;
        }

        // A csucs buffert (vertices) es az index buffert (indices) a Program-ban egyszer hozzuk letre,
        // itt csak hasznaljuk oket. Minden kis kockanak csak a szin buffere sajat.
        public static unsafe GlCube CreateCubeWithFaceColors(GL Gl, uint vertices, uint indices, float[] face1Color, float[] face2Color, float[] face3Color, float[] face4Color, float[] face5Color, float[] face6Color)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            // csucsok: a kozos buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            // szinek: ez minden kockanal mas
            List<float> colorsList = new List<float>();
            colorsList.AddRange(face1Color);
            colorsList.AddRange(face1Color);
            colorsList.AddRange(face1Color);
            colorsList.AddRange(face1Color);

            colorsList.AddRange(face2Color);
            colorsList.AddRange(face2Color);
            colorsList.AddRange(face2Color);
            colorsList.AddRange(face2Color);

            colorsList.AddRange(face3Color);
            colorsList.AddRange(face3Color);
            colorsList.AddRange(face3Color);
            colorsList.AddRange(face3Color);

            colorsList.AddRange(face4Color);
            colorsList.AddRange(face4Color);
            colorsList.AddRange(face4Color);
            colorsList.AddRange(face4Color);

            colorsList.AddRange(face5Color);
            colorsList.AddRange(face5Color);
            colorsList.AddRange(face5Color);
            colorsList.AddRange(face5Color);

            colorsList.AddRange(face6Color);
            colorsList.AddRange(face6Color);
            colorsList.AddRange(face6Color);
            colorsList.AddRange(face6Color);

            float[] colorArray = colorsList.ToArray();

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            // haromszogek: a kozos index buffer
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            Gl.BindVertexArray(0);

            return new GlCube(vao, colors, Gl);
        }

        internal void ReleaseGlCube()
        {
            // csak a sajat dolgait torli, a kozos buffereket a Program torli
            Gl.DeleteBuffer(Colors);
            Gl.DeleteVertexArray(Vao);
        }
    }
}
