﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class Resource : IResource
    {
        #region _rootElements

        static volatile Dictionary<ResourceType, string> _rootElements = new Dictionary<ResourceType, string>
        {
            { ResourceType.Unknown, "Service" },
            { ResourceType.Server, "Source" },
            { ResourceType.DbService, "Service" },
            { ResourceType.DbSource, "Source" },
            { ResourceType.PluginService, "Service" },
            { ResourceType.PluginSource, "Source" },
            { ResourceType.EmailSource, "Source" },
            { ResourceType.WebSource, "Source" },
            { ResourceType.WebService, "Service" },
            { ResourceType.WorkflowService, "Service" },
        };

        #endregion

        #region CTOR

        public Resource()
        {
            EnsureVersion();
        }

        public Resource(IResource copy)
        {
            ResourceID = copy.ResourceID;
            Version = copy.Version;
            ResourceName = copy.ResourceName;
            ResourceType = copy.ResourceType;
            ResourcePath = copy.ResourcePath;
            AuthorRoles = copy.AuthorRoles;
            FilePath = copy.FilePath;
            EnsureVersion();
        }

        public Resource(XElement xml)
        {
            if(xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            Guid resourceID;
            if(!Guid.TryParse(xml.AttributeSafe("ID"), out resourceID))
            {
                // This is here for legacy XML!
                resourceID = Guid.NewGuid();
                IsUpgraded = true;
            }
            ResourceID = resourceID;
            ResourceType = ParseResourceType(xml.AttributeSafe("ResourceType"));
            ResourceName = xml.AttributeSafe("Name");

            //if (ResourceName == "Emit ComplexType Service")
            //{
            //    string s = "";
            //}

            ResourcePath = xml.ElementSafe("Category");
            EnsureVersion(xml.AttributeSafe("Version"));
            AuthorRoles = xml.ElementSafe("AuthorRoles");
            LoadDependencies(xml);
            // This is here for legacy XML!
            if(ResourceType == ResourceType.Unknown)
            {
                #region Check source type

                var sourceTypeStr = xml.AttributeSafe("Type");
                enSourceType sourceType;
                if(Enum.TryParse(sourceTypeStr, out sourceType))
                {
                    switch(sourceType)
                    {
                        case enSourceType.Dev2Server:
                            ResourceType = ResourceType.Server;
                            IsUpgraded = true;
                            break;
                        case enSourceType.SqlDatabase:
                        case enSourceType.MySqlDatabase:
                            ResourceType = ResourceType.DbSource;
                            IsUpgraded = true;
                            break;
                        case enSourceType.Plugin:
                            ResourceType = ResourceType.PluginSource;
                            IsUpgraded = true;
                            break;
                    }
                }

                #endregion

                #region Check action type

                var actions = xml.Element("Actions");

                var action = actions != null ? actions.Descendants().FirstOrDefault() : xml.Element("Action");

                if(action != null)
                {
                    var actionTypeStr = action.AttributeSafe("Type");
                    ResourceType = GetResourceTypeFromString(actionTypeStr);
                    IsUpgraded = true;
                    //                    enActionType actionType;
                    //                    if(Enum.TryParse(actionTypeStr, out actionType))
                    //                    {
                    //                        switch(actionType)
                    //                        {
                    //                            case enActionType.InvokeStoredProc:
                    //                                ResourceType = ResourceType.DbService;
                    //                                IsUpgraded = true;
                    //                                break;
                    //                            case enActionType.Plugin:
                    //                                ResourceType = ResourceType.PluginService;
                    //                                IsUpgraded = true;
                    //                                break;
                    //                            case enActionType.Workflow:
                    //                                ResourceType = ResourceType.WorkflowService;
                    //                                IsUpgraded = true;
                    //                                break;
                    //                        }
                    //                    }
                }

                #endregion
            }
        }

        #endregion

        #region Properties

        [JsonIgnore]
        public bool IsUpgraded { get; set; }

        /// <summary>
        /// The resource ID that uniquely identifies the resource.
        /// </summary>   
        public Guid ResourceID { get; set; }

        /// <summary>
        /// The version that uniquely identifies the resource.
        /// </summary>
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// The display name of the resource.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the category of the resource.
        /// </summary>
        public string ResourcePath { get; set; }

        /// <summary>
        /// Gets or sets the file path of the resource.
        /// <remarks>
        /// Must only be used by the catalog!
        /// </remarks>
        /// </summary>   
        [JsonIgnore]
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the author roles.
        /// </summary>
        [JsonIgnore]
        public string AuthorRoles { get; set; }

        [JsonIgnore]
        public List<ResourceForTree> Dependencies { get; set; }

        #endregion

        #region GetResourceTypeFromString

        public ResourceType GetResourceTypeFromString(string actionTypeStr)
        {
            enActionType actionType;
            if(Enum.TryParse(actionTypeStr, out actionType))
            {
                switch(actionType)
                {
                    case enActionType.InvokeStoredProc:
                        return ResourceType.DbService;
                    case enActionType.Plugin:
                        return ResourceType.PluginService;
                    case enActionType.Workflow:
                        return ResourceType.WorkflowService;
                }
            }
            return ResourceType.Unknown;
        }

        #endregion

        //#region Save

        ///// <summary>
        ///// Saves the specified workspace ID - this is a no-op.
        ///// </summary>
        ///// <param name="workspaceID">The workspace ID.</param>
        //public virtual void Save(Guid workspaceID)
        //{
        //}

        //#endregion

        #region IsUserInAuthorRoles

        public bool IsUserInAuthorRoles(string userRoles)
        {
            if(string.IsNullOrEmpty(userRoles))
            {
                return false;
            }

            if(string.IsNullOrEmpty(AuthorRoles))
            {
                return true;
            }

            var user = userRoles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var res = AuthorRoles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            if(user.Contains("Domain Admins"))
            {
                return true;
            }

            if(!user.Any())
            {
                return false;
            }

            return res.Any() && user.Intersect(res).Any();
        }

        #endregion

        #region ToXml

        public virtual XElement ToXml()
        {
            EnsureVersion();
            return new XElement(_rootElements[ResourceType],
                new XAttribute("ID", ResourceID),
                new XAttribute("Version", Version.ToString()),
                new XAttribute("Name", ResourceName ?? string.Empty),
                new XAttribute("ResourceType", ResourceType),
                new XElement("DisplayName", ResourceName ?? string.Empty),
                new XElement("Category", ResourcePath ?? string.Empty),
                new XElement("AuthorRoles", AuthorRoles ?? string.Empty)
                );
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion

        #region ParseResourceType

        protected ResourceType ParseResourceType(string resourceTypeStr)
        {
            ResourceType resourceType;
            Enum.TryParse(resourceTypeStr, out resourceType);
            return resourceType;
        }

        #endregion

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IResource other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return ResourceID.Equals(other.ResourceID) && Version.Equals(other.Version);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param>
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((IResource)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (ResourceID.GetHashCode() * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Resource left, IResource right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Resource left, IResource right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region EnsureVersion

        void EnsureVersion(string versionStr = null)
        {
            if(Version == null)
            {
                Version version;
                Version = Version.TryParse(versionStr, out version) ? version : new Version(1, 0);
            }
        }

        #endregion

        #region UpgradeXml

        /// <summary>
        /// If this instance <see cref="IsUpgraded"/> then sets the ID, Version, Name and ResourceType attributes on the given XML.
        /// </summary>
        /// <param name="xml">The XML to be upgraded.</param>
        /// <returns>The XML with the additional attributes set.</returns>
        public XElement UpgradeXml(XElement xml)
        {
            if(IsUpgraded)
            {
                xml.SetAttributeValue("ID", ResourceID);
                xml.SetAttributeValue("Version", Version.ToString());
                xml.SetAttributeValue("Name", ResourceName ?? string.Empty);
                xml.SetAttributeValue("ResourceType", ResourceType);
            }
            return xml;
        }

        #endregion

        #region LoadDependencies

        public void LoadDependencies(XElement xml)
        {
            if(xml == null)
            {
                return;
            }

            var loadXml = xml.Descendants("XamlDefinition").ToList();
            if(loadXml.Count != 1)
            {
                return;
            }

            var textReader = new StringReader(loadXml[0].Value);
            var errors = new StringBuilder();
            try
            {
                var elementToUse = loadXml[0].HasElements ? loadXml[0] : XElement.Load(textReader, LoadOptions.None);
                var dependenciesFromXml = from desc in elementToUse.Descendants()
                                          where desc.Name.LocalName.Contains("DsfActivity") && desc.Attribute("UniqueID") != null
                                          select desc;
                var xElements = dependenciesFromXml as List<XElement> ?? dependenciesFromXml.ToList();
                var count = xElements.Count();
                if(count > 0)
                {
                    Dependencies = new List<ResourceForTree>();
                    xElements.ForEach(element =>
                    {
                        var uniqueIDAsString = element.AttributeSafe("UniqueID");
                        var resourceName = element.AttributeSafe("ServiceName");
                        var actionTypeStr = element.AttributeSafe("Type");
                        var resourceType = GetResourceTypeFromString(actionTypeStr);
                        Guid uniqueID;
                        Guid.TryParse(uniqueIDAsString, out uniqueID);
                        Dependencies.Add(CreateResourceForTree(uniqueID, resourceName, resourceType));
                    });
                }
            }
            catch(Exception e)
            {
                var resName = xml.AttributeSafe("Name");
                errors.AppendLine("Loading dependencies for [ " + resName + " ] caused " + e.Message);
            }
        }

        #endregion

        #region CreateResourceForTree

        static ResourceForTree CreateResourceForTree(Guid resourceID, string resourceName, ResourceType resourceType)
        {
            return new ResourceForTree
            {
                ResourceID = resourceID,
                ResourceName = resourceName,
                ResourceType = resourceType
            };
        }

        #endregion

        #region ParseProperties

        // PBI 5656 - 2013.05.20 - TWR - Refactored
        public static void ParseProperties(string s, Dictionary<string, string> properties)
        {
            if(s == null)
            {
                throw new ArgumentNullException("s");
            }
            if(properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            var props = s.Split(';');
            foreach(var p in props.Select(prop => prop.Split('=')).Where(p => p.Length >= 1))
            {
                var key = p[0];
                if(!properties.ContainsKey(key))
                {
                    continue;
                }
                properties[key] = string.Join("=", p.Skip(1));
            }
        }

        #endregion


    }
}
