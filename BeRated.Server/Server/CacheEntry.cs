﻿using System;

namespace BeRated.Server
{
	class CacheEntry
	{
		public DateTimeOffset Time { get; private set; }

		public string Markup { get; private set; }

		public CacheEntry(string markup)
		{
			Time = DateTimeOffset.Now;
			Markup = markup;
		}
	}
}
