using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePlayer
{
	/// <summary>
	/// Represent T but inherits EventArgs
	/// It is convertible to T.
	/// T is convertible to this.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SimpleArgs<T> : EventArgs
	{
		/// <summary>
		/// Just value
		/// </summary>
		public T Value { get; }
		public SimpleArgs(T value)
		{
			Value = value;
		}
		public static implicit operator T(SimpleArgs<T> args)
			=> args.Value;
		public static implicit operator SimpleArgs<T>(T value)
			=> new SimpleArgs<T>(value);
		public override string ToString()
		{
			return Value.ToString();
		}
	}
}
