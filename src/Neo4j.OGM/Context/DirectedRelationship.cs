using Neo4j.OGM.Annotations;

namespace Neo4j.OGM.Context
{
    internal class DirectedRelationship
    {
        private string _type;
        private RelationshipAttribute.DirectionEnum _direction;

        internal RelationshipAttribute.DirectionEnum Direction => _direction;

        public string Type => _type;

        public DirectedRelationship(string type, RelationshipAttribute.DirectionEnum direction)
        {
            _type = type;
            _direction = direction;
        }
    }
}
