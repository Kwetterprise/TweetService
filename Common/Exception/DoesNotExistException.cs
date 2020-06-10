using System;
using System.Collections.Generic;
using System.Text;

namespace Kwetterprise.TweetService.Common.Exception
{
    using Exception = System.Exception;

    public class DoesNotExistException : Exception
    {
        public DoesNotExistException(string name, params Guid[] identifiers)
        {
            this.Name = name;
            this.Identifiers = identifiers;
        }

        private string Name { get; }

        public Guid[] Identifiers { get; }
    }
}
