// --------------------------------------------------------------------------
//     Copyright (c) Microsoft Open Technologies, Inc.
//     All Rights Reserved. Licensed under the Apache 2.0 License.
// --------------------------------------------------------------------------
namespace MicrosoftOpenTech.PuppetProject
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    internal class ForgeJsonMetadata
    {

        [DataMember]
        internal string name;

        [DataMember]
        internal string version;

        [DataMember]
        internal string source = "UNKNOWN";

        [DataMember]
        internal string author;

        [DataMember]
        internal string license = "Apache License, Version 2.0";

        [DataMember]
        internal string summary;

        [DataMember]
        internal string description;

        [DataMember]
        internal string project_page = "UNKNOWN";

        [DataMember]
        internal List<Dependency> dependencies = new List<Dependency>();
        
        [DataMember]
        internal List<string> types = new List<string>();
        
        [DataMember]
        internal Dictionary<string, string> checksums = new Dictionary<string, string>();

        [DataContract]
        internal class Dependency
        {
            [DataMember]
            internal string name;

            [DataMember]
            internal string version_requirement;
        }
    }
}

/*
 * 
 json metadata example
 * 
 * {
  "name": "vlshch-puppetmodule5",
  "version": "0.1.1",
  "source": "UNKNOWN",
  "author": "vlshch",
  "license": "Apache License, Version 2.0",
  "summary": "",
  "description": "",
  "project_page": "UNKNOWN",
  "dependencies": [
    {
      "name": "puppetlabs/mysql",
      "version_requirement": "1.2.3"
    }
  ],
  "types": [

  ],
  "checksums": {
    "Modulefile": "9be8c61d304b569dcdb5ae7a5100e4a9",
    "README": "c6f8d9cf95dde13ed858fb6b724f9461",
    "manifests/manifest.pp": "d657ce3becc9d197aa85c3d187054808",
    "spec/spec_helper.rb": "a55d1e6483344f8ec6963fcb2c220372",
    "tests/manifest_test.pp": "c5ca20e56fccb197e33d991f47055206"
  }
}
 
 */
