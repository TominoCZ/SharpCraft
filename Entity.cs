using System;
using OpenTK;

namespace SharpCraft_Server
{
    internal class Entity
    {
        public Guid ID = Guid.NewGuid();

        public Vector3 Pos;
        public Vector3 LookVec;

        public void Update(Vector3 pos, Vector3 dir)
        {
            Pos = pos;

            LookVec = dir;
        }
    }
}