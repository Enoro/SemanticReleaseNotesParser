using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace SemanticReleaseNotesParser.Core.Tests
{
    public class EnoroParserTests
    {
        private const string Escape_Category_Marker = @" - this is a list item escaping category 1\+2\+3 +new";
        [Fact]
        public void Ignore_Escaped_Category_Markers()
        {
            var releaseNote = SemanticReleaseNotesConverter.Parse(Escape_Category_Marker);

            // assert
            Assert.Equal(1, releaseNote.Items.FirstOrDefault().Categories.Count);
        }
    }
}
