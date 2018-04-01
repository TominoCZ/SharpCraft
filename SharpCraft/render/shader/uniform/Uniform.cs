using System;

namespace SharpCraft.render.shader.uniform
{
	public abstract class Uniform<T>
	{
		protected readonly int Id;

		protected T Data;

		public Uniform(int id)
		{
			Id = id;
		}

		public void Update(T data)
		{
			if (Data.Equals(data)) return;
			Data = data;
			Upload();
		}

		protected abstract void Upload();
	}
}