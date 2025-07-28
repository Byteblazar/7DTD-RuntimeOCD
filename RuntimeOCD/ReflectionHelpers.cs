/*
 * RuntimeOCD
 * Copyright © 2025 Byteblazar <byteblazar@protonmail.com> * 
 * 
 * 
 * This file is part of RuntimeOCD.
 * 
 * RuntimeOCD is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * RuntimeOCD is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with RuntimeOCD. If not, see <https://www.gnu.org/licenses/>. 
 * 
*/

using System.Collections.Concurrent;
using System.Reflection;

namespace RuntimeOCD
{
	public static class ReflectionHelpers
	{
		private const BindingFlags All = BindingFlags.Instance
									   | BindingFlags.Static
									   | BindingFlags.Public
									   | BindingFlags.NonPublic;

		// Cache of (Type, MemberName) → MemberInfo (FieldInfo or PropertyInfo)
		private static readonly ConcurrentDictionary<(Type, string), MemberInfo?> MemberCache = new ConcurrentDictionary<(Type, string), MemberInfo?>();

		public static bool TryGetMemberValue<T>(this object obj, string memberName, out T value)
		{
			value = default!;
			if (obj == null) return false;

			var key = (obj.GetType(), memberName);
			var member = MemberCache.GetOrAdd(key, k =>
			{
				var fi = k.Item1.GetField(k.Item2, All);
				if (fi != null) return fi;
				var pi = k.Item1.GetProperty(k.Item2, All);
				return pi;
			});

			if (member is FieldInfo field)
			{
				var raw = field.GetValue(obj);
				if (raw is T tv) { value = tv; return true; }
			}
			else if (member is PropertyInfo prop && prop.CanRead)
			{
				var raw = prop.GetValue(obj);
				if (raw is T tv) { value = tv; return true; }
			}

			return false;
		}

		public static bool TryGetNestedMemberValue<T>(
			this object obj,
			string parentName,
			string childName,
			out T value)
		{
			value = default!;
			if (!obj.TryGetMemberValue<object>(parentName, out var parent))
				return false;

			return parent.TryGetMemberValue(childName, out value);
		}
	}

}
