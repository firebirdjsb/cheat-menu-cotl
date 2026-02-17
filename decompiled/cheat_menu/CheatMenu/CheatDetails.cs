using System;

namespace CheatMenu
{
	public class CheatDetails : Attribute
	{
		public CheatDetails(string title, string description, bool isFlagCheat = false, int sortOrder = 0)
		{
			this._isFlagCheat = isFlagCheat;
			this._title = title;
			this._description = description;
			this._sortOrder = sortOrder;
		}

		public CheatDetails(string cheatTitle, string offTitle, string onTitle, string description, bool isFlagCheat = true, int sortOrder = 0)
		{
			if (!isFlagCheat)
			{
				throw new Exception("Multi name flag cheat can not have isFlagCheat set to false!");
			}
			this._onTitle = onTitle;
			this._offTitle = offTitle;
			this._description = description;
			this._title = cheatTitle;
			this._isMultiNameFlagCheat = true;
			this._isFlagCheat = true;
			this._sortOrder = sortOrder;
		}

		public virtual string Title
		{
			get
			{
				return this._title;
			}
		}

		public virtual string Description
		{
			get
			{
				return this._description;
			}
		}

		public virtual string OnTitle
		{
			get
			{
				return this._onTitle;
			}
		}

		public virtual string OffTitle
		{
			get
			{
				return this._offTitle;
			}
		}

		public virtual bool IsFlagCheat
		{
			get
			{
				return this._isFlagCheat;
			}
		}

		public virtual bool IsMultiNameFlagCheat
		{
			get
			{
				return this._isMultiNameFlagCheat;
			}
		}

		public virtual int SortOrder
		{
			get
			{
				return this._sortOrder;
			}
		}

		private readonly string _title;

		private readonly string _description;

		private readonly string _onTitle;

		private readonly string _offTitle;

		private readonly bool _isMultiNameFlagCheat;

		private readonly bool _isFlagCheat;

		private readonly int _sortOrder;
	}
}
