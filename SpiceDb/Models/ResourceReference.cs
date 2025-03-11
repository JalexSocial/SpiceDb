namespace SpiceDb.Models;

    /// <summary>
    /// Represents a reference to a resource or subject in the system.
    /// A reference is typically composed of a type and an identifier, and may optionally include a relation.
    /// This is analogous to the ObjectReference (and part of SubjectReference) in the core.proto.
    /// </summary>
    public class ResourceReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceReference"/> class from a string in the format "type:id".
        /// If the identifier contains a '#' character, the portion after it is treated as the relation.
        /// </summary>
        /// <param name="type">The resource type and id in the format "type:id".</param>
        /// <exception cref="ArgumentException">Thrown if the string does not have exactly two parts separated by a colon.</exception>
        public ResourceReference(string type)
        {
            var parts = type.Split(':');

            if (parts.Length != 2)
                throw new ArgumentException("Invalid permission key - must have two parts only has '" + type + "'");

            Type = parts[0];
            Id = parts[1];

            ProcessId();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceReference"/> class with separate type, identifier, and optional relation.
        /// </summary>
        /// <param name="type">The resource type.</param>
        /// <param name="id">The resource identifier.</param>
        /// <param name="optionalSubjectRelation">An optional relation for subject references.</param>
        public ResourceReference(string type, string id, string optionalSubjectRelation = "")
        {
            this.Type = type;
            this.Id = id;
            this.Relation = optionalSubjectRelation;

            ProcessId();
        }

        /// <summary>
        /// Gets or sets the resource or subject type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the resource or subject identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the optional relation (only applicable for subject references).
        /// </summary>
        public string Relation { get; set; } = string.Empty;

        private void ProcessId()
        {
            if (Id.Contains("#"))
            {
                var parts = Id.Split("#");
                Id = parts[0];
                Relation = parts[1];
            }
        }

        /// <summary>
        /// Returns a new <see cref="ResourceReference"/> with the specified subject relation applied.
        /// </summary>
        /// <param name="relation">The subject relation to set.</param>
        /// <returns>A new instance of <see cref="ResourceReference"/> with the updated relation.</returns>
        public ResourceReference WithSubjectRelation(string relation) => new ResourceReference(this.Type, this.Id, relation);

        /// <summary>
        /// Returns a new <see cref="ResourceReference"/> ensuring that the type has the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to ensure.</param>
        /// <returns>A new instance of <see cref="ResourceReference"/> with the prefixed type if not already present.</returns>
        public ResourceReference EnsurePrefix(string? prefix)
        {
            var type = this.Type;
            if (string.IsNullOrEmpty(prefix))
            {
                return new ResourceReference(type, this.Id, this.Relation);
            }
            type = string.IsNullOrEmpty(type) ? type : type.StartsWith(prefix + "/") ? type : $"{prefix}/{type}";

            if (type == this.Type)
                return this;

            return new ResourceReference(type, this.Id, this.Relation);
        }

        /// <summary>
        /// Returns a new <see cref="ResourceReference"/> with the specified prefix removed from the type.
        /// </summary>
        /// <param name="prefix">The prefix to remove.</param>
        /// <returns>A new instance of <see cref="ResourceReference"/> with the prefix excluded if present.</returns>
        public ResourceReference ExcludePrefix(string? prefix)
        {
            var type = this.Type;
            if (string.IsNullOrEmpty(prefix))
            {
                return new ResourceReference(type, this.Id, this.Relation);
            }

            if (!prefix.EndsWith("/")) prefix += "/";

            type = type.StartsWith(prefix) ? type.Substring(prefix.Length) : type;

            if (type == this.Type)
                return this;

            return new ResourceReference(type, this.Id, this.Relation);
        }

        /// <summary>
        /// Returns a string representation of the resource reference.
        /// Format: "type:id" or "type:id#relation" if a relation is present.
        /// </summary>
        public override string ToString() => $"{this.Type}:{this.Id}" + (string.IsNullOrEmpty(Relation) ? "" : $"#{Relation}");
    }