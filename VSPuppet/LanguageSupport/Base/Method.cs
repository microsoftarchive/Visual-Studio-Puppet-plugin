/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.

***************************************************************************/

namespace Puppet
{
    using System;
    using System.Collections.Generic;

    public struct Method
    {
        public string Name;
        public string Description;
        public string Type;
        public IList<Parameter> Parameters;
    }

    public struct Parameter
    {
        public string Name;
        public string Display;
        public string Description;
    }
}