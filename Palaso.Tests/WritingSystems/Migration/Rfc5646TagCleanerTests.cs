﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Palaso.TestUtilities;
using Palaso.WritingSystems.Migration;

namespace Palaso.Tests.WritingSystems.Migration
{
	[TestFixture]
	public class Rfc5646TagCleanerTests
	{

		[Test]
		public void CompleteTagConstructor_HasInvalidLanguageName_MovedToPrivateUse()
		{
			var cleaner = new Rfc5646TagCleaner("234");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("x-234"));
		}

		[Test]
		public void CompleteTagConstructor_HasLanguageNameAndOtherName_OtherNameMovedToPrivateUse()
		{
			var cleaner = new Rfc5646TagCleaner("abc-123");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("abc-x-123"));
		}

		[Test]
		public void CompleteTagConstructor_LanguageNameWithAudio_GetZxxxAdded()
		{
			var cleaner = new Rfc5646TagCleaner("aaa-x-audio");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("aaa-Zxxx-x-audio"));
		}

		[Test]
		public void CompleteTagConstructor_InvalidLanguageNameWithScript_QaaAdded()
		{
			var cleaner = new Rfc5646TagCleaner("wee-Latn");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("qaa-Latn-x-wee"));
		}
	}
}