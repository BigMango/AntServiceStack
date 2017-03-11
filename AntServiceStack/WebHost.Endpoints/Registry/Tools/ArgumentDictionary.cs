using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;

namespace AntServiceStack.WebHost.Endpoints.Registry.Tools
{
	public class ArgumentDictionary : IDictionary, ICollection, IEnumerable
	{
		private Hashtable contents;

		public virtual int Count
		{
			get
			{
				return this.contents.Count;
			}
		}

		public virtual bool IsReadOnly
		{
			get
			{
				return this.contents.IsReadOnly;
			}
		}

		public virtual bool IsFixedSize
		{
			get
			{
				return this.contents.IsFixedSize;
			}
		}

		public virtual bool IsSynchronized
		{
			get
			{
				return this.contents.IsSynchronized;
			}
		}

		public virtual ICollection Keys
		{
			get
			{
				return this.contents.Keys;
			}
		}

		public virtual object SyncRoot
		{
			get
			{
				return this.contents.SyncRoot;
			}
		}

		public virtual string this[string key]
		{
			get
			{
				return this.GetArgument(key, 0);
			}
			set
			{
				this.Add(key, value);
			}
		}

		object IDictionary.this[object key]
		{
			get
			{
				return this[(string)key];
			}
			set
			{
				this.Add((string)key, (StringCollection)value);
			}
		}

		public virtual ICollection Values
		{
			get
			{
				return this.contents.Values;
			}
		}

		public ArgumentDictionary()
		{
			this.contents = new Hashtable();
		}

		public ArgumentDictionary(int length)
		{
			this.contents = new Hashtable(length);
		}

		public virtual void Add(string key, StringCollection values)
		{
			this.contents.Add(key.ToLower(CultureInfo.InvariantCulture), values);
		}

		void IDictionary.Add(object key, object values)
		{
			this.Add((string)key, (StringCollection)values);
		}

		public virtual void Add(string key, string value)
		{
			StringCollection stringCollection;
			if (!this.Contains(key))
			{
				stringCollection = new StringCollection();
				this.Add(key, stringCollection);
			}
			else
			{
				stringCollection = this.GetArguments(key);
			}
			stringCollection.Add(value);
		}

		public virtual void Clear()
		{
			this.contents.Clear();
		}

		public virtual bool Contains(string key)
		{
			return this.contents.ContainsKey(key.ToLower(CultureInfo.InvariantCulture));
		}

		bool IDictionary.Contains(object key)
		{
			return this.Contains((string)key);
		}

		public virtual bool ContainsValue(StringCollection value)
		{
			return this.contents.ContainsValue(value);
		}

		public virtual void CopyTo(Array array, int index)
		{
			this.contents.CopyTo(array, index);
		}

		public string GetArgument(string key, int valueNumber)
		{
			StringCollection arguments = this.GetArguments(key);
			if (arguments.Count != 0)
			{
				return arguments[valueNumber];
			}
			return string.Empty;
		}

		public StringCollection GetArguments(string key)
		{
			StringCollection stringCollection = (StringCollection)this.contents[key.ToLower(CultureInfo.InvariantCulture)];
			if (stringCollection != null)
			{
				return stringCollection;
			}
			return new StringCollection();
		}

		public virtual IDictionaryEnumerator GetEnumerator()
		{
			return this.contents.GetEnumerator();
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public virtual void Remove(string key)
		{
			this.contents.Remove(key.ToLower(CultureInfo.InvariantCulture));
		}

		void IDictionary.Remove(object key)
		{
			this.Remove((string)key);
		}
	}
}
