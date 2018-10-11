using OpenTK.Graphics.OpenGL;
using System;

namespace SharpCraft_Client.render
{
    public class FBO
    {
        private int _textureId;

        private int _frameBuffer;
        private int _depthBuffer;

        private int _width, _height;

        private readonly bool _isTextureBuffer;
        private readonly int _samples;

        private int _colorBuffer = -1;

        public FBO(int w, int h, bool isTextureBuffer = false, int samples = 1)
        {
            _isTextureBuffer = isTextureBuffer;
            _samples = samples;

            SetSize(w, h);

            if (!Init())
            {
                Console.WriteLine("Failed to create FBO");
            }
        }

        private bool Init()
        {
            _frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBuffer);

            CreateTexture();

            if (_samples <= 1)
                CreateDepthBuffer();

            if (_isTextureBuffer)
            {
                // Set "renderedTexture" as our colour attachement #0
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, _textureId, 0);
            }

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            // Always check that our framebuffer is ok
            var b = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferComplete;

            BindDefault();

            return b;
        }

        public void SetSize(int w, int h)
        {
            if (w == _width && h == _height)
                return;

            Destroy();

            _width = w;
            _height = h;

            Init();
        }

        private void CreateTexture()
        {
            if (!_isTextureBuffer)
            {
                CreateRenderBuffer();

                return;
            }

            _textureId = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, _textureId);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                _width,
                _height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                (IntPtr)null);

            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int)TextureMagFilter.Nearest);
        }

        private void CreateDepthBuffer()
        {
            // The depth buffer
            _depthBuffer = GL.GenRenderbuffer();

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, _width, _height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthBuffer);
        }

        private void CreateRenderBuffer()
        {
            _colorBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _colorBuffer);

            if (_samples > 1)
                GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, _samples, RenderbufferStorage.DepthComponent32, _width, _height);
            else
                GL.RenderbufferStorage(RenderbufferTarget.RenderbufferExt, RenderbufferStorage.Rgba8, _width, _height);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, _colorBuffer);
        }

        public void CopyToScreen()
        {
            CopyToScreen(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void CopyToScreen(ClearBufferMask what)
        {
            CopyToScreen(what, BlitFramebufferFilter.Nearest);
        }

        public void CopyToScreen(ClearBufferMask what, BlitFramebufferFilter how)
        {
            //create();
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _frameBuffer);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BlitFramebuffer(0, 0, _width, _height, 0, 0, SharpCraft.Instance.Width, SharpCraft.Instance.Height, what, how);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void BindTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, _textureId);
        }

        public void Bind()
        {
            if (_isTextureBuffer)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBuffer);

            GL.Viewport(0, 0, _width, _height);
        }

        public void BindDefault()
        {
            if (_isTextureBuffer)
                GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, SharpCraft.Instance.Width, SharpCraft.Instance.Height);
        }

        public void Destroy()
        {
            if (_colorBuffer != -1)
            {
                GL.DeleteRenderbuffer(_colorBuffer);
                _colorBuffer = -1;
            }

            GL.DeleteFramebuffer(_frameBuffer);
            GL.DeleteTexture(_textureId);
        }
    }
}