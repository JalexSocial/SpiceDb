namespace SpiceDb.Api
{
    internal class SchemaParser
    {
        static List<SchemaEntity> _entities = new List<SchemaEntity>();

        public static IEnumerable<SchemaEntity> Parse(string schema)
        {
            var indx = 0;
            _entities = new List<SchemaEntity>();
            while (indx < schema.Length)
            {
                var remainingSchema = schema.Substring(indx);
                var newindx = ParseEntity(indx, remainingSchema);
                indx = indx + newindx;
            }

            return _entities;
        }

        static int ParseEntity(int _index, string text)
        {
            var tt = text;

            var startindex = tt.IndexOf("definition ");
            if (startindex == -1)
                return int.MaxValue;

            SchemaEntity entity = new SchemaEntity();

            var nameStartIndex = startindex + "definition ".Length;

            var nameLastIndex = tt.IndexOf("{", nameStartIndex);

            var name = tt.Substring(nameStartIndex, nameLastIndex - nameStartIndex).Trim();
            entity.ResourceType = name;

            var closingBracesIndex = tt.IndexOf("}", nameStartIndex);

            var defenation = tt.Substring(nameLastIndex, (closingBracesIndex - nameLastIndex) + 1);

            ReadRelationShip(defenation, entity);
            ReadPermissions(defenation, entity);

            _entities.Add(entity);

            return closingBracesIndex + 1;
        }

        static void ReadRelationShip(string txt, SchemaEntity entity)
        {
            var lines = txt.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {

                var startIndx = line.IndexOf("relation ");

                if (startIndx == -1)
                    continue;

                var startRelationNameIndx = startIndx + "relation ".Length;

                var arr = line.Substring(startRelationNameIndx).Split(':');

                var subjectEntities = arr[1].Trim().Split('|');

                foreach (var subjectEntity in subjectEntities)
                {
                    var relationship = new Relation(arr[0].Trim(), entity.ResourceType, subjectEntity, subjectEntity == entity.ResourceType);
                    entity.Relationships.Add(relationship);
                }
            }
        }

        static void ReadPermissions(string txt, SchemaEntity entity)
        {
            var lines = txt.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line == null)
                    continue;

                var startIndx = line!.IndexOf("permission ");

                if (startIndx == -1)
                    continue;

                var startRelationNameIndx = startIndx + "permission ".Length;

                var arr = line.Substring(startRelationNameIndx).Split('=');
                entity.Permissions.Add(new Permission(arr[0].Trim()));
            }
        }
    }

    public class SchemaEntity
    {
        public string ResourceType { get; set; } = string.Empty;

        public List<Relation> Relationships { get; set; } = new List<Relation>();

        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }

    public class Permission
    {
        public Permission(string name)
        {
            Name = name;
        }
        public string Name { get; private set; }
    }

    public class Relation
    {
        public Relation(string name, string resourceType, string subjectType, bool isSelfRelation)
        {
            Name = name;
            ResourceType = resourceType;
            SubjectType = subjectType;
            IsSelfRelation = isSelfRelation;
        }
        public string Name { get; private set; }
        public string ResourceType { get; private set; }
        public string SubjectType { get; private set; }

        public string SubjectTypeWithoutHash
        {
            get
            {
                return SubjectType.Split('#')[0].Trim();
            }
        }
        public bool IsSelfRelation { get; private set; }
    }

}
