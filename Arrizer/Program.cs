using System;
using System.Diagnostics;
using System.Reflection;
using CodeGeneration.Roslyn;
using Arrizer.Attributes;

namespace Arrizer
{

    [SOA] // default suffix for new type - "SOA"
    [SOA("DifferentSuffix")]
    struct Particle
    {
        public float PositionX;
        public float PositionY;
        public float Rotation;

        public int Status;
    }

    class Program
    {

        static void Main(string[] args)
        {
            foreach (var type in typeof(Program).Assembly.GetTypes())
            {
                Console.WriteLine(type.FullName);
                var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach(var field in fields)
                {
                    Console.WriteLine("  - " + (field.IsPublic?"public":"private") + " " + field.FieldType.Name + " " + field.Name);
                }
            }

            Particle particle = new Particle();
            ParticleSOA particleSOA = new ParticleSOA();
            ParticleDifferentSuffix particleDifferrentSuffix = new ParticleDifferentSuffix();
        }
    }
}
