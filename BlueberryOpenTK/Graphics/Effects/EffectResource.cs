using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blueberry.OpenGL
{
    internal class EffectResource
    {
        public static EffectResource BasicEffect { get; } = new EffectResource("Blueberry.OpenGL.Graphics.Effects.Resources.BasicEffect.glsl");
        public static EffectResource SpriteEffect { get; } = new EffectResource("Blueberry.OpenGL.Graphics.Effects.Resources.SpriteEffect.glsl");

        private readonly string path;
        private string fragment, vertex;

        public string Fragment
        {
            get
            {
                if (fragment == null)
                {
                    LoadData();
                }

                return fragment;
            }
        }

        public string Vertex
        {
            get
            {
                if (vertex == null)
                {
                    LoadData();
                }

                return vertex;
            }
        }

        private void LoadData()
        {
            var assembly = Assembly.GetAssembly(typeof(EffectResource));
            
            using (Stream stream = assembly.GetManifestResourceStream(path))
            using (StreamReader reader = new StreamReader(stream))
            {
                var data = reader.ReadToEnd();
                vertex = data.Replace("//$define", "#define VERTEX");
                fragment = data.Replace("//$define", "#define FRAGMENT");
            }
        }

        public EffectResource(string path)
        {
            this.path = path;
        }
    }
}
