using System;

namespace CheatMenu
{
	public class StringEnum : Attribute
	{
		public StringEnum(string value)
		{
			this._value = value;
		}

		public virtual string Value
		{
			get
			{
				return this._value;
			}
		}

		private readonly string _value;
	}
}
