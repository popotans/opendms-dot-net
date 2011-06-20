using System.Collections.Generic;

namespace OpenDMS.Storage.Data
{
    public class Version
    {
        /// <summary>
        /// Actions this <see cref="Version"/> currently has the required data to perform.
        /// </summary>
        [System.Flags]
        private enum ActionsType : int
        {
            /// <summary>
            /// Nothing (clear flags).
            /// </summary>
            None = 0x0,
            

            /// <summary>
            /// Deletes a version.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/> and revision.</remarks>
            Delete                              =   0x000010,

            /// <summary>
            /// Gets the HEAD information for the current revision of this version.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/>.</remarks>
            HeadCurrentRevision                 =   0x000100,
            /// <summary>
            /// Gets the HEAD information for a specific revision of this version.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/> and revision.</remarks>
            HeadSpecificRevision                =   0x000101,

            /// <summary>
            /// Gets the current revision of this version.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/>.</remarks>
            GetCurrentRevision                  =   0x001000,
            /// <summary>
            /// Gets a specific revision of this version.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/> and revision.</remarks>
            GetSpecificRevision                 =   0x001001,
            
            /// <summary>
            /// Create a new version without properties or content.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/>.</remarks>
            CreateWithoutPropertiesOrContent    =   0x010000,
            /// <summary>
            /// Create a new version with properties but without content.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/> and <see cref="Metadata"/>.</remarks>
            CreateWithPropertiesWithoutContent  =   0x010010,
            /// <summary>
            /// Create a new version without properties but with content.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/> and <see cref="Content"/>.</remarks>
            CreateWithoutPropertiesWithContent  =   0x010001,
            /// <summary>
            /// Create a new version with properties and content.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/>, <see cref="Metadata"/> and <see cref="Content"/>.</remarks>
            CreateWithPropertiesAndContent      =   CreateWithPropertiesWithoutContent | 
                                                    CreateWithoutPropertiesWithContent,


            /// <summary>
            /// Updates an existing version without properties or content.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/> and revision.</remarks>
            UpdateWithoutPropertiesOrContent    =   0x100000,
            /// <summary>
            /// Updates an existing version with properties but without content.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/>, revision and <see cref="Metadata"/>.</remarks>
            UpdateWithPropertiesWithoutContent  =   0x100001,
            /// <summary>
            /// Updates an existing version without properties but with content.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/>, revision and <see cref="Content"/>.</remarks>
            UpdateWithoutPropertiesWithContent  =   0x100010,
            /// <summary>
            /// Updates an existing version with properties and content.
            /// </summary>
            /// <remarks>Requires: <see cref="VersionId"/>, revision, <see cref="Metadata"/> and <see cref="Content"/>.</remarks>
            UpdateWithPropertiesAndContent      =   UpdateWithPropertiesWithoutContent | 
                                                    UpdateWithoutPropertiesWithContent

        }

        private ActionsType _availableActions = ActionsType.None;

        public VersionId VersionId { get; protected set; }
        public string Revision { get; protected set; }
        public Metadata Metadata { get; protected set; }
        public Content Content { get; protected set; }
        public List<Security.UsageRight> UsageRights { get; protected set; }

