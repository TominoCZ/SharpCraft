using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpCraft.entity;
using SharpCraft.item;
using SharpCraft.model;
using SharpCraft.util;
using SharpCraft.world;
using System;
using SharpCraft.json;

#pragma warning disable 618

namespace SharpCraft.block
{
    public class TileEntityCraftingGrid : TileEntity
    {
        private readonly BlockPos _pos;

        private readonly ItemStack[,] _grid = new ItemStack[3, 3];

        private ItemStack _product;

        private int _ticks;
        private int _ticksLast;
        private readonly int[,] _placeDelay = new int[3, 3];

        public bool HasResult => _product != null;

        public TileEntityCraftingGrid(World world, BlockPos pos) : base(world)
        {
            _pos = pos;
        }

        public override void ReadData(ByteBufferReader bbr)
        {
            ClearGrid();

            var count = bbr.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var x = bbr.ReadInt32();
                var y = bbr.ReadInt32();

                var meta = bbr.ReadInt32();
                var name = bbr.ReadString();

                _grid[x, y] = new ItemStack(ItemRegistry.GetItem(name), 1, (short)meta);
            }
        }

        public override void WriteData(ByteBufferWriter bbw)
        {
            var count = 0;

            for (var y = 0; y < _grid.GetLength(1); y++)
                for (var x = 0; x < _grid.GetLength(0); x++)
                    if (_grid[x, y] is ItemStack stack && !stack.IsEmpty)
                        count++;

            bbw.WriteInt32(count);

            for (var y = 0; y < _grid.GetLength(1); y++)
            {
                for (var x = 0; x < _grid.GetLength(0); x++)
                {
                    if (_grid[x, y] is ItemStack stack && !stack.IsEmpty)
                    {
                        bbw.WriteInt32(x);
                        bbw.WriteInt32(y);
                        bbw.WriteInt32(stack.Meta);
                        bbw.WriteString(stack.Item.UnlocalizedName);
                    }
                }
            }
        }

        public override void Update()
        {
            _ticksLast = _ticks;

            _ticks = (_ticks + 1) % 90;

            if (_ticksLast > _ticks)
                _ticksLast = _ticks - 1;

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (_placeDelay[x, y] > 0)
                        _placeDelay[x, y]--;
                }
            }

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
            float partialTime = _ticksLast + (_ticks - _ticksLast) * partialTicks;

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
                            var model = ib.Block.GetState(stack.Meta).Model;//JsonModelLoader.GetModelForBlock(ib.Block));

                            var mat = Matrix4.CreateTranslation(_pos.X + offX + offsetX, _pos.Y + 1, _pos.Z + offZ + offsetY);
                            var scale = Matrix4.CreateScale(grid);

                            GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TextureBlocks);

                            model.Bind();
                            model.Shader.SetMatrix4("transformationMatrix", scale * mat);
                            model.RawModel.Render();
                            model.Unbind();
                        }
                        else
                        {
                            var model = JsonModelLoader.GetModelForItem(stack.Item);

                            var rot = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(90));
                            var mat = rot * Matrix4.CreateTranslation(_pos.X + offX + offsetX + grid, _pos.Y + 1, _pos.Z + offZ + offsetY);
                            var scale = Matrix4.CreateScale(grid);

                            GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TextureItems);

                            model.Bind();
                            model.Shader.SetMatrix4("transformationMatrix", scale * mat);
                            model.RawModel.Render();
                            model.Unbind();
                        }
                    }
                }
            }

            if (_product != null && !_product.IsEmpty)
            {
                float offY = (float)((Math.Sin(partialTime / 90f * MathHelper.TwoPi) + 1) / 16);

                var rot = Matrix4.CreateRotationY(partialTime / 90f * MathHelper.TwoPi);

                if (_product.Item is ItemBlock ib)
                {
                    var mat = Matrix4.CreateTranslation(_pos.X + 0.5f, _pos.Y + 1.5f + offY, _pos.Z + 0.5f);
                    var scale = Matrix4.CreateTranslation(Vector3.One * -0.5f) * Matrix4.CreateScale(0.35f);
                    var model = ib.Block.GetState(_product.Meta).Model;

                    GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TextureBlocks);

                    model.Bind();
                    model.Shader.SetMatrix4("transformationMatrix", scale * rot * mat);
                    model.RawModel.Render();
                    model.Unbind();
                }
                else
                {
                    var mat = Matrix4.CreateTranslation(_pos.X + 0.5f, _pos.Y + 1.55f + offY, _pos.Z + 0.5f);
                    var scale = Matrix4.CreateTranslation(Vector3.One * -0.5f) * Matrix4.CreateScale(0.475f);
                    var model = JsonModelLoader.GetModelForItem(_product.Item);

                    GL.BindTexture(TextureTarget.Texture2D, JsonModelLoader.TextureItems);

                    model.Bind();
                    model.Shader.SetMatrix4("transformationMatrix", scale * rot * mat);
                    model.RawModel.Render();
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

        public void OnRightClicked(World world, Vector3 hitVec, ItemStack with, EntityPlayerSp clicked)
        {
            if (clicked.IsSneaking && _product != null)
            {
                ClearGrid();
                Save();

                var pos = new Vector3(_pos.X + 0.5f, hitVec.Y, _pos.Z + 0.5f);

                world.AddEntity(new EntityItem(world, pos, Vector3.UnitY * 0.25f, _product));
                _product = null;

                return;
            }

            if (IsEmpty() && clicked.IsSneaking)
                return;

            var gap = 1 / 16f;
            var slot = 1 / 4f;

            var localPos = new Vector2(hitVec.X, hitVec.Z) - new Vector2(_pos.X, _pos.Z);
            localPos.X %= 1.0f;
            localPos.Y %= 1.0f;

            var indexX = (int)(localPos.X / (slot + 4 * gap / 3));
            var indexY = (int)(localPos.Y / (slot + 4 * gap / 3));

            if (_placeDelay[indexX, indexY] > 0)
                return;

            _placeDelay[indexX, indexY] = 6;

            var item = _grid[indexX, indexY];

            var stackEmpty = with == null || with.IsEmpty;

            if (item != null)
            {
                SetGridElement(indexX, indexY, null);
                world?.AddEntity(new EntityItem(world, hitVec + Vector3.UnitY * 0.25f, Vector3.UnitY * 0.1f, item, true));

                //TODO - maybe make the items swap?
            }
            else
            {
                if (!stackEmpty)
                {
                    SetGridElement(indexX, indexY, with.Copy(1));
                    with.Count--;
                }
            }
        }

        private void SetGridElement(int x, int y, ItemStack stack)
        {
            _grid[x, y] = stack;

            Save();
        }

        public bool IsEmpty()
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (_grid[x, y] != null)
                        return false;
                }
            }

            return true;
        }

        private void ClearGrid()
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    _grid[x, y] = null;
                }
            }
        }

        private void Save()
        {
            World.SaveTileEntity(_pos);
        }
    }
}