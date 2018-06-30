using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.entity;
using SharpCraft.item;
using SharpCraft.model;
using SharpCraft.world;

namespace SharpCraft.block
{
    public class BlockCraftingTable : Block
    {
        public BlockCraftingTable() : base(Material.GetMaterial("wood"), "crafting_table")
        {
            CanBeInteractedWith = true;

            Hardness = 32;
        }

        public override void OnPlaced(World world, BlockPos pos, EntityPlayerSP placer)
        {
            world.AddTileEntity(pos, new TileEntityCraftingGrid(pos));
        }

        public override void OnRightClicked(MouseOverObject moo, EntityPlayerSP clicked)
        {
            if (moo.sideHit != FaceSides.Up)
                return;

            if (clicked.World.GetTileEntity(moo.blockPos) is TileEntityCraftingGrid tecg)
            {
                tecg.OnRightClicked(clicked.World, moo.hitVec, clicked.GetEquippedItemStack());
            }
        }
    }

    class TileEntityCraftingGrid : TileEntity
    {
        private BlockPos _pos;
        private ItemStack[,] _grid = new ItemStack[3, 3];

        private Item _product;

        public TileEntityCraftingGrid(BlockPos pos)
        {
            _pos = pos;
        }

        public override void Update()
        {
            Item[] table = new Item[9];

            for (var y = 0; y < _grid.GetLength(1); y++)
            {
                for (var x = 0; x < _grid.GetLength(0); x++)
                {
                    table[y * 3 + x] = _grid[x, y]?.Item;
                }
            }

            _product = RecipeRegistry.GetProduct(table);
        }

        public override void Render(float partialTicks)
        {
            var grid = 1 / 4f;
            var gap = 1 / 16f;

            for (int y = 0; y < _grid.GetLength(1); y++)
            {
                for (int x = 0; x < _grid.GetLength(0); x++)
                {
                    var stack = _grid[x, y];

                    var offX = grid * x;
                    var offZ = grid * y;

                    var offsetX = (x + 1) * gap;
                    var offsetY = (y + 1) * gap;

                    if (stack != null && !stack.IsEmpty)
                    {
                        if (stack.Item is ItemBlock ib)
                        {
                            var model = JsonModelLoader.GetModelForBlock(ib.UnlocalizedName);

                            var mat = Matrix4.CreateTranslation(_pos.X + offX + offsetX, _pos.Y + 1, _pos.Z + offZ + offsetY);
                            var scale = Matrix4.CreateScale(grid);

                            GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TEXTURE_BLOCKS);

                            model.Bind();
                            model.Shader.UpdateGlobalUniforms();
                            model.Shader.UpdateInstanceUniforms(scale * mat, model);
                            model.Shader.UpdateModelUniforms();
                            model.RawModel.Render(PrimitiveType.Quads);
                            model.Unbind();
                        }
                        else
                        {
                            var model = JsonModelLoader.GetModelForItem(stack.Item.UnlocalizedName);

                            var rot = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(90));
                            var mat = rot * Matrix4.CreateTranslation(_pos.X + offX + offsetX + grid, _pos.Y + 1, _pos.Z + offZ + offsetY);
                            var scale = Matrix4.CreateScale(grid);

                            GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TEXTURE_ITEMS);

                            model.Bind();
                            model.Shader.UpdateGlobalUniforms();
                            model.Shader.UpdateInstanceUniforms(scale * mat, model);
                            model.Shader.UpdateModelUniforms();
                            model.RawModel.Render(PrimitiveType.Quads);
                            model.Unbind();
                        }
                    }
                }
            }

            if (_product != null)
            {
                if (_product is ItemBlock ib)
                {
                    var model = JsonModelLoader.GetModelForBlock(ib.UnlocalizedName);

                    var mat = Matrix4.CreateTranslation(_pos.X + 0.5f, _pos.Y + 0.5f, _pos.Z + 0.5f);
                    var scale = Matrix4.CreateScale(0.5f);

                    GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TEXTURE_BLOCKS);

                    model.Bind();
                    model.Shader.UpdateGlobalUniforms();
                    model.Shader.UpdateInstanceUniforms(scale * mat, model);
                    model.Shader.UpdateModelUniforms();
                    model.RawModel.Render(PrimitiveType.Quads);
                    model.Unbind();
                }
                else
                {
                    var model = JsonModelLoader.GetModelForItem(_product.UnlocalizedName);

                    var mat = Matrix4.CreateTranslation(_pos.X + 0.5f, _pos.Y + 1.75f, _pos.Z + 0.5f);
                    var scale = Matrix4.CreateTranslation(Vector3.One * -0.5f) * Matrix4.CreateScale(0.75f);

                    GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TEXTURE_ITEMS);

                    model.Bind();
                    model.Shader.UpdateGlobalUniforms();
                    model.Shader.UpdateInstanceUniforms(scale * mat, model);
                    model.Shader.UpdateModelUniforms();
                    model.RawModel.Render(PrimitiveType.Quads);
                    model.Unbind();
                }
            }
        }

        public override void OnDestroyed(World world, BlockPos pos)
        {
            foreach (var stack in _grid)
            {
                world.AddEntity(new EntityItem(world, pos.ToVec() + Vector3.One * 0.5f, Vector3.UnitY * 0.25f, stack));
            }
        }

        public void OnRightClicked(World world, Vector3 hitVec, ItemStack with)
        {
            var gap = 1 / 16f;
            var slot = 1 / 4f;

            var localPos = new Vector2(hitVec.X, hitVec.Z) - new Vector2(_pos.X, _pos.Z);
            localPos.X %= 1.0f;
            localPos.Y %= 1.0f;

            var indexX = (int)(localPos.X / (slot + 4 * gap / 3));
            var indexY = (int)(localPos.Y / (slot + 4 * gap / 3));

            Console.WriteLine((hitVec.X < 0) + "," + (hitVec.Z < 0));
            Console.WriteLine(indexX + "," + indexY);

            var item = _grid[indexX, indexY];

            var stackEmpty = with == null || with.IsEmpty;

            if (item != null)
            {
                if (stackEmpty)
                {
                    _grid[indexX, indexY] = null;
                    world?.AddEntity(new EntityItem(world, hitVec + Vector3.UnitY * 0.25f, Vector3.UnitY * 0.1f, item));
                }
                else
                {
                    //TODO - maybe make the items swap?
                }
            }
            else
            {
                if (!stackEmpty)
                {
                    _grid[indexX, indexY] = with.Copy(1);
                    with.Count--;
                }
            }
        }
    }
}