        public bool CanDelete { get { return _availableActions.HasFlag(ActionsType.Delete); } }
        public bool CanHeadCurrentRevision { get { return _availableActions.HasFlag(ActionsType.HeadCurrentRevision); } }
        public bool CanHeadSpecificRevision { get { return _availableActions.HasFlag(ActionsType.HeadSpecificRevision); } }
        public bool CanGetCurrentRevision { get { return _availableActions.HasFlag(ActionsType.GetCurrentRevision); } }
        public bool CanGetSpecificRevision { get { return _availableActions.HasFlag(ActionsType.GetSpecificRevision); } }
        public bool CanCreateWithoutPropertiesOrContent { get { return _availableActions.HasFlag(ActionsType.CreateWithoutPropertiesOrContent); } }
        public bool CanCreateWithPropertiesWithoutContent { get { return _availableActions.HasFlag(ActionsType.CreateWithPropertiesWithoutContent); } }
        public bool CanCreateWithoutPropertiesWithContent { get { return _availableActions.HasFlag(ActionsType.CreateWithoutPropertiesWithContent); } }
        public bool CanCreateWithPropertiesAndContent { get { return _availableActions.HasFlag(ActionsType.CreateWithPropertiesAndContent); } }
        public bool CanUpdateWithoutPropertiesOrContent { get { return _availableActions.HasFlag(ActionsType.UpdateWithoutPropertiesOrContent); } }
        public bool CanUpdateWithPropertiesWithoutContent { get { return _availableActions.HasFlag(ActionsType.UpdateWithoutPropertiesWithContent); } }
        public bool CanUpdateWithPropertiesAndContent { get { return _availableActions.HasFlag(ActionsType.UpdateWithPropertiesAndContent); } }


        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        /// <remarks>Enables actions: None</remarks>
        public Version()
            : this(null, null, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        /// <param name="versionId">The <see cref="VersionId"/>.</param>
        /// <param name="usageRights">The usage rights.</param>
        /// <remarks>Enables actions: <see cref="ActionsType.CreateWithoutPropertiesOrContent"/>, 
        ///     <see cref="ActionsType.HeadCurrentRevision"/> and 
        ///     <see cref="ActionsType.GetCurrentRevision"/>.
        /// </remarks>
        public Version(VersionId versionId, List<Security.UsageRight> usageRights)
            : this(versionId, null, null, null, usageRights)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        /// <param name="versionId">The <see cref="VersionId"/>.</param>
        /// <param name="revision">The revision.</param>
        /// <param name="usageRights">The usage rights.</param>
        /// <remarks>Enables actions: <see cref="ActionsType.Delete"/>, 
        ///     <see cref="ActionsType.HeadCurrentRevision"/>,
        ///     <see cref="ActionsType.HeadSpecificRevision"/>, 
        ///     <see cref="ActionsType.CreateWithoutPropertiesOrContent"/>,
        ///     <see cref="ActionsType.GetCurrentRevision"/>,
        ///     <see cref="ActionsType.GetSpecificRevision"/> and 
        ///     <see cref="ActionsType.UpdateWithoutPropertiesOrContent"/>.
        /// </remarks>
        public Version(VersionId versionId, string revision, List<Security.UsageRight> usageRights)
            : this(versionId, revision, null, null, usageRights)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        /// <param name="versionId">The <see cref="VersionId"/>.</param>
        /// <param name="metadata">The <see cref="Metadata"/>.</param>
        /// <param name="usageRights">The usage rights.</param>
        /// <remarks>Enables actions: <see cref="ActionsType.HeadCurrentRevision"/>, 
        ///     <see cref="ActionsType.GetCurrentRevision"/>,
        ///     <see cref="ActionsType.CreateWithoutPropertiesOrContent"/>,
        ///     <see cref="ActionsType.CreateWithPropertiesWithoutContent"/>.
        /// </remarks>
        public Version(VersionId versionId, Metadata metadata, List<Security.UsageRight> usageRights)
            : this(versionId, null, metadata, null, usageRights)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        /// <param name="versionId">The <see cref="VersionId"/>.</param>
        /// <param name="content">The <see cref="Content"/>.</param>
        /// <param name="usageRights">The usage rights.</param>
        /// <remarks>Enables actions: <see cref="ActionsType.HeadCurrentRevision"/>, 
        ///     <see cref="ActionsType.GetCurrentRevision"/>,
        ///     <see cref="ActionsType.CreateWithoutPropertiesOrContent"/>,
        ///     <see cref="ActionsType.CreateWithoutPropertiesWithContent"/>.
        /// </remarks>
        public Version(VersionId versionId, Content content, List<Security.UsageRight> usageRights)
            : this(versionId, null, null, content, usageRights)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        /// <param name="versionId">The <see cref="VersionId"/>.</param>
        /// <param name="metadata">The <see cref="Metadata"/>.</param>
        /// <param name="content">The <see cref="Content"/>.</param>
        /// <param name="usageRights">The usage rights.</param>
        /// <remarks>Enables actions: <see cref="ActionsType.HeadCurrentRevision"/>, 
        ///     <see cref="ActionsType.GetCurrentRevision"/>,
        ///     <see cref="ActionsType.CreateWithoutPropertiesOrContent"/>,
        ///     <see cref="ActionsType.CreateWithPropertiesWithoutContent"/>,
        ///     <see cref="ActionsType.CreateWithoutPropertiesWithContent"/>,
        ///     <see cref="ActionsType.CreateWithPropertiesAndContent"/>.
        /// </remarks>
        public Version(VersionId versionId, Metadata metadata, Content content, List<Security.UsageRight> usageRights)
            : this(versionId, null, metadata, content, usageRights)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        /// <param name="versionId">The <see cref="VersionId"/>.</param>
        /// <param name="revision">The revision.</param>
        /// <param name="metadata">The <see cref="Metadata"/>.</param>
        /// <param name="usageRights">The usage rights.</param>
        /// <remarks>Enables actions: <see cref="ActionsType.Delete"/>,
        ///     <see cref="ActionsType.HeadCurrentRevision"/>,
        ///     <see cref="ActionsType.HeadSpecificRevision"/>,
        ///     <see cref="ActionsType.GetCurrentRevision"/>,
        ///     <see cref="ActionsType.GetSpecificRevision"/>,
        ///     <see cref="ActionsType.CreateWithoutPropertiesOrContent"/>,
        ///     <see cref="ActionsType.CreateWithPropertiesWithoutContent"/>,
        ///     <see cref="ActionsType.UpdateWithoutPropertiesOrContent"/>,
        ///     <see cref="ActionsType.UpdateWithPropertiesWithoutContent"/>.
        /// </remarks>
        public Version(VersionId versionId, string revision, Metadata metadata, List<Security.UsageRight> usageRights)
            : this(versionId, revision, metadata, null, usageRights)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        /// <param name="versionId">The <see cref="VersionId"/>.</param>
        /// <param name="revision">The revision.</param>
        /// <param name="metadata">The <see cref="Content"/>.</param>
        /// <param name="usageRights">The usage rights.</param>
        /// <remarks>Enables actions: <see cref="ActionsType.Delete"/>, 
        ///     <see cref="ActionsType.HeadCurrentRevision"/>,
        ///     <see cref="ActionsType.HeadSpecificRevision"/>,
        ///     <see cref="ActionsType.GetCurrentRevision"/>,
        ///     <see cref="ActionsType.GetSpecificRevision"/>,
        ///     <see cref="ActionsType.CreateWithoutPropertiesOrContent"/>,
        ///     <see cref="ActionsType.CreateWithoutPropertiesWithContent"/>,
        ///     <see cref="ActionsType.UpdateWithoutPropertiesOrContent"/>,
        ///     <see cref="ActionsType.UpdateWithoutPropertiesWithContent"/>.
        /// </remarks>
        public Version(VersionId versionId, string revision, Content content, List<Security.UsageRight> usageRights)
            : this(versionId, revision, null, content, usageRights)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        /// <param name="versionId">The <see cref="VersionId"/>.</param>
        /// <param name="revision">The revision.</param>
        /// <param name="metadata">The <see cref="Metadata"/>.</param>
        /// <param name="content">The content.</param>
        /// <param name="usageRights">The usage rights.</param>
        /// <remarks>
        /// Enables actions: all
        /// </remarks>
        public Version(VersionId versionId, string revision, Metadata metadata, Content content, List<Security.UsageRight> usageRights)
        {
            VersionId = versionId;
            Revision = revision;
            Metadata = metadata;
            Content = content;
            UsageRights = usageRights;
        }

        /// <summary>
        /// Calculates the state of the this version and stores that in _availableActions.
        /// </summary>
        private void CalculateActionsState()
        {
            // Yes, I realize the following code could be more efficient, it was written this way so 
            // it would be half-way understandable.

            // Reset flags
            _availableActions = ActionsType.None;

            // No versionid means that we cannot do anything
            if (VersionId == null) return;
            
            if (string.IsNullOrEmpty(Revision))
            {
                // VersionId
                _availableActions |= ActionsType.CreateWithoutPropertiesOrContent |
                    ActionsType.HeadCurrentRevision |
                    ActionsType.GetCurrentRevision;
            }
            if (!string.IsNullOrEmpty(Revision))
            {
                // VersionId + Revision
                _availableActions |= ActionsType.Delete |
                    ActionsType.HeadSpecificRevision |
                    ActionsType.GetSpecificRevision | 
                    ActionsType.UpdateWithoutPropertiesOrContent;
            }
            if (Metadata != null)
            {
                // VersionId + Metadata
                _availableActions |= ActionsType.CreateWithPropertiesWithoutContent;
            }
            if (Content != null)
            {
                // VersionId + Content
                _availableActions |= ActionsType.CreateWithoutPropertiesWithContent;
            }
            if (Metadata != null && Content != null)
            {
                // VersionId + Metadata + Content
                _availableActions |= ActionsType.CreateWithPropertiesAndContent;
            }
            if (!string.IsNullOrEmpty(Revision) && Metadata != null)
            {
                // VersionId + Revision + Metadata
                _availableActions |= ActionsType.UpdateWithPropertiesWithoutContent;
            }
            if (!string.IsNullOrEmpty(Revision) && Content != null)
            {
                // VersionId + Revision + Content
                _availableActions |= ActionsType.UpdateWithoutPropertiesWithContent;
            }
            if (!string.IsNullOrEmpty(Revision) && Metadata != null && Content != null)
            {
                _availableActions |= ActionsType.UpdateWithPropertiesAndContent;
            }
        }
    }
}
