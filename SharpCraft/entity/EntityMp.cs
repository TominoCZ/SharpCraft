using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.ES10;
using SharpCraft_Client.block;
using SharpCraft_Client.util;
using SharpCraft_Client.world;

namespace SharpCraft_Client.entity
{
    class EntityMp : Entity
    {
        //protected AxisAlignedBb BoundingBox, CollisionBoundingBox;

        //public World World;

        public Guid ID;

        public Vector3 PosServer;
        public Vector3 Dir;

        //public Vector3 Pos;
        //public Vector3 LastPos;

        //public bool IsAlive = true;

        public EntityMp(World world, Vector3 pos, Guid id) : base(world, pos)
        {
            ID = id;
            World = world;
            Pos = PosServer = LastPos = pos;

            CollisionBoundingBox = AxisAlignedBb.BlockFull;
        }

        public override void Update()
        {
            LastPos = Pos;

            Pos = PosServer;
        }

        public override void Render(float partialTicks)
        {
            var model = BlockRegistry.GetBlock<BlockRare>().GetState().Model;

            if (model == null)
                return;

            var lerpPos = Vector3.Lerp(LastPos, Pos, partialTicks);

            var mat = MatrixHelper.CreateTransformationMatrix(lerpPos - Vector3.One * 0.5f, Vector3.Zero, 1);

            model.Bind();

            model.Shader.SetMatrix4("transformationMatrix", mat);

            model.RawModel.Render();

            model.Unbind();
        }

        /*
        public AxisAlignedBb GetEntityBoundingBox()
        {
            return CollisionBoundingBox.Offset(Pos - CollisionBoundingBox.Size / 2);
        }

        public AxisAlignedBb GetCollisionBoundingBox()
        {
            return CollisionBoundingBox;
        }*/
    }
}
