using System;

using Xamarin.Interactive.Serialization;

namespace KitchenSinkIntegration
{
	public sealed class Person : ISerializableObject
	{
		public string Name { get; }

		public Person (string name)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));

			Name = name;
		}

		void ISerializableObject.Serialize (ObjectSerializer serializer)
			=> serializer.Property (nameof (Name), Name);
	}
}