using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RuntimeOCD
{
	public sealed class ExplosionBuffsMerger : PropertyMergerBase
	{
		private static ExplosionBuffsMerger? _instance;
		private static readonly object _lock = new();

		public static ExplosionBuffsMerger Instance
		{
			get
			{
				if (_instance == null)
					lock (_lock)
						_instance ??= new ExplosionBuffsMerger();
				return _instance;
			}
		}

		private ExplosionBuffsMerger()
			: base(componentName: "ExplosionBuffs", targetPropertyName: "Explosion.Buffs", delimiter: ',')
		{ }

		public override string Name => "ExplosionBuffs";
	}
}
